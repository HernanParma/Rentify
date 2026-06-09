using Application.Interfaces.HttpClients;
using Application.Interfaces.IServices;
using Infrastructure.HttpClients;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Rentify.Shared;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString, sqlOptions =>
{
    sqlOptions.EnableRetryOnFailure();
    sqlOptions.MigrationsAssembly("Infrastructure");
}));

builder.Services.AddScoped<IReservationService, ReservationService>();

var vehicleServiceUrl = builder.Configuration["VehicleService:BaseUrl"];
builder.Services.AddHttpClient<IVehicleServiceClient, VehicleServiceClient>(client =>
{
    client.BaseAddress = new Uri(vehicleServiceUrl!);
});

var branchOfficeServiceUrl = builder.Configuration["BranchOfficeService:BaseUrl"];
builder.Services.AddHttpClient<IBranchOfficeServiceClient, BranchOfficeServiceClient>(client =>
{
    client.BaseAddress = new Uri(branchOfficeServiceUrl!);
});

var authServiceUrl = builder.Configuration["AuthService:BaseUrl"];
builder.Services.AddHttpClient<INotificationServiceClient, NotificationServiceClient>(client =>
{
    client.BaseAddress = new Uri(authServiceUrl!);
});

builder.Services.AddHostedService<ReservationReminderBackgroundService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ReservationMS", Version = "1.0" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddRentifyJwtAuth(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    await db.Database.ExecuteSqlRawAsync("""
        IF OBJECT_ID(N'ReservationReminders', N'U') IS NULL
        CREATE TABLE ReservationReminders (
            ReservationReminderId uniqueidentifier NOT NULL PRIMARY KEY,
            ReservationId uniqueidentifier NOT NULL,
            ReminderType nvarchar(50) NOT NULL,
            SentAt datetime2 NOT NULL,
            CONSTRAINT UQ_ReservationReminders UNIQUE (ReservationId, ReminderType)
        );
        """);
}

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("ReservationMS arrancó correctamente");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
