using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .ConfigureHttpClient((_, handler) =>
    {
        handler.SslOptions.RemoteCertificateValidationCallback = static (
            object _, X509Certificate? _, X509Chain? _, SslPolicyErrors _) => true;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");
app.MapReverseProxy();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Rentify ApiGateway" }));

app.Run();
