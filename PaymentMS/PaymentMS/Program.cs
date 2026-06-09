using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Application.Interfaces;
using Application.UseCase;
using Infrastructure.Command;
using Infrastructure.Query;
using Application.Interfaces.IServices;
using Application.UseCase.Payments;
using Application.Interfaces.ICommand;
using Infrastructure.HttpClients;
using Application.Interfaces.IServices.IReservationServices;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddControllers();

#if DEBUG
builder.Configuration.AddUserSecrets<Program>();
#endif

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString, sqlOptions =>
{
    sqlOptions.EnableRetryOnFailure();
    sqlOptions.MigrationsAssembly("Infrastructure");
}));

builder.Services.AddScoped<MercadoPagoService>();
builder.Services.AddScoped<IPaymentQuery, PaymentQuery>();
builder.Services.AddScoped<IPaymentCommand, PaymentCommand>();
builder.Services.AddScoped<ICreatePaymentService, CreatePaymentService>();
builder.Services.AddScoped<IGetPaymentService, GetPaymentService>();
builder.Services.AddScoped<IUpdatePaymentStatusService, UpdatePaymentStatusService>();
builder.Services.AddScoped<IPaymentCalculationService, PaymentCalculationService>();

var reservationServiceUrl = builder.Configuration["ReservationService:BaseUrl"];
builder.Services.AddHttpClient<IReservationServiceClient, ReservationServiceClient>(client =>
{
    client.BaseAddress = new Uri(reservationServiceUrl);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "PaymentMS", Version = "1.0" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
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

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var mpToken = app.Configuration["MercadoPago:AccessToken"]?.Trim() ?? string.Empty;
var useMock = bool.TryParse(app.Configuration["MercadoPago:UseMockPayments"], out var mockFlag) && mockFlag
    || string.IsNullOrWhiteSpace(mpToken)
    || mpToken.Contains("-111111-", StringComparison.Ordinal);
if (useMock)
    logger.LogWarning("Mercado Pago en modo SIMULADO. Para pagos reales configure MercadoPago:AccessToken (user-secrets) y UseMockPayments=false.");
else
    logger.LogInformation("Mercado Pago configurado con token real.");
logger.LogInformation("PaymentMS arranco correctamente");

app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 401)
    {
        context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
        context.Response.Headers.Append("Access-Control-Allow-Headers", "Authorization, Content-Type");
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();
app.UseCors("AllowAll");
app.MapControllers();

app.Run();
