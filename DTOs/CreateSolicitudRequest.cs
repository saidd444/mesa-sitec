namespace MesaSitec.Dtos;

using MesaSitec.Models;

public class CreateSolicitudRequest
{
    public string Titulo {get; set;} = string.Empty;
    public string Descripcion {get; set;} = string.Empty;
    public Guid CategoriaId {get; set;} 
    public Prioridad Prioridad {get; set;}

}