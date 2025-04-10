using Microsoft.EntityFrameworkCore;
using Serilog;
using WatchDogServiceApi.Mapper;
using WatchDogServiceApi.Repository;
using WatchDogServiceApi.Service;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

// Configure Serilog before building the host
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Log to the console
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day) // Log to file with daily rolling
    .CreateLogger();

// Add Serilog to the logging pipeline
builder.Logging.AddSerilog();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Repositories (Dapper-based)
builder.Services.AddScoped<IWatchDogSQLJobsRepository, WatchDogServicesRepository>();
builder.Services.AddScoped<IWatchDogWinServicesRepository, WatchDogWinServicesRepository>();

// Register application services (Dependency Injection)
builder.Services.AddScoped<IWatchDogSQLJobService, WatchDogSQLJobService>();
builder.Services.AddScoped<IWatchDogWinService, WatchDogWinService>();

// Register the LoggingService
builder.Services.AddScoped<ILoggingService, LoggingService>(); // Changed to Scoped

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add services to the container.
builder.Services.AddControllers();
// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Hangfire and CORS
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(name: MyAllowSpecificOrigins, builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins); // Fixed to use the correct policy
app.UseAuthorization();

app.MapControllers();

// Run the application
app.Run();

// Ensure proper log cleanup when the application shuts down
Log.CloseAndFlush();
