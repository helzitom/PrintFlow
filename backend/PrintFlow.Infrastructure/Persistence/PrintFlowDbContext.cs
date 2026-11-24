using Microsoft.EntityFrameworkCore;
using PrintFlow.Domain.Entities;

namespace PrintFlow.Infrastructure.Persistence;

// DbContext representa la sesión de trabajo de Entity Framework Core
// con la base de datos. Gestiona consultas, seguimiento de entidades
// y operaciones de persistencia.
public class PrintFlowDbContext : DbContext
{
    // Constructor que recibe las opciones de configuración del contexto.
    public PrintFlowDbContext(DbContextOptions<PrintFlowDbContext> options)
        : base(options)
    {
        // Envía las opciones recibidas al constructor de DbContext
        // para inicializar el contexto con la configuración proporcionada.
    }

    // Representa la tabla Pedidos de la base de datos, permite consultar, agregar, actualizar y eliminar pedidos mediante EF Core.
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    
    // Permite trabajar con los documentos asociados a los pedidos.
    public DbSet<Documento> Documentos => Set<Documento>();

    // Permite configurar cómo las entidades del dominio, se representan dentro del modelo relacional de PostgreSQL.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ejecuta primero la configuración base de DbContext.
        base.OnModelCreating(modelBuilder);

        // Configura el mapeo de la entidad Pedido.
        modelBuilder.Entity<Pedido>(entity =>
        {
            // Define el nombre de la tabla que almacenará los pedidos.
            entity.ToTable("Pedidos");

            // Establece la propiedad Id como clave primaria.
            entity.HasKey(p => p.Id);

            // Configura NumeroPedido como campo obligatorio
            // y limita su longitud máxima a 30 caracteres.
            entity.Property(p => p.NumeroPedido)
                .IsRequired()
                .HasMaxLength(30);

            // Configura ClienteNombre como campo obligatorio
            // y limita su longitud máxima a 150 caracteres.
            entity.Property(p => p.ClienteNombre)
                .IsRequired()
                .HasMaxLength(150);

            // Configura ClienteTelefono como campo obligatorio
            // y limita su longitud máxima a 20 caracteres.
            entity.Property(p => p.ClienteTelefono)
                .IsRequired()
                .HasMaxLength(20);

            // Configura Estado como un campo obligatorio.
            // Convierte el enum EstadoPedido a texto para almacenarlo
            // en PostgreSQL, por ejemplo: "Pendiente" o "Aprobado".
            entity.Property(p => p.Estado)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            // Configura FechaCreacion como un campo obligatorio.
            entity.Property(p => p.FechaCreacion)
                .IsRequired();

            // Define la relación uno a muchos entre Pedido y Documento.
            // Un pedido puede contener varios documentos.
            entity.HasMany(p => p.Documentos)

                // Indica que Documento no tiene una propiedad de navegación
                // hacia Pedido dentro de su clase.
                .WithOne()

                // Establece PedidoId como clave foránea en Documento.
                .HasForeignKey(d => d.PedidoId)

                // Configura el borrado en cascada:
                // al eliminar un pedido de la base de datos,
                // EF Core permite eliminar también sus documentos relacionados.
                .OnDelete(DeleteBehavior.Cascade);

            // Indica que EF Core debe utilizar el campo privado
            // _documentos para acceder a la colección de documentos.
            // Esto permite conservar el encapsulamiento de la entidad Pedido.
            entity.Navigation(p => p.Documentos)
                .HasField("_documentos")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        // Configura el mapeo de la entidad Documento.
        modelBuilder.Entity<Documento>(entity =>
        {
            // Define el nombre de la tabla que almacenará los documentos.
            entity.ToTable("Documentos");

            // Establece la propiedad Id como clave primaria.
            entity.HasKey(d => d.Id);

            // Configura NombreArchivo como campo obligatorio
            // con una longitud máxima de 255 caracteres.
            entity.Property(d => d.NombreArchivo)
                .IsRequired()
                .HasMaxLength(255);

            // Configura Extension como campo obligatorio
            // con una longitud máxima de 10 caracteres.
            entity.Property(d => d.Extension)
                .IsRequired()
                .HasMaxLength(10);

            // Configura RutaAlmacenamiento como campo obligatorio
            // con una longitud máxima de 500 caracteres.
            entity.Property(d => d.RutaAlmacenamiento)
                .IsRequired()
                .HasMaxLength(500);

            // Configura CantidadPaginas como campo obligatorio.
            entity.Property(d => d.CantidadPaginas)
                .IsRequired();

            // Configura NumeroCopias como campo obligatorio.
            entity.Property(d => d.NumeroCopias)
                .IsRequired();

            // Configura EsColor como campo obligatorio.
            entity.Property(d => d.EsColor)
                .IsRequired();
        });
    }
}
