using System;
using System.Linq;
using System.Security.Claims;
using MadrasaSiddeeqiyahAPI.Data;
using MadrasaSiddeeqiyahAPI.DTOs;
using MadrasaSiddeeqiyahAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace MadrasaSiddeeqiyahAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthController(
            IConfiguration configuration,
            AppDbContext context,
            IPasswordHasher<User> passwordHasher)
        {
            _configuration = configuration;
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // ----------------------------------------------------
        // POST: api/Auth/login
        // Allows a user to authenticate and receive a JWT.
        // ----------------------------------------------------

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            if (loginDto == null ||
                string.IsNullOrWhiteSpace(loginDto.Username) ||
                string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return Unauthorized(new
                {
                    message = "Invalid username or password."
                });
            }

            var username = loginDto.Username.Trim();

            var user = _context.Users.SingleOrDefault(
                u => u.Username == username);

            // Return the same response for an unknown username
            // and an incorrect password.
            if (user == null ||
                string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return Unauthorized(new
                {
                    message = "Invalid username or password."
                });
            }

            var verificationResult =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    loginDto.Password);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    message = "Invalid username or password."
                });
            }

            // SuccessRehashNeeded is also a successful verification.
            // The password hasher can be used to upgrade the hash
            // when needed.
            if (verificationResult ==
                PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash =
                    _passwordHasher.HashPassword(
                        user,
                        loginDto.Password);

                _context.SaveChanges();
            }

            var token = GenerateJwtToken(
                user.Username,
                user.Role);

            return Ok(new { token });
        }

        // ----------------------------------------------------
        // POST: api/Auth/register
        // Only an authenticated administrator can create users.
        // ----------------------------------------------------

        [Authorize(Roles = "Admin")]
        [HttpPost("register")]
        public IActionResult Register(
            [FromBody] RegisterDto registerDto)
        {
            if (registerDto == null)
            {
                return BadRequest(new
                {
                    message = "Registration details are required."
                });
            }

            var username = registerDto.Username?.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new
                {
                    message = "Username is required."
                });
            }

            if (string.IsNullOrWhiteSpace(registerDto.Password) ||
                registerDto.Password.Length < 12)
            {
                return BadRequest(new
                {
                    message = "Password must be at least 12 characters long."
                });
            }

            var requestedRole =
                string.IsNullOrWhiteSpace(registerDto.Role)
                    ? "User"
                    : registerDto.Role.Trim();

            // Only the explicitly supported roles are accepted.
            if (!requestedRole.Equals(
                    "User",
                    StringComparison.OrdinalIgnoreCase) &&
                !requestedRole.Equals(
                    "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Role must be either User or Admin."
                });
            }

            var role = requestedRole.Equals(
                "Admin",
                StringComparison.OrdinalIgnoreCase)
                    ? "Admin"
                    : "User";

            if (_context.Users.Any(u => u.Username == username))
            {
                return Conflict(new
                {
                    message = "Username already exists."
                });
            }

            var user = new User
            {
                Username = username,
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    registerDto.Password);

            _context.Users.Add(user);
            _context.SaveChanges();

            return StatusCode(201, new
            {
                user.Username,
                user.Role
            });
        }

        // ----------------------------------------------------
        // GET: api/Auth/profile
        // Returns the authenticated user's basic profile.
        // ----------------------------------------------------

        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            var username =
                User.Identity?.Name ?? string.Empty;

            var role =
                User.FindFirst(ClaimTypes.Role)?.Value
                ?? string.Empty;

            return Ok(new
            {
                Username = username,
                Role = role
            });
        }

        // ----------------------------------------------------
        // Creates a signed JWT using secure configuration.
        // The token expires after 30 minutes.
        // ----------------------------------------------------

        private string GenerateJwtToken(
            string username,
            string role)
        {
            var jwtSettings =
                _configuration.GetSection("JwtSettings");

            var encodedKey = jwtSettings["Key"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            if (string.IsNullOrWhiteSpace(encodedKey) ||
                string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(audience))
            {
                throw new InvalidOperationException(
                    "JWT configuration is incomplete.");
            }

            byte[] key;

            try
            {
                key = Convert.FromBase64String(encodedKey);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException(
                    "JwtSettings:Key must be Base64 encoded.",
                    ex);
            }

            if (key.Length < 32)
            {
                throw new InvalidOperationException(
                    "JwtSettings:Key must contain at least 32 random bytes.");
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,

                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role)
                }),

                Expires = DateTime.UtcNow.AddMinutes(30),

                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler =
                new JwtSecurityTokenHandler();

            var token =
                tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
