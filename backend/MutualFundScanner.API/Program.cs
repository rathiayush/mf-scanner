using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MutualFundScanner.Infrastructure.Data;
using MutualFundScanner.Application.Interfaces;
using MutualFundScanner.Application.Services;
using MutualFundScanner.Application.DTOs;
using MutualFundScanner.Infrastructure.UnitOfWork;
using UnitOfWork = MutualFundScanner.Infrastructure.UnitOfWork.UnitOfWork;
using MutualFundScanner.Infrastructure.Repositories;
using Serilog;
using Serilog.Events;
using MutualFundScanner.Application.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
var serilogConfig = builder.Configuration.GetSection("Serilog");
var enableConsoleLogging = serilogConfig.GetValue<bool>("EnableConsoleLogging", true);
var enableFileLogging = serilogConfig.GetValue<bool>("EnableFileLogging", true);

var loggerConfig = new LoggerConfiguration();

if (enableConsoleLogging)
{
    loggerConfig.WriteTo.Console();
}

if (enableFileLogging)
{
    loggerConfig.WriteTo.File("logs/mutualfundscanner-.txt", rollingInterval: RollingInterval.Day);
}

Log.Logger = loggerConfig.CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Mutual Fund Scanner API", 
        Version = "v1",
        Description = "API for scanning and processing mutual fund portfolio files"
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add Entity Framework
builder.Services.AddDbContext<MutualFundContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Application Services
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IStreamlinedUploadService, StreamlinedUploadService>();
builder.Services.AddScoped<MutualFundDetectionService>();
builder.Services.AddScoped<IMutualFundService, MutualFundScanner.Application.Services.MutualFundService>();
builder.Services.AddScoped<IConcreteMutualFundService, MutualFundScanner.Application.Services.ConcreteMutualFundService>();
builder.Services.AddScoped<ISecurityAnalyticsService, SecurityAnalyticsService>();
builder.Services.AddScoped<IBackgroundProcessingService, BackgroundProcessingService>();
builder.Services.AddScoped<IExcelReportService, ExcelReportService>();
builder.Services.AddScoped<ISnapshotManagementService, SnapshotManagementService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Register Repository Services
builder.Services.AddScoped<IConcreteMutualFundRepository, ConcreteMutualFundRepository>();

// Register Unit of Work (using the new simplified pattern)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Helper Classes
builder.Services.AddScoped<MutualFundScanner.Application.Helpers.AnalyticsCalculationHelper>();
builder.Services.AddScoped<MutualFundScanner.Application.Helpers.DataValidationHelper>();
builder.Services.AddScoped<MutualFundScanner.Application.Interfaces.ISnapshotFetchingHelper, MutualFundScanner.Application.Helpers.SnapshotFetchingHelper>();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "MutualFundScanner",
        ValidAudience = jwtSettings["Audience"] ?? "MutualFundScannerUsers",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000", 
                "http://localhost:5173",
                "http://127.0.0.1:3000",
                "http://127.0.0.1:5173"
            )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
    
    // Add a more permissive policy for network access
    options.AddPolicy("AllowNetwork", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mutual Fund Scanner API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowNetwork");

// app.UseHttpsRedirection(); // Disabled for network access

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MutualFundContext>();
    await MutualFundScanner.Infrastructure.Data.Seeders.DatabaseSeeder.SeedAsync(context);

    // Seed initial admin/user login if not exists
    try
    {
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var db = scope.ServiceProvider.GetRequiredService<MutualFundContext>();

        var seedEmail = "ayushrathi@gmail.com";
        var seedUsername = "ayushrathi";
        var seedPassword = "User@1785";
        var userExists = await db.Users.AnyAsync(u => u.Email == seedEmail);
        if (!userExists)
        {
            var createDto = new CreateUserRequestDto
            {
                Username = seedUsername,
                Email = seedEmail,
                Password = seedPassword,
                Role = "Admin",
                Notes = "Seeded user"
            };
            // createdByUserId: 0 indicates system seeding
            await authService.CreateUserAsync(createDto, 0);
            Log.Information("Seeded initial user {Email}", seedEmail);
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to seed initial user");
    }
}

// For IIS Express, we'll use the default configuration
// For local development, we'll use the specified URL
if (app.Environment.IsDevelopment())
{
    app.Run("http://0.0.0.0:5247");
}
else
{
    app.Run();
}
