using Microsoft.EntityFrameworkCore;
using MutualFundScanner.Infrastructure.Data;
using MutualFundScanner.Application.Interfaces;
using MutualFundScanner.Application.Services;
using MutualFundScanner.Infrastructure.UnitOfWork;
using MutualFundScanner.Infrastructure.Repositories;
using Serilog;
using Serilog.Events;

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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Mutual Fund Scanner API", 
        Version = "v1",
        Description = "API for scanning and processing mutual fund portfolio files"
    });
});

// Add Entity Framework
builder.Services.AddDbContext<MutualFundContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Application Services
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IFileProcessingService, FileProcessingService>();
builder.Services.AddScoped<IUploadV2Service, UploadV2Service>();
builder.Services.AddScoped<IPdfProcessingService, PdfProcessingService>();
builder.Services.AddScoped<MutualFundDetectionService>();
builder.Services.AddScoped<IMutualFundService, MutualFundScanner.Application.Services.MutualFundService>();
builder.Services.AddScoped<IAnalyticsService, MutualFundScanner.Application.Services.AnalyticsService>();
builder.Services.AddScoped<IFileUploadService, MutualFundScanner.Application.Services.FileUploadService>();
builder.Services.AddScoped<IConcreteMutualFundService, MutualFundScanner.Application.Services.ConcreteMutualFundService>();

// Register Repository Services
builder.Services.AddScoped<IConcreteMutualFundRepository, ConcreteMutualFundRepository>();

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<Func<IUnitOfWork>>(provider => () => provider.GetService<IUnitOfWork>()!);
builder.Services.AddScoped<IUnitOfWorkWrapper, UnitOfWorkWrapper>();

// Register Helper Classes
builder.Services.AddScoped<MutualFundScanner.Application.Helpers.AnalyticsCalculationHelper>();
builder.Services.AddScoped<MutualFundScanner.Application.Helpers.DataValidationHelper>();
builder.Services.AddScoped<MutualFundScanner.Application.Interfaces.ISnapshotFetchingHelper, MutualFundScanner.Application.Helpers.SnapshotFetchingHelper>();

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

app.UseAuthorization();

app.MapControllers();


app.Run("http://0.0.0.0:5247");
