namespace MesaSitec.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MesaSitec.Data;
using MesaSitec.Dtos;
using MesaSitec.Models;

[ApiController]
[Route("api/v1")]
[Authorize]
public class SolicitudController : ControllerBase
{
    private readonly MesaSitecContext _context;

    public SolicitudController(MesaSitecContext context)
    {
        _context = context;
    }

    // GET /api/v1/solicitudes — Listado paginado y filtrado
    [HttpGet("solicitudes")]
    public IActionResult GetSolicitudes(
        [FromQuery] string? estado,
        [FromQuery] string? prioridad,
        [FromQuery] Guid? categoriaId,
        [FromQuery] Guid? agenteId,
        [FromQuery] string? q,
        [FromQuery] bool? vencidas,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sort = "-fechaCreacion")
    {
        // Obtiene el tenantId del JWT
        var tenantId = User.FindFirst("tenantId")?.Value;
        if (string.IsNullOrEmpty(tenantId) || !Guid.TryParse(tenantId, out var tenantGuid))
            return Unauthorized();

        // Valida parámetros
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { codigo = "PARAMETRO_INVALIDO", detail = "page >= 1, 1 <= pageSize <= 100" });

        // Comienza la query base (filtrada por tenant)
        var query = _context.Solicitudes.AsQueryable()
            .Where(s => s.TenantId == tenantGuid);

        // Filtro por estado
        if (!string.IsNullOrEmpty(estado))
        {
            if (Enum.TryParse<EstadoSolicitud>(estado, out var estadoEnum))
                query = query.Where(s => s.Estado == estadoEnum);
        }

        // Filtro por prioridad
        if (!string.IsNullOrEmpty(prioridad))
        {
            if (Enum.TryParse<Prioridad>(prioridad, out var prioridadEnum))
                query = query.Where(s => s.Prioridad == prioridadEnum);
        }

        // Filtro por categoría
        if (categoriaId.HasValue)
            query = query.Where(s => s.CategoriaId == categoriaId.Value);

        // Filtro por agente
        if (agenteId.HasValue)
            query = query.Where(s => s.AgenteId == agenteId.Value);

        // Búsqueda por texto
        if (!string.IsNullOrEmpty(q))
        {
            var qLower = q.ToLower();
            query = query.Where(s =>
                s.Codigo.ToLower().Contains(qLower) ||
                s.Titulo.ToLower().Contains(qLower) ||
                s.Descripcion.ToLower().Contains(qLower)
            );
        }

        // Filtro por vencidas
        if (vencidas.HasValue && vencidas.Value)
        {
            var ahora = DateTime.UtcNow;
            query = query.Where(s =>
                s.FechaLimiteSla < ahora &&
                s.Estado != EstadoSolicitud.Resuelta &&
                s.Estado != EstadoSolicitud.Cerrada &&
                s.Estado != EstadoSolicitud.Cancelada
            );
        }

        // Contar total ANTES de paginar
        var total = query.Count();
        var totalPaginas = (total + pageSize - 1) / pageSize;

        // Ordenamiento
        query = sort switch
        {
            "fechaCreacion" => query.OrderBy(s => s.FechaCreacion),
            "-fechaCreacion" => query.OrderByDescending(s => s.FechaCreacion),
            "prioridad" => query.OrderBy(s => s.Prioridad),
            "-prioridad" => query.OrderByDescending(s => s.Prioridad),
            "codigo" => query.OrderBy(s => s.Codigo),
            _ => query.OrderByDescending(s => s.FechaCreacion)
        };

        // Paginación
        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Mapear a DTOs
        var solicitudesDto = items.Select(s => MapToDto(s)).ToList();

        return Ok(new ListadoSolicitudesResponse
        {
            Items = solicitudesDto,
            Page = page,
            PageSize = pageSize,
            Total = total,
            TotalPaginas = totalPaginas
        });
    }

    // POST /api/v1/solicitudes — Crear solicitud
    [HttpPost("solicitudes")]
    public IActionResult CreateSolicitud([FromBody] CreateSolicitudRequest request)
    {
        // Obtiene tenantId y userId del JWT
        var tenantId = User.FindFirst("tenantId")?.Value;
        var userId = User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(tenantId) || !Guid.TryParse(tenantId, out var tenantGuid))
            return Unauthorized();
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        // Valida título
        if (string.IsNullOrEmpty(request.Titulo) || request.Titulo.Length < 5 || request.Titulo.Length > 120)
            return BadRequest(new { codigo = "VALIDACION", detail = "Título debe tener entre 5 y 120 caracteres" });

        // Valida descripción
        if (string.IsNullOrEmpty(request.Descripcion) || request.Descripcion.Length < 10 || request.Descripcion.Length > 4000)
            return BadRequest(new { codigo = "VALIDACION", detail = "Descripción debe tener entre 10 y 4000 caracteres" });

        // Verifica que la categoría existe y pertenece al tenant
        var categoria = _context.Categorias.FirstOrDefault(c => c.Id == request.CategoriaId && c.TenantId == tenantGuid);
        if (categoria == null)
            return NotFound(new { codigo = "RECURSO_NO_ENCONTRADO", detail = "Categoría no encontrada" });

        // Genera código de solicitud (RN-07)
        var year = DateTime.UtcNow.Year;
        var proximoCorrelativo = _context.Solicitudes
            .Where(s => s.TenantId == tenantGuid && s.Codigo.StartsWith($"SOL-{year}-"))
            .Count() + 1;
        var codigo = $"SOL-{year}-{proximoCorrelativo:D5}";

        // Calcula SLA (RN-04)
        var fechaCreacion = DateTime.UtcNow;
        var factor = request.Prioridad switch
        {
            Prioridad.Critica => 0.5,
            Prioridad.Alta => 0.75,
            Prioridad.Media => 1.0,
            Prioridad.Baja => 2.0,
            _ => 1.0
        };
        var fechaLimiteSla = fechaCreacion.AddHours(categoria.SlaHoras * factor);

        // Crea solicitud
        var solicitud = new Solicitud
        {
            Id = Guid.NewGuid(),
            TenantId = tenantGuid,
            Codigo = codigo,
            Titulo = request.Titulo,
            Descripcion = request.Descripcion,
            CategoriaId = request.CategoriaId,
            Prioridad = request.Prioridad,
            Estado = EstadoSolicitud.Nueva,
            SolicitanteId = userGuid,
            AgenteId = null,
            FechaCreacion = fechaCreacion,
            FechaLimiteSla = fechaLimiteSla,
            FechaResolucion = null,
            MotivoResolucion = null,
            MotivoCancelacion = null
        };

        _context.Solicitudes.Add(solicitud);
        _context.SaveChanges();

        return CreatedAtAction(nameof(GetSolicitudById), new { id = solicitud.Id }, MapToDto(solicitud));
    }

    // GET /api/v1/solicitudes/{id} — Obtener detalle
    [HttpGet("solicitudes/{id}")]
    public IActionResult GetSolicitudById(Guid id)
    {
        var tenantId = User.FindFirst("tenantId")?.Value;
        if (string.IsNullOrEmpty(tenantId) || !Guid.TryParse(tenantId, out var tenantGuid))
            return Unauthorized();

        var solicitud = _context.Solicitudes.FirstOrDefault(s => s.Id == id && s.TenantId == tenantGuid);
        if (solicitud == null)
            return NotFound(new { codigo = "RECURSO_NO_ENCONTRADO" });

        return Ok(MapToDto(solicitud));
    }

    // PUT /api/v1/solicitudes/{id} — Editar solicitud
    [HttpPut("solicitudes/{id}")]
    public IActionResult UpdateSolicitud(Guid id, [FromBody] UpdateSolicitudRequest request)
    {
        var tenantId = User.FindFirst("tenantId")?.Value;
        var userId = User.FindFirst("sub")?.Value;
        var rolClaim = User.FindFirst("rol")?.Value;

        if (string.IsNullOrEmpty(tenantId) || !Guid.TryParse(tenantId, out var tenantGuid))
            return Unauthorized();
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        var solicitud = _context.Solicitudes.FirstOrDefault(s => s.Id == id && s.TenantId == tenantGuid);
        if (solicitud == null)
            return NotFound(new { codigo = "RECURSO_NO_ENCONTRADO" });

        // Valida permisos: solo Admin/Agente pueden editar, o Solicitante si es suya y está Nueva
        bool esAdmin = rolClaim == "Admin";
        bool esAgente = rolClaim == "Agente";
        bool esSolicitante = rolClaim == "Solicitante";
        bool esSuya = solicitud.SolicitanteId == userGuid;
        bool estaNueva = solicitud.Estado == EstadoSolicitud.Nueva;

        if (esSolicitante && (!esSuya || !estaNueva))
            return BadRequest(new { codigo = "OPERACION_NO_PERMITIDA", detail = "Solo puedes editar tus solicitudes en estado Nueva" });

        // Actualiza campos permitidos
        if (!string.IsNullOrEmpty(request.Titulo))
        {
            if (request.Titulo.Length < 5 || request.Titulo.Length > 120)
                return BadRequest(new { codigo = "VALIDACION", detail = "Título: 5-120 caracteres" });
            solicitud.Titulo = request.Titulo;
        }

        if (!string.IsNullOrEmpty(request.Descripcion))
        {
            if (request.Descripcion.Length < 10 || request.Descripcion.Length > 4000)
                return BadRequest(new { codigo = "VALIDACION", detail = "Descripción: 10-4000 caracteres" });
            solicitud.Descripcion = request.Descripcion;
        }

        if (request.CategoriaId.HasValue)
        {
            var categoria = _context.Categorias.FirstOrDefault(c => c.Id == request.CategoriaId && c.TenantId == tenantGuid);
            if (categoria == null)
                return NotFound(new { codigo = "RECURSO_NO_ENCONTRADO", detail = "Categoría no encontrada" });
            solicitud.CategoriaId = request.CategoriaId.Value;
        }

        if (request.Prioridad.HasValue)
            solicitud.Prioridad = request.Prioridad.Value;

        _context.SaveChanges();
        return Ok(MapToDto(solicitud));
    }

    // POST /api/v1/solicitudes/{id}/transiciones — Cambiar estado
    [HttpPost("solicitudes/{id}/transiciones")]
    public IActionResult TransicionarSolicitud(Guid id, [FromBody] TransicionarRequest request)
    {
        var tenantId = User.FindFirst("tenantId")?.Value;
        var userId = User.FindFirst("sub")?.Value;
        var rolClaim = User.FindFirst("rol")?.Value;

        if (string.IsNullOrEmpty(tenantId) || !Guid.TryParse(tenantId, out var tenantGuid))
            return Unauthorized();
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        var solicitud = _context.Solicitudes.FirstOrDefault(s => s.Id == id && s.TenantId == tenantGuid);
        if (solicitud == null)
            return NotFound(new { codigo = "RECURSO_NO_ENCONTRADO" });

        // Máquina de estados (RN-02)
        var transicionValida = (solicitud.Estado, request.NuevoEstado) switch
        {
            (EstadoSolicitud.Nueva, EstadoSolicitud.Asignada) => true,
            (EstadoSolicitud.Nueva, EstadoSolicitud.Cancelada) => rolClaim == "Admin",
            (EstadoSolicitud.Asignada, EstadoSolicitud.EnProceso) => true,
            (EstadoSolicitud.Asignada, EstadoSolicitud.Asignada) => true,
            (EstadoSolicitud.Asignada, EstadoSolicitud.Cancelada) => rolClaim == "Admin",
            (EstadoSolicitud.EnProceso, EstadoSolicitud.Resuelta) => true,
            (EstadoSolicitud.EnProceso, EstadoSolicitud.Asignada) => true,
            (EstadoSolicitud.EnProceso, EstadoSolicitud.Cancelada) => rolClaim == "Admin",
            (EstadoSolicitud.Resuelta, EstadoSolicitud.Cerrada) => true,
            (EstadoSolicitud.Resuelta, EstadoSolicitud.EnProceso) => true,
            _ => false
        };

        if (!transicionValida)
            return BadRequest(new { codigo = "TRANSICION_INVALIDA", detail = $"No se puede pasar de {solicitud.Estado} a {request.NuevoEstado}" });

        // Valida motivos según transición
        if (request.NuevoEstado == EstadoSolicitud.Resuelta)
        {
            if (string.IsNullOrEmpty(request.Motivo) || request.Motivo.Length < 20)
                return BadRequest(new { codigo = "MOTIVO_REQUERIDO", detail = "Motivo de resolución mínimo 20 caracteres" });
            solicitud.MotivoResolucion = request.Motivo;
            solicitud.FechaResolucion = DateTime.UtcNow;
        }

        if (request.NuevoEstado == EstadoSolicitud.Cancelada)
        {
            if (string.IsNullOrEmpty(request.Motivo) || request.Motivo.Length < 10)
                return BadRequest(new { codigo = "MOTIVO_REQUERIDO", detail = "Motivo de cancelación mínimo 10 caracteres" });
            solicitud.MotivoCancelacion = request.Motivo;
        }

        // Asigna agente si es necesario
        if (request.NuevoEstado == EstadoSolicitud.Asignada && request.AgenteId.HasValue)
        {
            var agente = _context.Usuarios.FirstOrDefault(u => u.Id == request.AgenteId && u.TenantId == tenantGuid && (u.Rol == Rol.Admin || u.Rol == Rol.Agente) && u.Activo);
            if (agente == null)
                return BadRequest(new { codigo = "AGENTE_INVALIDO" });
            solicitud.AgenteId = request.AgenteId;
        }

        solicitud.Estado = request.NuevoEstado;
        _context.SaveChanges();

        return Ok(MapToDto(solicitud));
    }

    // Método auxiliar: mapea Solicitud a DTO
    private SolicitudDto MapToDto(Solicitud s)
    {
        return new SolicitudDto
        {
            Id = s.Id,
            Codigo = s.Codigo,
            Titulo = s.Titulo,
            Estado = s.Estado.ToString(),
            Prioridad = s.Prioridad.ToString(),
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
            Vencida = (s.FechaLimiteSla ?? DateTime.MaxValue) < DateTime.UtcNow &&
                      s.Estado != EstadoSolicitud.Resuelta &&
                      s.Estado != EstadoSolicitud.Cerrada &&
                      s.Estado != EstadoSolicitud.Cancelada
        };
    }
}