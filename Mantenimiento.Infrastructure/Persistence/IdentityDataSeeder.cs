using Mantenimiento.Domain.Entities;
using Mantenimiento.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mantenimiento.Infrastructure.Persistence;

public static class IdentityDataSeeder
{
    public static async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();
        var context = serviceProvider.GetRequiredService<MantenimientoDbContext>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        // Development-only fallback used while the API is still in design phase.
        var seedPassword = configuration["SeedSettings:DefaultPassword"] ?? "Admin123!";

        // 1. Seed Roles
        string[] roles =
        {
            RolUsuario.Administrador.ToString(),
            RolUsuario.Tecnico.ToString(),
            RolUsuario.Solicitante.ToString(),
            RolUsuario.Programador.ToString()
        };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }

        // 2. Seed Dependencias in an idempotent way
        var defaultDependencias = new[]
        {
            new Dependencia { Id = Guid.NewGuid(), Nombre = "Mantenimiento General",  Regional = "Antioquia", Departamento = "Medellin",      Codigo = "DEP-001", Ubicacion = "Sede Central",    JefeEmail = "jefe.mantenimiento@fixops.com" },
            new Dependencia { Id = Guid.NewGuid(), Nombre = "Infraestructura Fisica",  Regional = "Antioquia", Departamento = "Envigado",       Codigo = "DEP-002", Ubicacion = "Planta Norte",    JefeEmail = "jefe.infraestructura@fixops.com" },
            new Dependencia { Id = Guid.NewGuid(), Nombre = "Sistemas y Redes",        Regional = "Bogota",    Departamento = "Cundinamarca",   Codigo = "DEP-003", Ubicacion = "Edificio Capital", JefeEmail = "jefe.sistemas@fixops.com" },
            new Dependencia { Id = Guid.NewGuid(), Nombre = "Electromecanica",         Regional = "Valle",     Departamento = "Cali",           Codigo = "DEP-004", Ubicacion = "Sede Sur",        JefeEmail = "jefe.electromecanica@fixops.com" },
            new Dependencia { Id = Guid.NewGuid(), Nombre = "Servicios Generales",     Regional = "Bogota",    Departamento = "Cundinamarca",   Codigo = "DEP-005", Ubicacion = "Sede Bogota",     JefeEmail = "jefe.servicios@fixops.com" }
        };

        foreach (var dep in defaultDependencias)
        {
            var existing = context.Dependencias.FirstOrDefault(x => x.Codigo == dep.Codigo);
            if (existing == null)
            {
                await context.Dependencias.AddAsync(dep);
            }
            else if (string.IsNullOrWhiteSpace(existing.JefeEmail))
            {
                existing.JefeEmail = dep.JefeEmail;
                context.Dependencias.Update(existing);
            }
        }

        await context.SaveChangesAsync();

        var dependenciasByCode = context.Dependencias
            .ToDictionary(d => d.Codigo, d => d.Id, StringComparer.OrdinalIgnoreCase);

        // 3. Seed Users (2 admins, 3 technicians)
        var usersToSeed = new (string Email, string Name, RolUsuario Role, string DepCode)[]
        {
            ("admin1@mantenimiento.com",      "Admin Sistema Principal",    RolUsuario.Administrador, "DEP-001"),
            ("admin2@mantenimiento.com",      "Admin Auxiliar",             RolUsuario.Administrador, "DEP-002"),
            ("programador1@mantenimiento.com","Carlos Programador",         RolUsuario.Programador,   "DEP-001"),
            ("tecnico1@mantenimiento.com",    "Juan Tecnico Antioquia",     RolUsuario.Tecnico,       "DEP-001"),
            ("tecnico2@mantenimiento.com",    "Pedro Tecnico Bogota",       RolUsuario.Tecnico,       "DEP-003"),
            ("tecnico3@mantenimiento.com",    "Maria Tecnica Valle",        RolUsuario.Tecnico,       "DEP-004")
        };

        foreach (var u in usersToSeed)
        {
            if (!dependenciasByCode.TryGetValue(u.DepCode, out var depId))
            {
                continue;
            }

            var existingUser = await userManager.FindByEmailAsync(u.Email);
            if (existingUser == null)
            {
                var newUser = new Usuario
                {
                    UserName = u.Email,
                    Email = u.Email,
                    Nombre = u.Name,
                    Rol = u.Role,
                    DependenciaId = depId,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, seedPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, u.Role.ToString());
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(existingUser, u.Role.ToString()))
                {
                    await userManager.AddToRoleAsync(existingUser, u.Role.ToString());
                }
            }
        }

        // 4. Seed ConfiguracionEmpresa (singleton)
        if (!context.ConfiguracionEmpresa.Any())
        {
            await context.ConfiguracionEmpresa.AddAsync(new ConfiguracionEmpresa
            {
                Id                     = Guid.NewGuid(),
                AlmuerzoPagadoEnJornada = true,
                HorasDiariasEfectivas  = 7,
                HorasSemanalesMaximas  = 42,
                HorasExtrasMaximas     = 8,
                HoraInicioDefault      = new TimeOnly(7, 0)
            });
            await context.SaveChangesAsync();
        }

        // 5. Seed recomendaciones de horario (3 defaults)
        if (!context.Recomendaciones.Any())
        {
            var recomendaciones = new[]
            {
                new Domain.Entities.RecomendacionHorario { Id = Guid.NewGuid(), Nombre = "Mañana",       HoraInicio = new TimeOnly(6,  0), HoraFin = new TimeOnly(14, 0), Activo = true },
                new Domain.Entities.RecomendacionHorario { Id = Guid.NewGuid(), Nombre = "Intermedio",   HoraInicio = new TimeOnly(8,  0), HoraFin = new TimeOnly(17, 0), Activo = true },
                new Domain.Entities.RecomendacionHorario { Id = Guid.NewGuid(), Nombre = "Tarde-Noche",  HoraInicio = new TimeOnly(13, 0), HoraFin = new TimeOnly(21, 0), Activo = true }
            };
            await context.Recomendaciones.AddRangeAsync(recomendaciones);
            await context.SaveChangesAsync();
        }
    }
}
