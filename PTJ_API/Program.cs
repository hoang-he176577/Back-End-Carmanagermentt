using API.Json;
using API.Middlewares;
using Data.Repositories.Implementations;
using Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Models.Models;
using Service.Services.Implementations;
using Service.Services.Implementations.Repository;
using Service.Services.Interfaces;
using Service.Services.Interfaces.Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =============================
// Controllers + JSON
// =============================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập JWT token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
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

// =============================
// DbContext
// =============================
var connectionString = builder.Configuration.GetConnectionString("CarManager");

builder.Services.AddDbContext<CarManagerContext>(options =>
    options.UseSqlServer(connectionString));

// =============================
// Repositories
// =============================
builder.Services.AddScoped<IVehicleAssetRepository, VehicleAssetRepository>();
builder.Services.AddScoped<IVehicleAssetService, VehicleAssetService>();

// 🔥 ADD AUTH REPO
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
// 🔥 ADD AUTH SERVICE
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddHttpContextAccessor();

// =============================
// JWT CONFIG
// =============================
var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrEmpty(jwtSecret))
    throw new Exception("Jwt:Secret missing in appsettings");
if (Encoding.UTF8.GetByteCount(jwtSecret) < 32)
    throw new Exception("Jwt:Secret must be at least 32 bytes for HS256.");

var key = Encoding.UTF8.GetBytes(jwtSecret);
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CarManagement.API";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CarManagement.Client";

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
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userIdClaim =
                context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                context.Fail("Invalid token: user id not found.");
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<CarManagerContext>();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                context.Fail("User does not exist.");
                return;
            }

            if (user.DeletedAt != null)
            {
                context.Fail("Your account has been deactivated.");
            }
        },
        OnChallenge = context =>
        {
            if (!context.Handled)
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"success\":false,\"message\":\"Unauthorized\"}");
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// =============================
// BUILD APP
// =============================
var app = builder.Build();

// =============================
// MIDDLEWARE
// =============================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCustomExceptionHandler();

// 🔥 QUAN TRỌNG
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
