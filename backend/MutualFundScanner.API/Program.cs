using Microsoft.EntityFrameworkCore;
using MutualFundScanner.Infrastructure.Data;
using MutualFundScanner.Application.Interfaces;
using Serilog;
using Serilog.Events;
using MutualFundScanner.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/mutualfundscanner-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

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
builder.Services.AddScoped<MutualFundDetectionService>();
builder.Services.AddScoped<IMutualFundService, MutualFundScanner.Application.Services.MutualFundService>();
builder.Services.AddScoped<IAnalyticsService, MutualFundScanner.Application.Services.AnalyticsService>();
builder.Services.AddScoped<IFileUploadService, MutualFundScanner.Application.Services.FileUploadService>();

// Register Helper Classes
builder.Services.AddScoped<MutualFundScanner.Application.Helpers.SnapshotAggregationHelper>();
builder.Services.AddScoped<MutualFundScanner.Application.Helpers.AnalyticsCalculationHelper>();
builder.Services.AddScoped<MutualFundScanner.Application.Helpers.DataValidationHelper>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
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

app.UseCors("AllowReactApp");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();
