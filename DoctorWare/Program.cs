using DoctorWare.Data;
using DoctorWare.Constants;
using DoctorWare.Data.Implementation;
using DoctorWare.Data.Interfaces;
using Serilog;

var constructor = WebApplication.CreateBuilder(args);

// ========================================
// CONFIGURAR SERILOG (Lee de appsettings según ambiente)
// ========================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(constructor.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

constructor.Host.UseSerilog();

// ========================================
// SERVICIOS DE BASE DE DATOS
// ========================================
constructor.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
constructor.Services.AddScoped<DatabaseInitializer>();
constructor.Services.AddScoped<EjecutorScriptsSQL>(); // ✅ Ya lo tienes

// ========================================
// REPOSITORIOS
// ========================================
//constructor.Services.AddScoped(typeof(IRepositorioBase<>), typeof(RepositorioBase<>));
//constructor.Services.AddScoped<IRepositorioProducto, RepositorioProducto>();
//constructor.Services.AddScoped<IRepositorioCategoria, RepositorioCategoria>();

// ========================================
// SERVICIOS
// ========================================
//constructor.Services.AddScoped<IServicioProducto, ServicioProducto>();
//constructor.Services.AddScoped<IServicioCategoria, ServicioCategoria>();

// ========================================
// CORS (Lee los orígenes permitidos según ambiente)
// ========================================
var origenesPermitidos = constructor.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200" };

constructor.Services.AddCors(opciones =>
{
    opciones.AddPolicy("PermitirAngular", politica =>
    {
        politica.WithOrigins(origenesPermitidos)
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

// ========================================
// CONFIGURAR CONTROLLERS Y OPENAPI
// ========================================
constructor.Services.AddControllers();

constructor.Services.AddOpenApi(opciones =>
{
    opciones.AddDocumentTransformer((documento, contexto, token) =>
    {
        var baseUrl = constructor.Configuration["BaseUrl"] ?? "http://localhost:5000";
        var ambiente = constructor.Environment.EnvironmentName;

        documento.Info = new()
        {
            Title = "DoctorWareApi",
            Version = "v1",
            Description = $"API con Dapper, PostgreSQL y arquitectura en capas - Ambiente: {ambiente}",
            Contact = new()
            {
                Name = "Clinitech",
                Email = "support@Clinitech.com"
            }
        };

        documento.Servers = new[]
        {
            new Microsoft.OpenApi.Models.OpenApiServer
            {
                Url = baseUrl,
                Description = $"Servidor {ambiente}"
            }
        };

        return Task.CompletedTask;
    });
});

var app = constructor.Build();

// ========================================
// INICIALIZACIÓN DE BASE DE DATOS
// ========================================
if (app.Environment.IsDevelopment())
{
    try
    {
        using var alcance = app.Services.CreateScope();
        
        var inicializador = alcance.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        Log.Information(LogMessages.CHECKING_DATABASE_CONNECTION, app.Environment.EnvironmentName);
        await inicializador.InitializeDatabaseAsync();

        // 2. Ejecutar scripts SQL 👈 NUEVO
        var ejecutorScripts = alcance.ServiceProvider.GetRequiredService<EjecutorScriptsSQL>();
        await ejecutorScripts.EjecutarScriptsAsync();
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, LogMessages.CRITICAL_DATABASE_CONNECTION_ERROR);
        throw;
    }
}
else
{
    // En producción solo verificar conexión
    try
    {
        using var alcance = app.Services.CreateScope();
        var inicializador = alcance.ServiceProvider.GetRequiredService<DatabaseInitializer>();

        Log.Information(LogMessages.CHECKING_DATABASE_CONNECTION, app.Environment.EnvironmentName);
        await inicializador.InitializeDatabaseAsync();
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, LogMessages.CRITICAL_DATABASE_CONNECTION_ERROR);
        throw;
    }
}

// ========================================
// CONFIGURAR MIDDLEWARE
// ========================================

// Mapear OpenAPI
app.MapOpenApi();

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("PermitirAngular");
app.UseAuthorization();
app.MapControllers();

// Endpoint raíz con información
app.MapGet("/", (IConfiguration config, IWebHostEnvironment env) => Results.Json(new
{
    nombre = "DoctorWareApi",
    version = "v1",
    ambiente = env.EnvironmentName,
    baseUrl = config["BaseUrl"],
    descripcion = "API con Dapper, PostgreSQL y arquitectura en capas",
    documentacion = "/openapi/v1.json",
    endpoints = new
    {
        //productos = "/api/productos",
        //categorias = "/api/categorias"
    }
}))
.WithName("Inicio")
.ExcludeFromDescription();

// ========================================
// EJECUTAR APLICACIÓN
// ========================================
try
{
    Log.Information(LogMessages.STARTING_APPLICATION, app.Environment.EnvironmentName);
    Log.Information(LogMessages.OPENAPI_DOCUMENTATION_AVAILABLE);
    Log.Information(LogMessages.CORS_CONFIGURED_FOR, string.Join(", ", origenesPermitidos));
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, LogMessages.APPLICATION_STARTUP_ERROR);
}
finally
{
    Log.CloseAndFlush();
}