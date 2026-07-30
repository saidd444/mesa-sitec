namespace MesaSitec.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MesaSitec.Data;

[ApiController]
[Route("api/v1")]
[Authorize]  // Requiere JWT válido
public class CategoriaController : ControllerBase
{
    private readonly MesaSitecContext _context;

    public CategoriaController(MesaSitecContext context)
    {
        _context = context;
    }

    // GET /api/v1/categorias — Devuelve categorías activas del tenant
    [HttpGet("categorias")]
    public IActionResult GetCategorias()
    {
        System.Console.WriteLine("=== ENTRAMOS A GETCATEGORIAS ===");
        
        // Extrae tenantId del JWT (MINÚSCULA)
        var tenantId = User.FindFirst("tenantId")?.Value;
        System.Console.WriteLine($"TenantId from JWT: {tenantId}");

        if (string.IsNullOrEmpty(tenantId))
        {
            System.Console.WriteLine("TenantId está vacío, retorno Unauthorized");
            return Unauthorized();
        }

        // Convierte a Guid
        if (!Guid.TryParse(tenantId, out var tenantGuid))
        {
            System.Console.WriteLine("No se pudo parsear tenantId como Guid");
            return Unauthorized();
        }

        // Busca solo categorías activas de este tenant
        var categorias = _context.Categorias
            .Where(c => c.TenantId == tenantGuid && c.Activo)
            .Select(c => new
            {
                c.Id,
                c.Nombre,
                c.SlaHoras
            })
            .ToList();

        System.Console.WriteLine($"Encontradas {categorias.Count} categorías");

        return Ok(categorias);
    }
}