namespace MesaSitec.Dtos;

public class SolicitudDto

{
    public Guid Id{get; set;}
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;  
    public string Titulo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Prioridad { get; set; } = string.Empty;
    public CategoriaResumenDto Categoria { get; set; } = new();
    public AgenteResumenDto? Agente { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaLimiteSla { get; set; }
    public bool Vencida { get; set; }
}

public class CategoriaResumenDto
{
    public Guid Id {get; set;}
    public string Nombre {get; set; } = string.Empty;
}

public class AgenteResumenDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
public class ListadoSolicitudesResponse
    {
        public List<SolicitudDto> Items {get; set;} = new();
        public int Page {get; set;}
        public int PageSize {get; set;}
        public int Total {get; set;}
        public int TotalPaginas {get; set;}
    }