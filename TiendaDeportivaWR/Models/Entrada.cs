using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaDeportivaWR.Models;

public class Entrada
{
    [Key]
    public int EntradaId { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria")]
    public DateTime Fecha { get; set; } = DateTime.Now;

    // Cambiamos ClienteNombre por Concepto, ya que es una entrada de almacén
    [Required(ErrorMessage = "El concepto es obligatorio")]
    public string Concepto { get; set; } = string.Empty;

    // Total peso de la entrada (suma de cantidad * costo)
    public decimal Total { get; set; }

    [ForeignKey("EntradaId")]
    public virtual ICollection<EntradaDetalle> Detalle { get; set; } = new List<EntradaDetalle>();
}