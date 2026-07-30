namespace MesaSitec.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MesaSitec.Data;
using MesaSitec.Models;
using MesaSitec.Dtos;
using Microsoft.AspNetCore.Identity;

public class AuthService
{
    private readonly MesaSitecContext _context;
    private readonly  IConfiguration _config;
    private readonly PasswordHasher<Usuario> _passwordHasher;

    public AuthService(MesaSitecContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
        _passwordHasher = new PasswordHasher<Usuario>();

    }
    public LoginResponse? Login(string email, string password)
    {
        // busco el usuario por email
        var usuario = _context.Usuarios
        .FirstOrDefault(u => u.Email == email && u.Activo);

        if (usuario ==null)
        return null; // si no existe o no esta activo 

        // se confirma la password
        var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, password);
        if (resultado == PasswordVerificationResult.Failed)
        return null; // password incorrecta

        // obtiene el tenant para devolver el nombre
        var tenant = _context.Tenants.FirstOrDefault(t => t.Id == usuario.TenantId);

        // GENERANDO EL JWT
        var token = GenerarJwt(usuario);
        // SE DEVUELVEN LAS RESPUESTAS
        return new LoginResponse
        {
            AccessToken = token,
            ExpiraEn = 28800, // 8 horas en segundos
            Usuario = new UsuarioDto
            {
                Id = usuario.Id,
                Email = usuario.Email,
                Nombre = usuario.Nombre,
                Rol = usuario.Rol.ToString(),
                TenantId= usuario.TenantId,
                TenantNombre = tenant?.Nombre?? "Desconocido"
            }
        };
    }
    private string GenerarJwt(Usuario usuario) // crea el token con los claims 
    {
        var secretKey = _config["Jwt:SecretKey"] ?? throw new InvalidOperationException ("JWT: SecretKey no configurada");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("sub", usuario.Id.ToString()),
            new Claim("email", usuario.Email),
            new Claim("tenantId", usuario.TenantId.ToString()),
            new Claim("rol", usuario.Rol.ToString())
        };
                var token = new JwtSecurityToken(
            issuer: "MesaSitec",
            audience: "MesaSitec",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    }
