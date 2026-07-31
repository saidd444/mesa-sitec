namespace MesaSitec.Dtos;

using MesaSitec.Models;

public class TransicionarRequest

{
    public EstadoSolicitud NuevoEstado {get; set;}
    public string? Motivo {get; set;}
    public Guid? AgenteId {get; set;}
}