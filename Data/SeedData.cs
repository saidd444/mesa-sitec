namespace MesaSitec.Data;

using MesaSitec.Models;
using Microsoft.AspNetCore.Identity;

public static class SeedData
{
    public static void Initialize(MesaSitecContext context)
    {
        //si hay datos no lo hace
        if(context.Tenants.Any())
            return;

        var seedFechaBase = DateTime.Parse(
            Environment.GetEnvironmentVariable("SEED_FECHA_BASE")
            ?? "2026-01-15T08:00:00Z"
        );

        // Crea el tenant
        var tenantNorte = new Tenant
        {
            Id=Guid.NewGuid(),
            Nombre = "Cooperativa Norte",
            Activo = true
        };

        var tenantSur = new Tenant
        {
            Id = Guid.NewGuid(),
            Nombre = "Bufete Sur",
            Activo = true
        };

        context.Tenants.AddRange(tenantNorte, tenantSur);
        context.SaveChanges();
        
        //crea el usuario
        var hasher = new PasswordHasher<Usuario>();
        var passwordHash = hasher.HashPassword(new Usuario(), "Sitec.2026");

        var usuariosNorte = new List<Usuario>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantNorte.Id,
                Email = "admin@norte.test",
                PasswordHash = passwordHash,
                Nombre = "Admin Norte",
                Rol = Rol.Admin,
                Activo = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantNorte.Id,
                Email = "agente1@norte.test",
                PasswordHash = passwordHash,
                Nombre = "Agente 1",
                Rol = Rol.Agente,
                Activo = true
            },
            new()
            {
                Id =  Guid.NewGuid(),
                TenantId= tenantNorte.Id,
                Email = "agente2@norte.test",
                PasswordHash = passwordHash,
                Nombre = "Agente 2",
                Rol = Rol.Agente,
                Activo = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId =tenantNorte.Id,
                Email = "user1@norte.test",
                PasswordHash = passwordHash,
                Nombre = "Usuario 1",
                Rol =  Rol.Solicitante,
                Activo = true
            },
            new ()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantNorte.Id,
                Email =  "user2@norte.test",
                PasswordHash =  passwordHash,
                Nombre = "Usuario 1",
                Rol = Rol.Solicitante,
                Activo = true
            }
        };
            // crear usuarios del sur
        var usuariosSur = new List<Usuario>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantSur.Id,
                Email = "admin@sur.test",
                PasswordHash = passwordHash,
                Nombre = "Admin Sur",
                Rol = Rol.Admin,
                Activo = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantSur.Id,
                Email = "user1@sur.test",
                PasswordHash = passwordHash,
                Nombre = "Usuario 1 Sur",
                Rol = Rol.Solicitante,
                Activo = true
            }
        };
        var todosUsuarios = usuariosNorte.Concat(usuariosSur).ToList();
        context.Usuarios.AddRange(todosUsuarios);
        context.SaveChanges();

        // Se crean categorias, ambos tenants

        var categorias = new List<Categoria>();
        foreach(var tenant in new [] {tenantNorte, tenantSur })
        {
            categorias.AddRange(new[]
            {
                new Categoria
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    Nombre = "Incidente",
                    SlaHoras = 8,
                    Activo = true
                },
                new Categoria
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    Nombre = "Requerimiento",
                    SlaHoras = 40,
                    Activo = true
                },
                new Categoria
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    Nombre = "Consulta",
                    SlaHoras = 24,
                    Activo = true
                },
                new Categoria
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    Nombre = "Falla critica",
                    SlaHoras = 4,
                    Activo = true
                }
            });
        }
        context.Categorias.AddRange(categorias);
        context.SaveChanges();

        // creando solicitudes de ejemplo
        var incidenteNorte = categorias.First(c => c.TenantId == tenantNorte.Id && c.Nombre == "Incidente");
        var solicitante = usuariosNorte.First(u => u.Rol == Rol.Solicitante);
        var agente = usuariosNorte.First(u => u.Email == "agente1@norte.test");

        var solicitud = new Solicitud
        {
            Id = Guid.NewGuid(),
            TenantId = tenantNorte.Id,
            Codigo = "SOL-2026-00001",
            Titulo = "No puedo acceder al portal.",
            Descripcion = "Al ingresar mis credenciales del sistema me regresan al login (intente 5 veces)",
            CategoriaId = incidenteNorte.Id,
            Prioridad = Prioridad.Alta,
            Estado = EstadoSolicitud.Nueva,
            SolicitanteId = solicitante.Id,
            AgenteId = null,
            FechaCreacion = seedFechaBase,
            FechaLimiteSla = seedFechaBase.AddHours (incidenteNorte.SlaHoras*0.75),
            FechaResolucion = null,
            MotivoResolucion = null,
            MotivoCancelacion = null
        };

        context.Solicitudes.Add(solicitud);
        context.SaveChanges();
    }
}