using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TiendaDeportiva.DAL;
using TiendaDeportiva.Models;

namespace TiendaDeportiva.Services;

public class EntradaServices(IDbContextFactory<Contexto> DbFactory)
{
    private enum Operacion
    {
        Suma = 1,
        Resta = 2
    }

    private async Task AfectarExistencia(ICollection<EntradaDetalle> detalle, Operacion tipoOperacion)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        foreach (var item in detalle)
        {
            // Buscamos el producto implicado en el detalle
            var producto = await contexto.Articulos
                .SingleAsync(p => p.ArticuloId == item.ArticuloId);

            var cantidad = item.Cantidad;

            // Lógica de inventario para ENTRADAS:
            if (tipoOperacion == Operacion.Suma)
                producto.Existencia += cantidad; // Si entra mercancía, Suma.
            else
                producto.Existencia -= cantidad; // Si se revierte una entrada, Resta.

            await contexto.SaveChangesAsync();
        }
    }

    public async Task<bool> Existe(int id)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Ventas.AnyAsync(e => e.VentaId == id);
    }

    // Nota: Aunque el documento pedía Entradas, si prefieres usar "Ventas" (salidas)
    // avísame. Aquí he programado una ENTRADA (Aumenta Stock) según el documento PDF.

    public async Task<bool> Insertar(Entrada entrada)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();

        contexto.Add(entrada);

        // ALERTA: Aquí usamos SUMA porque es una Entrada de almacén (Entra mercancía)
        await AfectarExistencia(entrada.Detalle, Operacion.Suma);

        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<bool> Modificar(Entrada entrada)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();

        var original = await contexto.Entradas
            .Include(e => e.Detalle)
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.EntradaId == entrada.EntradaId);

        if (original == null) return false;

        // 1. Revertir la operación anterior (Si antes sumó, ahora restamos la cantidad vieja)
        await AfectarExistencia(original.Detalle, Operacion.Resta);

        // 2. Borrar los detalles viejos
        contexto.EntradaDetalles.RemoveRange(original.Detalle);

        // 3. Actualizar la cabecera
        contexto.Update(entrada);

        // 4. Aplicar la nueva operación (Sumar la nueva cantidad)
        await AfectarExistencia(entrada.Detalle, Operacion.Suma);

        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<bool> Eliminar(int entradaId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();

        var entidad = await contexto.Entradas
            .Include(e => e.Detalle)
            .FirstOrDefaultAsync(e => e.EntradaId == entradaId);

        if (entidad is null) return false;

        // Revertir stock: Como fue una entrada (Suma), al eliminar debemos Restar.
        await AfectarExistencia(entidad.Detalle, Operacion.Resta);

        contexto.EntradaDetalles.RemoveRange(entidad.Detalle);
        contexto.Entradas.Remove(entidad);

        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<Entrada?> Buscar(int entradaId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Entradas
            .Include(e => e.Detalle)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EntradaId == entradaId);
    }

    public async Task<bool> Guardar(Entrada entrada)
    {
        // Calculamos el total automáticamente aquí para asegurar la integridad
        // (Opcional, pero recomendado en Entradas)
        entrada.Total = entrada.Detalle.Sum(d => d.Cantidad * d.Costo);

        if (entrada.EntradaId == 0) // Si el ID es 0, es nuevo
        {
            return await Insertar(entrada);
        }
        else
        {
            return await Modificar(entrada);
        }
    }

    public async Task<List<Entrada>> Listar(Expression<Func<Entrada, bool>> criterio)
    {
        using var ctx = await DbFactory.CreateDbContextAsync();
        return await ctx.Entradas
                        .Include(e => e.Detalle)
                        .Where(criterio)
                        .AsNoTracking()
                        .ToListAsync();
    }

    // Método para obtener los Artículos/Productos para el ComboBox
    public async Task<List<Articulo>> GetArticulos()
    {
        using var ctx = await DbFactory.CreateDbContextAsync();
        return await ctx.Articulos
            .AsNoTracking()
            .ToListAsync();
    }
}