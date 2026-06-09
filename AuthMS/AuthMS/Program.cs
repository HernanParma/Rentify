using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices.IUserServices;
using Application.UseCase.UserServices;
using Application.Validators;
using FluentValidation.AspNetCore;
using FluentValidation;
using Infrastructure.Command;
using Infrastructure.Persistence;
using Infrastructure.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Application.Interfaces.IServices.ICryptographyService;
using Application.UseCase.CryptographyService;
using Application.Interfaces.IServices.IAuthServices;
using Application.UseCase.AuthServices;
using Infrastructure.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Application.Interfaces.IServices;
using Application.Interfaces.IRepositories;
using Application.UseCase.NotificationServices;
using Infrastructure.Repositories;
using Infrastructure.Service.NotificationFormatter;
using Application.UseCase;
using Domain.Entities;
using Infrastructure.Persistence.Seeders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#if DEBUG
builder.Configuration.AddUserSecrets<Program>();
#endif

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthMS", Version = "1.0" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});


// Custom            
var connectionString = builder.Configuration["ConnectionString"];

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// Services
builder.Services.AddScoped<IUserPostServices, UserPostServices>();
builder.Services.AddScoped<IUserPutServices, UserPutServices>();
builder.Services.AddScoped<IUserGetServices, UserGetServices>();
builder.Services.AddScoped<IUserPatchServices, UserPatchServices>();
builder.Services.AddScoped<ICryptographyService, CryptographyService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IAuthTokenService, JwtService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<ILogoutService, LogoutService>();
builder.Services.AddSingleton<ITimeProvider, ArgentinaTimeProvider>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IResetCodeGenerator, ResetCodeGenerator>();
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddHostedService<NotificationDispatcher>();
// Formatters Rentify (alquiler)
builder.Services.AddSingleton<INotificationFormatter, ReservationConfirmedFormatter>();
builder.Services.AddSingleton<INotificationFormatter, PaymentConfirmedFormatter>();
builder.Services.AddSingleton<INotificationFormatter, ReservationCancelledFormatter>();
builder.Services.AddSingleton<INotificationFormatter, PickupReminderFormatter>();
builder.Services.AddSingleton<INotificationFormatter, ReturnReminderFormatter>();
builder.Services.AddSingleton<INotificationFormatter, RentalCompletedFormatter>();
builder.Services.AddSingleton<INotificationFormatter, DefaultNotificationFormatter>();


//CQRS
builder.Services.AddScoped<IUserCommand, UserCommand>();
builder.Services.AddScoped<IUserQuery, UserQuery>();
builder.Services.AddScoped<IRefreshTokenCommand, RefreshTokenCommand>();
builder.Services.AddScoped<IRefreshTokenQuery, RefreshTokenQuery>();
builder.Services.AddScoped<IPasswordResetCommand, PasswordResetCommand>();
builder.Services.AddScoped<IPasswordResetQuery, PasswordResetQuery>();
builder.Services.AddScoped<IEmailVerificationCommand, EmailVerificationCommand>();
builder.Services.AddScoped<IEmailVerificationQuery, EmailVerificationQuery>();

//validators
builder.Services.AddValidatorsFromAssembly(typeof(UserRequestValidator).Assembly);
builder.Services.AddFluentValidationAutoValidation();

//TokenConfiguration
var jwtKey = builder.Configuration["JwtSettings:key"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new Exception("No se encontr� 'JwtSettings:key'. Config�ralo en User Secrets o Variables de Entorno.");
}

builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Configurar políticas de autorización basadas en claims
builder.Services.AddAuthorization(options =>
{
    // Política para editar perfil propio
    options.AddPolicy("CanEditOwnProfile", policy =>
        policy.RequireClaim(CustomClaims.CanEditOwnProfile, "true"));

    options.AddPolicy("CanViewBranchInfo", policy =>
        policy.RequireClaim(CustomClaims.CanViewBranchInfo, "true"));

    options.AddPolicy("CanManageReservations", policy =>
        policy.RequireClaim(CustomClaims.CanManageReservations, "true"));

    options.AddPolicy("CanManageFleet", policy =>
        policy.RequireClaim(CustomClaims.CanManageFleet, "true"));

    options.AddPolicy("CanViewOwnReservations", policy =>
        policy.RequireClaim(CustomClaims.CanViewOwnReservations, "true"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(UserRoles.Admin));

    options.AddPolicy("CustomerOnly", policy =>
        policy.RequireRole(UserRoles.Customer));

    // Política para usuarios con email verificado
    options.AddPolicy("EmailVerified", policy =>
        policy.RequireClaim(CustomClaims.IsEmailVerified, "true"));

    // Política para usuarios activos
    options.AddPolicy("ActiveUser", policy =>
        policy.RequireClaim(CustomClaims.AccountStatus, "Active"));
});

//Obtener informacion del claim dentro del service

builder.Services.AddHttpContextAccessor();

//CORS
builder.Services.AddScoped<IUserQuery, UserQuery>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();


app.Use(async (context, next) =>
{
    // Contin�a con la solicitud
    await next();

    // Si el estado de la respuesta es 401 (No autorizado), a�ade los encabezados CORS
    if (context.Response.StatusCode == 401)
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Authorization, Content-Type");

    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseCors("AllowAll");

app.MapControllers();

await DevDataSeeder.SeedAsync(app.Services, app.Environment);

app.Run();
