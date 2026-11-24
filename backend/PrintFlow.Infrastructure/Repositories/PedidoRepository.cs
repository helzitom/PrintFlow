
using Microsoft.EntityFrameworkCore;
using PrintFlow.Application.Interfaces;
using PrintFlow.Domain.Entities;
using PrintFlow.Infrastructure.Persistence;

namespace PrintFlow.Infrastructure.Repositories;

/// <summary>
/// Implementa las operaciones de persistencia de los pedidos
/// utilizando Entity Framework Core.
/// </summary>
public class PedidoRepository : IPedidoRepository
{
    // Contexto de EF Core encargado de gestionar las operaciones d leer y escribir
    private readonly PrintFlowDbContext _context;

    /// <summary>
    /// Inicializa el repositorio recibiendo el contexto
    /// mediante inyección de dependencias.
    /// </summary>
    /// <param name="context">Contexto de acceso a la base de datos.</param>
    public PedidoRepository(PrintFlowDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Busca un pedido por su identificador e incluye documentos asociados.
    /// </summary>
    /// <param name="id">Identificador único del pedido.</param>
    /// <returns>
    /// El pedido encontrado con sus documentos o null si no existe.
    /// </returns>
    public async Task<Pedido?> ObtenerPorIdAsync(Guid id)
    {
        return await _context.Pedidos
            // Carga también los documentos relacionados con el pedido.
            .Include(p => p.Documentos)

            // Busca el pedido cuyo identificador coincida con el recibido.
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Agrega un pedido al seguimiento de EF Core, la operación todavía no se confirma en la base de datos.
    /// </summary>
    /// <param name="pedido">Pedido que se desea agregar.</param>
    public async Task AgregarAsync(Pedido pedido)
    {
        await _context.Pedidos.AddAsync(pedido);
    }

    /// <summary>
    /// Guarda en PostgreSQL los cambios pendientes registrados por el contexto.
    /// </summary>
    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }
}
