namespace MesaSitec.Dtos;

using MesaSitec.Models;

public class UpdateSolicitudRequest
{
    public string? Titulo {get; set;}
    public string? Descripcion {get; set;}
    public Guid? CategoriaId {get; set;}
    public Prioridad? Prioridad {get; set;}

}