using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TiendaDeportiva.Models;

namespace TiendaDeportiva.DAL
{
    // Heredamos de IdentityDbContext para tener las tablas de usuarios (Login)
    public class Contexto : IdentityDbContext
    {
        public Contexto(DbContextOptions<Contexto> options) : base(options) { }

        // Definimos las tablas del negocio (Tienda Deportiva)
        public DbSet<Entrada> Entradas { get; set; }
        public DbSet<EntradaDetalle> EntradaDetalles { get; set; }
        public DbSet<Producto> Productos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Datos iniciales para la Tienda Deportiva
            modelBuilder.Entity<Producto>().HasData(
                new Producto
                {
                    ProductoId = 1,
                    Descripcion = "Guantes de Boxeo Pro",
                    Costo = 1500,
                    Precio = 2500,
                    Existencia = 0 // Inicialmente 0, se llenará con Entradas
                },
                new Producto
                {
                    ProductoId = 2,
                    Descripcion = "Pelota de Fútbol FIFA",
                    Costo = 800,
                    Precio = 1200,
                    Existencia = 0
                },
                new Producto
                {
                    ProductoId = 3,
                    Descripcion = "Bate de Béisbol Aluminio",
                    Costo = 3000,
                    Precio = 4500,
                    Existencia = 0
                }
            );
        }
    }
}