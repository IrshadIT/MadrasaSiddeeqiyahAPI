using MadrasaSiddeeqiyahAPI.Data;
using MadrasaSiddeeqiyahAPI.DTOs;
using MadrasaSiddeeqiyahAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace MadrasaSiddeeqiyahAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UsersController(AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public ActionResult<IEnumerable<UserReadDto>> GetUsers()
        {
            var users = _context.Users
                .Select(user => new UserReadDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                })
                .ToList();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public ActionResult<UserReadDto> GetUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new UserReadDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            });
        }

        [HttpPost]
        public ActionResult<UserReadDto> CreateUser([FromBody] UserCreateDto createDto)
        {
            if (_context.Users.Any(u => u.Username == createDto.Username))
            {
                return Conflict(new { message = "Username already exists." });
            }

            var user = new User
            {
                Username = createDto.Username,
                Role = string.IsNullOrWhiteSpace(createDto.Role) ? "User" : createDto.Role,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, createDto.Password);
            _context.Users.Add(user);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new UserReadDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            });
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] UserUpdateDto updateDto)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Role))
            {
                user.Role = updateDto.Role;
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Password))
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, updateDto.Password);
            }

            _context.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
