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
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Madrasa Siddeeqiyah API",
        Version = "v1",
        Description = "### 📚 Madrasa Siddeeqiyah Admission & Student Management API\n\n" +
                      "**Default Admin Credentials:**\n" +
                      "- **Username:** `admin`\n" +
                      "- **Password:** `Madrasa@2026`\n\n" +
                      "**How to test protected endpoints in Swagger:**\n" +
                      "1. Open `POST /api/Auth/login` below, click **Try it out**, and enter the credentials.\n" +
                      "2. Copy the `token` string returned in the response.\n" +
                      "3. Click the **Authorize 🔓** button at the top right of this Swagger page.\n" +
                      "4. Paste your token and click **Authorize**.\n" +
                      "5. All endpoints (Students, Users, Profile) will now send the Bearer header automatically!"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste your JWT Bearer token here to authenticate all protected requests."
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

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// Dependency Injection
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IFinanceService, FinanceService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<StudentCreateDtoValidator>();

// Tracing
var otlpEndpoint = builder.Configuration["OpenTelemetry:Otlp:Endpoint"];
builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Environment.ApplicationName ?? "MadrasaSiddeeqiyahAPI"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddConsoleExporter();

    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
    {
        tracerProviderBuilder.AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint);
        });
    }
});

// Authentication Configuration (Phase 1.3)
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "SuperSecretKeyForMadrasaSiddeeqiyah2026");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.Users.Any())
    {
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var adminUser = new User
        {
            Username = "admin",
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        };

        adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Madrasa@2026");
        db.Users.Add(adminUser);
        db.SaveChanges();
    }

    // Seed Campuses if empty
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

    // Seed Expenses if empty
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

    // Seed Donations if empty
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
