using System.ComponentModel.DataAnnotations;

namespace TiendaDeportiva.Models;

public class EntradaDetalle
{
    [Key]
    public int DetalleId { get; set; }

    public int EntradaId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un producto")]
    public int ProductoId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Cantidad { get; set; }

    // Aquí usamos Costo porque representa el valor de adquisición
    [Range(0.01, double.MaxValue, ErrorMessage = "El costo debe ser mayor a 0")]
    public decimal Costo { get; set; }
}