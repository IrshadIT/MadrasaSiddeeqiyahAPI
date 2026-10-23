using System;
using System.Linq;
using MadrasaSiddeeqiyahAPI.Data;
using MadrasaSiddeeqiyahAPI.Models;
using MadrasaSiddeeqiyahAPI.Services;
using MadrasaSiddeeqiyahAPI.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// 1. Controllers and Swagger
// ------------------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Madrasa Siddeeqiyah API",
        Version = "v1",
        Description =
            "### Madrasa Siddeeqiyah API\n\n" +
            "**Authentication:**\n" +
            "1. Call `POST /api/Auth/login` with an authorized account.\n" +
            "2. Copy the JWT token from the response.\n" +
            "3. Click **Authorize** in Swagger and enter the token.\n" +
            "4. Test endpoints permitted for your account's role."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT Bearer token to authenticate."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ------------------------------------------------------------
// 2. Database
// ------------------------------------------------------------

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Connection string 'DefaultConnection' is not configured.");
    }

    options.UseSqlServer(connectionString);
});

// ------------------------------------------------------------
// 3. CORS
// Configure deployed frontend origins using Cors:AllowedOrigins.
// Example appsettings entries should be the exact origins, with no path.
// ------------------------------------------------------------

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .GetChildren()
    .Select(section => section.Value)
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Cast<string>()
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod();

        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins);
        }
        else if (builder.Environment.IsDevelopment())
        {
            // Local development only. This does not allow arbitrary domains.
            policy.SetIsOriginAllowed(origin =>
                Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
                uri.Scheme == Uri.UriSchemeHttp &&
                (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                 uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)));
        }
        else
        {
            // In production, cross-origin browser access stays disabled until
            // explicit frontend origins are configured.
            policy.SetIsOriginAllowed(_ => false);
        }
    });
});

// ------------------------------------------------------------
// 4. Dependency injection
// ------------------------------------------------------------

builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IFinanceService, FinanceService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// ------------------------------------------------------------
// 5. FluentValidation
// ------------------------------------------------------------

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<StudentCreateDtoValidator>();

// ------------------------------------------------------------
// 6. OpenTelemetry tracing
// ------------------------------------------------------------

var otlpEndpoint = builder.Configuration["OpenTelemetry:Otlp:Endpoint"];

builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(builder.Environment.ApplicationName ?? "MadrasaSiddeeqiyahAPI"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddConsoleExporter();

    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
    {
        if (!Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var parsedOtlpEndpoint) ||
            (parsedOtlpEndpoint.Scheme != Uri.UriSchemeHttp &&
             parsedOtlpEndpoint.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                "OpenTelemetry:Otlp:Endpoint must be an absolute HTTP or HTTPS URI.");
        }

        tracerProviderBuilder.AddOtlpExporter(options =>
        {
            options.Endpoint = parsedOtlpEndpoint;
        });
    }
});

// ------------------------------------------------------------
// 7. JWT authentication configuration
// Requires JwtSettings:Key, JwtSettings:Issuer and
// JwtSettings:Audience from a secure configuration provider.
// ------------------------------------------------------------

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var encodedKey = jwtSettings["Key"];
var jwtIssuer = jwtSettings["Issuer"];
var jwtAudience = jwtSettings["Audience"];

if (string.IsNullOrWhiteSpace(encodedKey) ||
    string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "JWT configuration is incomplete. Configure " +
        "JwtSettings:Key, JwtSettings:Issuer, and JwtSettings:Audience.");
}

byte[] key;

try
{
    key = Convert.FromBase64String(encodedKey);
}
catch (FormatException ex)
{
    throw new InvalidOperationException(
        "JwtSettings:Key must be a Base64-encoded key.", ex);
}

if (key.Length < 32)
{
    throw new InvalidOperationException(
        "JwtSettings:Key must contain at least 32 random bytes.");
}

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// ------------------------------------------------------------
// 8. Build the application
// ------------------------------------------------------------

var app = builder.Build();

// ------------------------------------------------------------
// 9. Database migration and initial data
// The bootstrap administrator is created only if the Users table is empty.
// Set AdminBootstrap:Username and AdminBootstrap:Password securely for a
// brand-new database. This application intentionally has no password-reset
// code in Program.cs.
// ------------------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // For production deployments, consider applying migrations through a
    // controlled deployment step instead of automatically at application startup.
    db.Database.Migrate();

    if (!db.Users.Any())
    {
        var adminUsername = app.Configuration["AdminBootstrap:Username"];
        var adminPassword = app.Configuration["AdminBootstrap:Password"];

        if (string.IsNullOrWhiteSpace(adminUsername) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "No users exist. Configure AdminBootstrap:Username and " +
                "AdminBootstrap:Password using a secure configuration provider.");
        }

        if (adminPassword.Length < 12)
        {
            throw new InvalidOperationException(
                "AdminBootstrap:Password must be at least 12 characters long.");
        }

        var passwordHasher = scope.ServiceProvider
            .GetRequiredService<IPasswordHasher<User>>();

        var adminUser = new User
        {
            Username = adminUsername.Trim(),
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        };

        adminUser.PasswordHash =
            passwordHasher.HashPassword(adminUser, adminPassword);

        db.Users.Add(adminUser);
        db.SaveChanges();
    }

    // --------------------------------------------------------
    // 9A. Seed campuses if none exist
    // These are illustrative/demo values. Verify them before real use.
    // --------------------------------------------------------

    if (!db.Campuses.Any())
    {
        db.Campuses.AddRange(
            new Campus
            {
                Name = "Main Campus - Hifz & Aalim Academy",
                BranchCode = "MS-HYD-01",
                Address = "Opp. Grand Mosque, Shah Ali Banda",
                City = "Hyderabad",
                State = "Telangana",
                Latitude = 17.3616,
                Longitude = 78.4747,
                GoogleMapEmbedUrl = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d15231.737119106093!2d78.4650567!3d17.3616428!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x3bcb978a3c8c7ad3%3A0x6b2b2d69f06e33e4!2sCharminar%2C%20Hyderabad!5e0!3m2!1sen!2sin!4v1715000000000",
                StudentCapacity = 250,
                CurrentStudents = 185,
                AnnualBudget = 1800000m,
                TotalSpent = 540700m,
                TotalReceived = 820000m,
                InchargePerson = "Maulana Abdul Qadeer Qasmi",
                ContactPhone = "+91 9849012345"
            },
            new Campus
            {
                Name = "Shuba-e-Aalima Girls Wing & Maktab",
                BranchCode = "MS-SEC-02",
                Address = "Near Eidgah Ground, Kalasiguda",
                City = "Secunderabad",
                State = "Telangana",
                Latitude = 17.4399,
                Longitude = 78.4983,
                GoogleMapEmbedUrl = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d15224.288219430164!2d78.488056!3d17.439928!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x3bcb9a24c5555555%3A0x1111111111111111!2sSecunderabad!5e0!3m2!1sen!2sin!4v1715000000000",
                StudentCapacity = 160,
                CurrentStudents = 130,
                AnnualBudget = 1000000m,
                TotalSpent = 310000m,
                TotalReceived = 450000m,
                InchargePerson = "Muallima Fatima Siddeeqi",
                ContactPhone = "+91 9849067890"
            },
            new Campus
            {
                Name = "Rural Deeniyat & Quran Centre",
                BranchCode = "MS-RR-03",
                Address = "Vikarabad Highway, Chevella",
                City = "Ranga Reddy",
                State = "Telangana",
                Latitude = 17.3090,
                Longitude = 78.1360,
                GoogleMapEmbedUrl = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d30472.93291888496!2d78.1250!3d17.3090!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x3bc9785633333333%3A0x2222222222222222!2sChevella!5e0!3m2!1sen!2sin!4v1715000000000",
                StudentCapacity = 100,
                CurrentStudents = 78,
                AnnualBudget = 700000m,
                TotalSpent = 175000m,
                TotalReceived = 260000m,
                InchargePerson = "Hafiz Mohammed Zubair",
                ContactPhone = "+91 9849098765"
            }
        );

        db.SaveChanges();
    }

    // --------------------------------------------------------
    // 9B. Seed sample expenses if none exist
    // --------------------------------------------------------

    if (!db.Expenses.Any())
    {
        db.Expenses.AddRange(
            new Expense
            {
                Title = "Monthly Grocery & Food Ration (Rice, Lentils, Oil, Wheat)",
                Category = "Food & Kitchen",
                Amount = 145000m,
                Date = DateTime.UtcNow.AddDays(-3),
                SpentBy = "Kitchen Supervisor - Hafiz Bilal",
                ReceiptNumber = "VCH-2026-101",
                Description = "Bulk monthly wholesale ration for 185 boarding hostel students at Main Campus.",
                CampusName = "Main Campus - Hifz & Aalim Academy"
            },
            new Expense
            {
                Title = "Asatizah & Staff Monthly Honorarium (14 Members)",
                Category = "Salaries & Honorarium",
                Amount = 240000m,
                Date = DateTime.UtcNow.AddDays(-8),
                SpentBy = "Treasurer - Brother Imran",
                ReceiptNumber = "VCH-2026-102",
                Description = "Monthly stipends disbursed to Quran teachers, Qaris, and administrative staff.",
                CampusName = "Main Campus - Hifz & Aalim Academy"
            },
            new Expense
            {
                Title = "Holy Quran & Dars-e-Nizami Kitabs Sets",
                Category = "Education & Books",
                Amount = 38500m,
                Date = DateTime.UtcNow.AddDays(-12),
                SpentBy = "Head Librarian - Maulana Farhan",
                ReceiptNumber = "VCH-2026-103",
                Description = "New sets of 30 Para Tajweedi Qurans, Noorani Qaidahs, and Mishkat Sharif text books.",
                CampusName = "Main Campus - Hifz & Aalim Academy"
            },
            new Expense
            {
                Title = "Girls Wing Educational Supplies & Uniforms",
                Category = "Education & Books",
                Amount = 32000m,
                Date = DateTime.UtcNow.AddDays(-15),
                SpentBy = "Muallima Fatima",
                ReceiptNumber = "VCH-2026-104",
                Description = "Notebooks, stationery, and abayas for underprivileged girl students.",
                CampusName = "Shuba-e-Aalima Girls Wing & Maktab"
            },
            new Expense
            {
                Title = "Rooftop Solar Plant Inverter & Electrical Overhaul",
                Category = "Utilities & Maintenance",
                Amount = 28500m,
                Date = DateTime.UtcNow.AddDays(-18),
                SpentBy = "Campus Maintenance Dept",
                ReceiptNumber = "VCH-2026-105",
                Description = "Preventative battery water top-up and solar inverter repairs to ensure 24/7 power for study halls.",
                CampusName = "Main Campus - Hifz & Aalim Academy"
            },
            new Expense
            {
                Title = "Student Health Camp & Emergency Medical Kit",
                Category = "Healthcare & Medical",
                Amount = 14200m,
                Date = DateTime.UtcNow.AddDays(-22),
                SpentBy = "Resident Doctor Panel",
                ReceiptNumber = "VCH-2026-106",
                Description = "Seasonal flu medications, vitamins, first-aid replenishments, and dental check-up camp.",
                CampusName = "Main Campus - Hifz & Aalim Academy"
            },
            new Expense
            {
                Title = "Hostel Water Purifier (RO Plant) Filter Replacement",
                Category = "Hostel & Facilities",
                Amount = 18200m,
                Date = DateTime.UtcNow.AddDays(-25),
                SpentBy = "Hostel Warden",
                ReceiptNumber = "VCH-2026-107",
                Description = "RO membranes and sediment filter cartridges replacement for clean drinking water.",
                CampusName = "Main Campus - Hifz & Aalim Academy"
            },
            new Expense
            {
                Title = "Rural Branch Classroom Ceiling Fans & Whiteboards",
                Category = "Infrastructure & Repairs",
                Amount = 24300m,
                Date = DateTime.UtcNow.AddDays(-28),
                SpentBy = "Hafiz Zubair",
                ReceiptNumber = "VCH-2026-108",
                Description = "4 heavy-duty ceiling fans and 2 magnetic whiteboards for Chevella branch.",
                CampusName = "Rural Deeniyat & Quran Centre"
            }
        );

        db.SaveChanges();
    }

    // --------------------------------------------------------
    // 9C. Seed sample donations if none exist
    // --------------------------------------------------------

    if (!db.Donations.Any())
    {
        db.Donations.AddRange(
            new Donation
            {
                DonorName = "Haji Abdul Wahid & Family",
                Amount = 250000m,
                Date = DateTime.UtcNow.AddDays(-5),
                Purpose = "Zakat (Student Ration & Education)",
                ReceiptNumber = "DON-2026-501",
                CampusName = "Main Campus - Hifz & Aalim Academy"
            },
            new Donation
            {
                DonorName = "Anonymous Well-Wisher",
                Amount = 180000m,
                Date = DateTime.UtcNow.AddDays(-9),
                Purpose = "Sadaqah Jariyah (Solar & Infrastructure)",
                ReceiptNumber = "DON-2026-502",
                CampusName = "Main Campus - Hifz & Aalim Academy"
            },
            new Donation
            {
                DonorName = "Community Food Drive Contributors",
                Amount = 120000m,
                Date = DateTime.UtcNow.AddDays(-14),
                Purpose = "Student Meal Sponsorship (Iftaar & Daily Food)",
                ReceiptNumber = "DON-2026-503",
                CampusName = "Main Campus - Hifz & Aalim Academy"
            },
            new Donation
            {
                DonorName = "Al-Madinah Trust Welfare Committee",
                Amount = 200000m,
                Date = DateTime.UtcNow.AddDays(-20),
                Purpose = "Girls Wing Education Endowment",
                ReceiptNumber = "DON-2026-504",
                CampusName = "Shuba-e-Aalima Girls Wing & Maktab"
            },
            new Donation
            {
                DonorName = "Dr. Tariq & Friends",
                Amount = 150000m,
                Date = DateTime.UtcNow.AddDays(-27),
                Purpose = "Healthcare & Clean Water Plant",
                ReceiptNumber = "DON-2026-505",
                CampusName = "Rural Deeniyat & Quran Centre"
            }
        );

        db.SaveChanges();
    }
}

// ------------------------------------------------------------
// 10. HTTP request pipeline
// ------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS redirection is enabled outside local Development to avoid the
// missing-HTTPS-port warning when launchSettings.json only uses HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
