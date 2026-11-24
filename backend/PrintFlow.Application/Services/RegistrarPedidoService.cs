
using PrintFlow.Application.DTOs;
using PrintFlow.Application.Interfaces;
using PrintFlow.Domain.Entities;

namespace PrintFlow.Application.Services;

/// <summary>
/// Coordina el registro de pedidos de impresión
/// y sus documentos asociados.
/// </summary>
public class RegistrarPedidoService
{
    private readonly IPedidoRepository _pedidoRepository;

    /// <summary>
    /// Inicializa el servicio con el repositorio de pedidos.
    /// </summary>
    public RegistrarPedidoService(IPedidoRepository pedidoRepository)
    {
        _pedidoRepository = pedidoRepository;
    }

    /// <summary>
    /// Registra un pedido con todos sus documentos, devuelve el identificador generado.
    /// </summary>
    public async Task<Guid> RegistrarAsync(RegistrarPedidoRequest request)
    {
        // Verifica que la solicitud exista.
        ArgumentNullException.ThrowIfNull(request);

        // Un pedido debe contener al menos un documento.
        if (request.Documentos is null || request.Documentos.Count == 0)
            throw new ArgumentException(
                "El pedido debe contener al menos un documento.",
                nameof(request));

        // Crea el pedido. Su estado inicial será Pendiente, según las reglas definidas en la entidad Pedido.
        var pedido = new Pedido(
            request.NumeroPedido,
            request.ClienteNombre,
            request.ClienteTelefono
        );

        // Construye cada documento y lo incorpora al pedido mediante el método que protege las reglas del dominio.
        foreach (var documentoRequest in request.Documentos)
        {
            // Evita procesar elementos nulos.
            if (documentoRequest is null)
                throw new ArgumentException(
                    "La lista contiene un documento nulo.",
                    nameof(request));

            var documento = new Documento(
                documentoRequest.NombreArchivo,
                documentoRequest.Extension,
                documentoRequest.RutaAlmacenamiento,
                documentoRequest.CantidadPaginas,
                documentoRequest.NumeroCopias,
                documentoRequest.EsColor
            );

            pedido.AgregarDocumento(documento);
        }

        // Agrega el pedido al seguimiento de Entity Framework Core.
        await _pedidoRepository.AgregarAsync(pedido);

        // Confirma los cambios en PostgreSQL.
        await _pedidoRepository.GuardarCambiosAsync();

        // Devuelve el identificador del pedido registrado.
        return pedido.Id;
    }
}
