
using PrintFlow.Domain.Entities;

namespace PrintFlow.Application.Interfaces;

public interface IPedidoRepository
{

    // Busca un pedido por id (devuelve null si no existe)
    Task<Pedido?> ObtenerPorIdAsync(Guid id);

    // Incorpora un pedido al seguimiento de EF Core
    Task AgregarAsync(Pedido pedido);

    // Confirma los cambios pendientes en PostgreSQL
    Task GuardarCambiosAsync();
}
