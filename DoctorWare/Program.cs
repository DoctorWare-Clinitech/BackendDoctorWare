using DoctorWare.Constants;
using DoctorWare.Data;
using DoctorWare.Data.Implementation;
using DoctorWare.Data.Interfaces;
using DoctorWare.Extensions;
using DoctorWare.Health;
using DoctorWare.Middleware;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using System.Reflection;

WebApplicationBuilder constructor = WebApplication.CreateBuilder(args);

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
constructor.Services.AddScoped<DoctorWare.Repositories.Interfaces.IUsuariosRepository, DoctorWare.Repositories.Implementation.UsuariosRepository>();
constructor.Services.AddScoped<DoctorWare.Repositories.Interfaces.IPersonasRepository, DoctorWare.Repositories.Implementation.PersonasRepository>();

// ========================================
// SERVICIOS
// ========================================
constructor.Services.AddScoped<DoctorWare.Services.Interfaces.IUserService, DoctorWare.Services.Implementation.UsersService>();
constructor.Services.AddScoped<DoctorWare.Services.Interfaces.ITokenService, DoctorWare.Services.Implementation.TokenService>();
constructor.Services.AddScoped<DoctorWare.Services.Interfaces.IEmailSender, DoctorWare.Services.Implementation.SmtpEmailSender>();

// ========================================
// CORS (Lee los orígenes permitidos según ambiente)
// ========================================
string[] origenesPermitidos = constructor.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
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
// CONFIGURAR CONTROLLERS Y SWAGGER (simple)
// ========================================
constructor.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
    });
constructor.Services.AddStandardizedApiBehavior();
constructor.Services.AddEndpointsApiExplorer();
constructor.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "DoctorWare API",
        Version = "v1",
        Description = "API con Dapper y PostgreSQL"
    });

    // Seguridad JWT (Bearer)
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "Authorization",
        Description = "Ingrese 'Bearer {token}'"
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

    // Incluir comentarios XML para Swagger (controllers y tipos)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

// Health checks (DB)
constructor.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

// ========================================
// AUTHENTICACIÓN JWT
// ========================================
constructor.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwt = constructor.Configuration.GetSection("Jwt");
    var secret = jwt["Secret"] ?? "dev-secret-change";
    var issuer = jwt["Issuer"] ?? "DoctorWare";
    var audience = jwt["Audience"] ?? "DoctorWare.Client";

    options.MapInboundClaims = false;
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        NameClaimType = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub
    };
});
constructor.Services.AddAuthorization();

WebApplication app = constructor.Build();

// ========================================
// INICIALIZACIÓN DE BASE DE DATOS
// ========================================
if (app.Environment.IsDevelopment())
{
    try
    {
        using IServiceScope alcance = app.Services.CreateScope();

        DatabaseInitializer inicializador = alcance.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        Log.Information(LogMessages.CHECKING_DATABASE_CONNECTION, app.Environment.EnvironmentName);
        await inicializador.InitializeDatabaseAsync();

        // 2. Ejecutar scripts SQL
        EjecutorScriptsSQL ejecutorScripts = alcance.ServiceProvider.GetRequiredService<EjecutorScriptsSQL>();
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
        using IServiceScope alcance = app.Services.CreateScope();
        DatabaseInitializer inicializador = alcance.ServiceProvider.GetRequiredService<DatabaseInitializer>();

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

// Swagger JSON + UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DoctorWare API v1");
    c.RoutePrefix = "swagger";
});

// Compatibilidad: redirigir antiguo endpoint OpenAPI nativo al de Swashbuckle
app.MapGet("/openapi/v1.json", () => Results.Redirect("/swagger/v1/swagger.json", permanent: false))
   .ExcludeFromDescription();
// Health endpoint
app.MapHealthChecks("/health");

app.UseSerilogRequestLogging();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("PermitirAngular");
app.UseAuthentication();
app.UseAuthorization();
try
{
    app.MapControllers();
}
catch (ReflectionTypeLoadException ex)
{
    Log.Fatal(ex, "ReflectionTypeLoadException al mapear controllers");
    if (ex.LoaderExceptions is not null)
    {
        foreach (var lex in ex.LoaderExceptions)
        {
            Log.Fatal(lex, "LoaderException: {Mensaje}", lex.Message);
        }
    }
    throw;
}

// Endpoint raíz con información
app.MapGet("/", (IConfiguration config, IWebHostEnvironment env) => Results.Json(new
{
    nombre = "DoctorWareApi",
    version = "v1",
    ambiente = env.EnvironmentName,
    baseUrl = config["BaseUrl"],
    descripcion = "API con Dapper, PostgreSQL y arquitectura en capas",
    documentacion = "/swagger/v1/swagger.json",
    endpoints = new
    {
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
