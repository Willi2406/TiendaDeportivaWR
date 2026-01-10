using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TiendaDeportivaWR.DAL;
using TiendaDeportivaWR.Models;

namespace TiendaDeportivaWR.Services;

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
            
            var producto = await contexto.Productos
                .SingleAsync(p => p.ProductoId == item.ProductoId);

            var cantidad = item.Cantidad;

            if (tipoOperacion == Operacion.Suma)
                producto.Existencia += cantidad;
            else
                producto.Existencia -= cantidad;

            await contexto.SaveChangesAsync();
        }
    }

    public async Task<bool> Existe(int id)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Entradas.AnyAsync(e => e.EntradaId == id);
    }

    public async Task<bool> Insertar(Entrada entrada)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();

        contexto.Add(entrada);

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

        await AfectarExistencia(original.Detalle, Operacion.Resta);

        contexto.EntradaDetalles.RemoveRange(original.Detalle);

        contexto.Update(entrada);

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
        entrada.Total = entrada.Detalle.Sum(d => d.Cantidad * d.Costo);

        if (entrada.EntradaId == 0)
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

    public async Task<List<Producto>> GetProductos()
    {
        using var ctx = await DbFactory.CreateDbContextAsync();
        return await ctx.Productos
            .AsNoTracking()
            .ToListAsync();
    }
}