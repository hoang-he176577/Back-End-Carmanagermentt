using API.Json;
using API.Middlewares;
using Data.Repositories.Implementations;
using Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Models.Models;
using Service.Services.Implementations;
using Service.Services.Implementations.Repository;
using Service.Services.Interfaces;
using Service.Services.Interfaces.Repository;
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
builder.Services.AddSwaggerGen();

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

builder.Services.AddHttpContextAccessor();

// =============================
// JWT CONFIG
// =============================
var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrEmpty(jwtSecret))
    throw new Exception("Jwt:Secret missing in appsettings");

var key = Encoding.UTF8.GetBytes(jwtSecret);

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
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
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

// 🔥 QUAN TRỌNG
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseCustomExceptionHandler();
app.Run();
