using Mantenimiento.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mantenimiento.Infrastructure.Persistence;

public class MantenimientoDbContext : IdentityDbContext<Usuario, IdentityRole<Guid>, Guid>
{
    public MantenimientoDbContext(DbContextOptions<MantenimientoDbContext> options) : base(options) { }

    public DbSet<Dependencia> Dependencias => Set<Dependencia>();
    public DbSet<Material> Materiales => Set<Material>();
    public DbSet<OrdenTrabajo> OrdenesTrabajo => Set<OrdenTrabajo>();
    public DbSet<Operacion> Operaciones => Set<Operacion>();
    public DbSet<EncuestaSatisfaccion> Encuestas => Set<EncuestaSatisfaccion>();
    public DbSet<SolicitudMaterial> SolicitudesMateriales => Set<SolicitudMaterial>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<TurnoHorario> Turnos => Set<TurnoHorario>();
    public DbSet<ConfiguracionEmpresa>  ConfiguracionEmpresa  => Set<ConfiguracionEmpresa>();
    public DbSet<RecomendacionHorario>  Recomendaciones       => Set<RecomendacionHorario>();
    public DbSet<RegistroAuditoria>    Auditorias           => Set<RegistroAuditoria>();
    public DbSet<ConfiguracionCorreo>  ConfiguracionCorreo  => Set<ConfiguracionCorreo>();
    public DbSet<CorreoEncolado>       CorreosEncolados     => Set<CorreoEncolado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrdenTrabajo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Folio).IsRequired().HasMaxLength(20);
            
            entity.HasOne(e => e.Solicitante)
                .WithMany()
                .HasForeignKey(e => e.SolicitanteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TecnicoAsignado)
                .WithMany()
                .HasForeignKey(e => e.TecnicoAsignadoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Dependencia)
                .WithMany()
                .HasForeignKey(e => e.DependenciaId);

            entity.HasMany(e => e.Operaciones)
                .WithOne(o => o.OrdenTrabajo)
                .HasForeignKey(o => o.OrdenTrabajoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Encuesta)
                .WithOne(es => es.OrdenTrabajo)
                .HasForeignKey<EncuestaSatisfaccion>(es => es.OrdenTrabajoId);
        });

        modelBuilder.Entity<EncuestaSatisfaccion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PuntajeAtencion).IsRequired();
            entity.Property(e => e.PuntajeServicio).IsRequired();
            entity.Property(e => e.PuntajeTiempo).IsRequired();
            entity.Property(e => e.Comentarios).HasMaxLength(500);
        });

        modelBuilder.Entity<Operacion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.MaterialesUsados)
                .WithMany();
            entity.HasMany(e => e.SolicitudesMateriales)
                .WithOne(s => s.Operacion)
                .HasForeignKey(s => s.OperacionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitudMaterial>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NombreMaterial).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.Material)
                .WithMany()
                .HasForeignKey(e => e.MaterialId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.Programador)
                .WithMany()
                .HasForeignKey(e => e.ProgramadorId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Mensaje).IsRequired().HasMaxLength(500);
            entity.HasOne(e => e.Destinatario).WithMany()
                .HasForeignKey(e => e.DestinatarioId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.OrdenTrabajo).WithMany()
                .HasForeignKey(e => e.OrdenTrabajoId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            entity.HasOne(e => e.Operacion).WithMany()
                .HasForeignKey(e => e.OperacionId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        });

        modelBuilder.Entity<TurnoHorario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Tecnico).WithMany()
                .HasForeignKey(e => e.TecnicoId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ConfiguracionEmpresa>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<RecomendacionHorario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Dependencia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Codigo).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Regional).HasMaxLength(50);
            entity.Property(e => e.Departamento).HasMaxLength(50);
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TipoMaterial).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<RegistroAuditoria>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TipoEntidad).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ResumenEntidad).HasMaxLength(200);
            entity.Property(e => e.Accion).IsRequired().HasMaxLength(50);
            entity.Property(e => e.UsuarioNombre).HasMaxLength(100);
            entity.Property(e => e.UsuarioEmail).HasMaxLength(200);
            entity.HasIndex(e => e.FechaHora);
            entity.HasIndex(e => e.TipoEntidad);
        });

        modelBuilder.Entity<CorreoEncolado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Destinatario).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Asunto).IsRequired().HasMaxLength(250);
            entity.Property(e => e.TipoCorreo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Estado).IsRequired().HasMaxLength(30);
            entity.Property(e => e.ErrorMensaje).HasMaxLength(2000);
            entity.HasIndex(e => e.Estado);
            entity.HasIndex(e => e.ProximoIntento);
            entity.HasIndex(e => e.FechaCreacion);
        });
    }
}
