
namespace PrintFlow.Application.DTOs;

/// <summary>
/// Contiene la información necesaria para registrar un pedido de impresión con sus documentos.
/// </summary>
public sealed record RegistrarPedidoRequest
{
    /// <summary>
    /// Número identificador del pedido.
    /// </summary>
    public required string NumeroPedido { get; init; }

    /// <summary>
    /// Nombre del cliente que solicita la impresión.
    /// </summary>
    public required string ClienteNombre { get; init; }

    /// <summary>
    /// Número telefónico del cliente.
    /// </summary>
    public required string ClienteTelefono { get; init; }

    /// <summary>
    /// Documentos que pertenecen al pedido.
    /// </summary>
    public required IReadOnlyCollection<DocumentoPedidoRequest> Documentos { get; init; }
}
