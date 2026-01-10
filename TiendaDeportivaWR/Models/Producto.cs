using System.ComponentModel.DataAnnotations;

namespace TiendaDeportivaWR.Models;

public class Producto
{
    [Key]
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "La descripcion es obligatoria")]
    public string Descripcion { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "El costo debe ser mayor a 0")]
    public decimal Costo { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Precio { get; set; }

    public int Existencia { get; set; }
}