
using PrintFlow.Application.Interfaces;
using PrintFlow.Domain.Entities;

namespace PrintFlow.UnitTests.Fakes;

/// <summary>
/// Simula el repositorio de pedidos utilizando memoria
/// para realizar pruebas unitarias sin acceder a PostgreSQL.
/// </summary>
public class FakePedidoRepository : IPedidoRepository
{
    private readonly List<Pedido> _pedidos = new();

    /// <summary>
    /// Expone los pedidos almacenados durante la prueba
    /// sin permitir modificar directamente la colección.
    /// </summary>
    public IReadOnlyCollection<Pedido> Pedidos => _pedidos.AsReadOnly();

    /// <summary>
    /// Indica cuántas veces se ha solicitado guardar los cambios, ayuda a comprobar si el servicio confirmó la operación.
    /// </summary>
    public int GuardarCambiosCount { get; private set; }

    /// <summary>
    /// Busca un pedido por su identificador en la colección almacenada en memoria.
    /// </summary>
    public Task<Pedido?> ObtenerPorIdAsync(Guid id)
    {
        var pedido = _pedidos.FirstOrDefault(p => p.Id == id);

        return Task.FromResult(pedido);
    }

    /// <summary>
    /// Agrega un pedido a la colección en memoria.
    /// </summary>
    public Task AgregarAsync(Pedido pedido)
    {
        ArgumentNullException.ThrowIfNull(pedido);

        _pedidos.Add(pedido);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Simula la confirmación de los cambios en el repositorio.
    /// </summary>
    public Task GuardarCambiosAsync()
    {
        GuardarCambiosCount++;

        return Task.CompletedTask;
    }
}
