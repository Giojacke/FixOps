using System.Net.Mime;
using System.Text;
using System.Text.Json.Serialization;
using Mantenimiento.API.Swagger;
using FluentValidation;
using Mantenimiento.Application.Common;
using Mantenimiento.Application.Interfaces;
using Mantenimiento.Application.Services;
using Mantenimiento.Application.Validators;
using Mantenimiento.Domain.Entities;
using Mantenimiento.Domain.Interfaces;
using Mantenimiento.Infrastructure.Persistence;
using Mantenimiento.Infrastructure.BackgroundServices;
using Mantenimiento.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- Services Configuration ---

// DB Context
builder.Services.AddDbContext<MantenimientoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<Usuario, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<MantenimientoDbContext>()
.AddDefaultTokenProviders();

// Authentication (JWT)
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey == "__SET_IN_USER_SECRETS__")
{
    throw new InvalidOperationException("Jwt:Key is not configured. Set it via User Secrets or environment variables.");
}

var key = Encoding.ASCII.GetBytes(jwtKey);

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async ctx =>
        {
            ctx.HandleResponse();
            ctx.Response.StatusCode = 401;
            ctx.Response.ContentType = MediaTypeNames.Application.Json;
            await ctx.Response.WriteAsJsonAsync(
                ApiResponse.Fail("No autenticado. Debe iniciar sesión para acceder a este recurso."));
        },
        OnForbidden = async ctx =>
        {
            ctx.Response.StatusCode = 403;
            ctx.Response.ContentType = MediaTypeNames.Application.Json;
            await ctx.Response.WriteAsJsonAsync(
                ApiResponse.Fail("Acceso denegado. No tiene permisos suficientes para realizar esta acción."));
        }
    };
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5242", "https://localhost:7085", "http://localhost:5166")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Repositories & Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// HttpContext accessor (needed by CurrentUserService for audit)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Application Services
builder.Services.AddScoped<IOrdenTrabajoService, OrdenTrabajoService>();
builder.Services.AddScoped<IDependenciaService, DependenciaService>();
builder.Services.AddScoped<ITecnicoService, TecnicoService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IEncuestaService, EncuestaService>();
// SmtpEmailService registered as itself so EmailQueueWorker can inject it directly
builder.Services.AddScoped<SmtpEmailService>();
// IEmailService now queues instead of sending — worker handles actual SMTP
builder.Services.AddScoped<IEmailService, QueuedEmailService>();
builder.Services.AddScoped<IUserService, IdentityUserService>();
builder.Services.AddScoped<IProgramadorService, ProgramadorService>();
builder.Services.AddScoped<IConfiguracionCorreoService, ConfiguracionCorreoService>();
builder.Services.AddScoped<ICorreoQueueService, CorreoEncoladoService>();
// Also register CorreoEncoladoService as itself for the worker's internal helpers
builder.Services.AddScoped<CorreoEncoladoService>();
// Background worker
builder.Services.AddHostedService<EmailQueueWorker>();

// Validations
builder.Services.AddValidatorsFromAssemblyContaining<CrearOrdenRequestValidator>();

// Controllers & Swagger
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title   = "API de Gestión de Mantenimiento",
        Version = "v1",
        Description = "Servicios para autenticación, administración operativa, órdenes de trabajo, materiales, técnicos y encuestas del proceso de mantenimiento."
    });
    c.SchemaFilter<EnumSchemaFilter>();
    c.OperationFilter<SwaggerOperationMetadataFilter>();
    c.CustomSchemaIds(SwaggerMetadata.GetSchemaName);
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingrese el token JWT en el encabezado Authorization usando el esquema Bearer."
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentityDataSeeder.SeedDataAsync(services);
    Console.WriteLine("Datos de identidad cargados");
    var dbCtx = services.GetRequiredService<MantenimientoDbContext>();
    await MaterialSeeder.SeedAsync(dbCtx);
}

// --- Middleware Configuration ---

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        context.Response.StatusCode = 500;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var message = app.Environment.IsDevelopment() && feature?.Error is not null
            ? $"Error interno: {feature.Error.Message}"
            : "Ha ocurrido un error inesperado en el servidor. Intente más tarde.";

        await context.Response.WriteAsJsonAsync(ApiResponse.Fail(message));
    });
});

app.UseStatusCodePages(async ctx =>
{
    var response = ctx.HttpContext.Response;
    if (response.ContentType?.Contains("application/json") == true) return;

    response.ContentType = MediaTypeNames.Application.Json;
    var body = response.StatusCode switch
    {
        404 => ApiResponse.Fail("El recurso solicitado no existe."),
        405 => ApiResponse.Fail("Método HTTP no permitido para esta ruta."),
        _   => ApiResponse.Fail($"Error HTTP {response.StatusCode}.")
    };
    await response.WriteAsJsonAsync(body);
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Gestión de Mantenimiento v1");
    c.DocumentTitle = "Documentación API de Mantenimiento";
});

app.UseHttpsRedirection();
app.UseCors("BlazorPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
