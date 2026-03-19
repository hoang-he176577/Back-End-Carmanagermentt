using API.Json;
using API.Middlewares;
using Data.Repositories.Auth.Implementations;
using Data.Repositories.Auth.Interfaces;
using Data.Repositories.Accessories.Implementations;
using Data.Repositories.Accessories.Interfaces;
using Data.Repositories.MaintenanceRequests.Implementations;
using Data.Repositories.MaintenanceRequests.Interfaces;
using Data.Repositories.VehicleAssets.Implementations;
using Data.Repositories.VehicleAssets.Interfaces;
using Data.Repositories.DisposalProposals.Implementations;
using Data.Repositories.DisposalProposals.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Models.Models;
using Service.Services.Auth.Implementations;
using Service.Services.Auth.Interfaces;
using Service.Services.Accessories.Implementations;
using Service.Services.Accessories.Interfaces;
using Service.Services.MaintenanceRequests.Implementations;
using Service.Services.MaintenanceRequests.Interfaces;
using Service.Services.VehicleAssets.Implementations;
using Service.Services.VehicleAssets.Interfaces;
using Service.Services.DisposalProposals.Implementations;
using Service.Services.DisposalProposals.Interfaces;
using Data.Repositories.Implementations;
using Data.Repositories.Interfaces;
using Service.Services.Implementations;
using Service.Services.Implementations.Repository;
using Service.Services.Interfaces;
using Service.Services.Interfaces.Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhap JWT token.",
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

var connectionString = builder.Configuration.GetConnectionString("CarManager");

builder.Services.AddDbContext<CarManagerContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CarManager"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure();
        }));


builder.Services.AddScoped<IVehicleAssetRepository, VehicleAssetRepository>();
builder.Services.AddScoped<IVehicleAssetService, VehicleAssetService>();
builder.Services.AddScoped<IPurchaseProposalRepository, PurchaseProposalRepository>();
builder.Services.AddScoped<ITripLogRepository, TripLogRepository>();
builder.Services.AddScoped<IMaintenanceRequestRepository, MaintenanceRequestRepository>();
builder.Services.AddScoped<IMaintenanceRequestService, MaintenanceRequestService>();
builder.Services.AddScoped<IBranchRepository, BranchRepository>();

builder.Services.AddScoped<IAccessoryRepository, AccessoryRepository>();
builder.Services.AddScoped<IAccessoryService, AccessoryService>();
builder.Services.AddScoped<IDisposalProposalRepository, DisposalProposalRepository>();
builder.Services.AddScoped<IDisposalProposalService, DisposalProposalService>();

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IPurchaseProposalService, PurchaseProposalService>();
builder.Services.AddScoped<ITripLogRepository, TripLogRepository>();
builder.Services.AddScoped<ITripLogService, TripLogService>();

builder.Services.AddScoped<IBranchService, BranchService>();




// 🔥 ADD USER REPO & SERVICE
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// 🔥 ADD PENDING REQUEST REPO & SERVICE
builder.Services.AddScoped<IPendingRequestRepository, PendingRequestRepository>();
builder.Services.AddScoped<IPendingRequestService, PendingRequestService>();

// 🔥 ADD VEHICLE RECEPTION REPO & SERVICE
builder.Services.AddScoped<IVehicleReceptionRepository, VehicleReceptionRepository>();
builder.Services.AddScoped<IVehicleReceptionService, VehicleReceptionService>();

// 🔥 ADD VEHICLE DISTRIBUTION REPO & SERVICE
builder.Services.AddScoped<IVehicleDistributionRepository, VehicleDistributionRepository>();
builder.Services.AddScoped<IVehicleDistributionService, VehicleDistributionService>();

builder.Services.AddHttpContextAccessor();

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

// 🔥 CORS – cho phép frontend gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddScoped<IPostPurchaseService, PostPurchaseService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // tắt để tránh redirect CORS khi dev
app.UseCors("AllowFrontend");
app.UseCustomExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
