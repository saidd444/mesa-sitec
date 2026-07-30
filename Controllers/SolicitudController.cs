namespace MesaSitec.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MesaSitec.Data;
using MesaSitec.Dtos;
using MesaSitec.Models;

[ApiController]
[Route("api/v1")] //requiere jwt

public class SolicitudController : ControllerBase
{
    private readonly MesaSitecContext _context;

    public SolicitudController(MesaSitecContext context)
    {
        _context = context;
    }

    // get api listado pagina y filtrado
    [HttpGet("solicitudes")]
    public IActionResult GetSolicitudes(
        [FromQuery] string? estado,
        [FromQuery] string? prioridad,
        [FromQuery] Guid? categoriaId,
        [FromQuery] Guid? agenteId,
        [FromQuery] string? q,
        [FromQuery] bool? vencidas,
        [FromQuery] int page =1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sort = "-fechaCreacion")
    {
        // obtiene el tenantID del JWT
        var tenantId = User.FindFirst("tenantId")?.Value;
        if (string.IsNullOrEmpty(tenantId)|| !Guid.TryParse(tenantId, out var tenantGuid))
        return Unauthorized();

        //valida parametros
        if (page < 1 || pageSize < 1 || pageSize > 100)
        return BadRequest(new{codigo = "PARAMETRO_INVALIDO", detail = "page >= 1, 1 <= pageSize <=100"});

        //comienza la query base 
        var query = _context.Solicitudes.AsQueryable()
        .Where(s => s.TenantId == tenantGuid);

        //filtro por estado
        if (!string.IsNullOrEmpty(estado))
        {
            if(Enum.TryParse<EstadoSolicitud>(estado, out var estadoEnum))
            query =query.Where(s => s.Estado ==estadoEnum);
        }
        //filtro por priopidad
        if (!string.IsNullOrEmpty(prioridad))
        {
            if(Enum.TryParse<Prioridad>(prioridad, out var prioridadEnum))
            query =query.Where(s => s.Prioridad ==prioridadEnum);
        }
        // Filtro por categoría
        if (categoriaId.HasValue)
            query = query.Where(s => s.CategoriaId == categoriaId.Value);

        // filtro por agente
        if (agenteId. HasValue)
            query  = query.Where(s => s.AgenteId == agenteId.Value);
        
        //busqueda por texto
        if(!string.IsNullOrEmpty(q))
        {
            var qLower = q.ToLower();
            query = query.Where(s =>
            s.Codigo.ToLower().Contains(qLower) ||
            s.Titulo.ToLower().Contains(qLower) ||
            s.Descripcion.ToLower().Contains(qLower)
            );
        }
        //filtro por vencidas
        if(vencidas.HasValue && vencidas.Value)
        {
            var ahora = DateTime.UtcNow;
            query = query.Where(s =>
            s.FechaLimiteSla < ahora &&
            s.Estado != EstadoSolicitud.Resuelta &&
            s.Estado != EstadoSolicitud.Cerrada &&
            s.Estado != EstadoSolicitud.Cancelada
            );
        }

        //contar total Antes de paginar
        var total  = query.Count ();
        var totalPaginas = (total + pageSize -1 ) / pageSize;

        //ordenar
        query = sort switch
        {
            "fechaCreacion" => query.OrderBy(s => s.FechaCreacion),
            "-fechaCreacion" => query.OrderByDescending(s => s.FechaCreacion),
            "prioridad" => query.OrderBy(s => s.Prioridad),
            "-prioridad" => query.OrderByDescending(s => s.FechaCreacion),
            "codigo" => query.OrderBy(s=> s.Codigo),
           _ => query.OrderByDescending(s => s.FechaCreacion) //default
        };
        //paginacion
        var items =query 
        .Skip((page -1)* pageSize)
        .Take(pageSize)
        .ToList();

        //mapear a DTOs
        var solicitudesDto = items.Select(s => new SolicitudDto
        {
            Id = s.Id,
            Codigo = s.Codigo,
            Titulo = s.Titulo,
            Estado = s.Estado.ToString(),
            Categoria = new CategoriaResumenDto
            {
                Id = s.CategoriaId,                
                Nombre = _context.Categorias.FirstOrDefault(c => c.Id == s.CategoriaId)?.Nombre ?? "Desconocido"
            },
            Agente = s.AgenteId.HasValue ? new AgenteResumenDto
            {
                Id = s.AgenteId.Value,
                Nombre = _context.Usuarios.FirstOrDefault(u => u.Id == s.AgenteId)?.Nombre ?? "Desconocido"
            } : null,
            FechaCreacion = s.FechaCreacion,
            FechaLimiteSla = s.FechaLimiteSla ?? DateTime.MinValue,
            Vencida = (s.FechaLimiteSla ?? DateTime.MaxValue) <DateTime.UtcNow && 
                      s.Estado != EstadoSolicitud.Resuelta &&
                      s.Estado != EstadoSolicitud.Cerrada &&
                      s.Estado != EstadoSolicitud.Cancelada
        }).ToList();
                // Devolver respuesta
        return Ok(new ListadoSolicitudesResponse
        {
            Items = solicitudesDto,
            Page = page,
            PageSize = pageSize,
            Total = total,
            TotalPaginas = totalPaginas
        });
    }
}
    