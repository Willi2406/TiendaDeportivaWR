using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TiendaDeportivaWR.DAL;
using TiendaDeportivaWR.Models;

namespace TiendaDeportivaWR.Services;

public class ProductoServices(IDbContextFactory<Contexto> DbFactory)
{
    public async Task<bool> Insertar(Producto producto)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        contexto.Productos.Add(producto);
        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<bool> Modificar(Producto producto)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        contexto.Update(producto);
        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<bool> Guardar(Producto producto)
    {
        if (await Existe(producto.ProductoId))
            return await Modificar(producto);
        else
            return await Insertar(producto);
    }

    public async Task<bool> Existe(int id)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Productos.AnyAsync(p => p.ProductoId == id);
    }

    public async Task<bool> ExisteDescripcion(string descripcion, int id)
    {
        // Verifica si ya existe otro producto con el mismo nombre
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Productos.AnyAsync(p => p.Descripcion.ToLower() == descripcion.ToLower() && p.ProductoId != id);
    }

    public async Task<bool> Eliminar(int id)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();

        // Validar si el producto se usó en alguna Entrada (Integridad Referencial)
        var enUso = await contexto.EntradaDetalles.AnyAsync(d => d.ProductoId == id);
        if (enUso) return false; // No se puede borrar

        var producto = await contexto.Productos.FindAsync(id);
        if (producto == null) return false;

        contexto.Productos.Remove(producto);
        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<Producto?> Buscar(int id)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Productos.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductoId == id);
    }

    public async Task<List<Producto>> Listar(Expression<Func<Producto, bool>> criterio)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Productos
            .AsNoTracking()
            .Where(criterio)
            .ToListAsync();
    }
}