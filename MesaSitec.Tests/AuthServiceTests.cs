using Xunit;
using Microsoft.AspNetCore.Identity;
using MesaSitec.Data;
using MesaSitec.Services;
using MesaSitec.Models;
using Microsoft.Extensions.Configuration;

namespace MesaSitec.Tests;

public class AuthServiceTests
{
    private readonly MesaSitecContext _context;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Setup BD en memoria para tests
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<MesaSitecContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MesaSitecContext(options);

        // Setup config mock
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Jwt:SecretKey", "Tu-clave-es-muy-larga-de-minimo-32-caracteres-para-produccion" }
            })
            .Build();

        _authService = new AuthService(_context, config);

        // Seed datos de test
        SeedTestData();
    }

    private void SeedTestData()
    {
        var tenant = new Tenant { Id = Guid.NewGuid(), Nombre = "Test Tenant", Activo = true };
        var hasher = new PasswordHasher<Usuario>();
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = "test@example.com",
            Nombre = "Test User",
            Rol = Rol.Admin,
            Activo = true,
            PasswordHash = hasher.HashPassword(null, "TestPassword123")
        };

        _context.Tenants.Add(tenant);
        _context.Usuarios.Add(usuario);
        _context.SaveChanges();
    }

    [Fact]
    public void Login_WithValidCredentials_ReturnsJWT()
    {
        // Act
        var result = _authService.Login("test@example.com", "TestPassword123");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.Equal("test@example.com", result.Usuario.Email);
    }

    [Fact]
    public void Login_WithInvalidPassword_ReturnsNull()
    {
        // Act
        var result = _authService.Login("test@example.com", "WrongPassword");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Login_WithNonExistentUser_ReturnsNull()
    {
        // Act
        var result = _authService.Login("nonexistent@example.com", "Password123");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Login_ReturnsCorrectExpiration()
    {
        // Act
        var result = _authService.Login("test@example.com", "TestPassword123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(28800, result.ExpiraEn); // 8 horas en segundos
    }
}