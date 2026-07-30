namespace MesaSitec.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MesaSitec.Data;
using MesaSitec.Dtos;

[ApiController]
[Route("api/v1")]
[Authorize] // requiere la autentificacion para accesar
public class MeController : ControllerBase
{
    private readonly MesaSitecContext _context;

    public MeController(MesaSitecContext context)
    {
        _context = context;
    }
     // Get devuelve los daots del usuario
   [HttpGet("me")]
    public IActionResult GetMe()
    {
        // Debug: lista todos los claims del JWT
        System.Console.WriteLine($"User.Claims count: {User.Claims.Count()}");
        foreach (var claim in User.Claims)
        {
            System.Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
        }

        // Extrae el Id usuario del token JWT
        var userId = User.FindFirst("sub")?.Value;
        var tenantId = User.FindFirst("tenantId")?.Value;

        // Debug
        System.Console.WriteLine($"UserId: {userId}, TenantId: {tenantId}");
        if(string.IsNullOrEmpty(userId)|| string.IsNullOrEmpty(tenantId))
            return Unauthorized();
        
// Parsea el userId como Guid y compara directamente (más eficiente y evita problemas de formato)
if (!Guid.TryParse(userId, out var userGuid))
    return Unauthorized();

var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == userGuid);
if (usuario == null)
    return NotFound();
        var tenant = _context.Tenants.FirstOrDefault(t => t.Id.ToString() == tenantId);
        

        // devuelve los datos del usuario
        return Ok(new UsuarioDto
        {
            Id = usuario.Id,
            Email = usuario.Email,
            Nombre = usuario.Nombre,
            Rol = usuario.Rol.ToString(),
            TenantId = usuario.TenantId,
            TenantNombre = tenant?.Nombre ?? "Desconocido"
        });
    }
}