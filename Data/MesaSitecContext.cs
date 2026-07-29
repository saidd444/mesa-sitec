namespace MesaSitec.Data;

using Microsoft.EntityFrameworkCore;
using MesaSitec.Models;

public class MesaSitecContext : DbContext

{
    public MesaSitecContext(DbContextOptions<MesaSitecContext> options)
    : base(options)
    {
    }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Usuario> Usuarios {get; set;}
    public DbSet<Categoria> Categorias {get; set;}
    public DbSet<Solicitud> Solicitudes {get; set;}
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Email unico
        modelBuilder.Entity<Usuario>()
        .HasIndex(u => u.Email)
        .IsUnique();
    }
}