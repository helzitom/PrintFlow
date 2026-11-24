
namespace PrintFlow.Application.DTOs;

/// <summary>
/// Contiene los datos de un documento que se incorporará
/// a un pedido de impresión.
/// </summary>
public sealed record DocumentoPedidoRequest
{
    /// Nombre original del archivo recibido.
    public required string NombreArchivo { get; init; }

    /// Extensión del archivo, por ejemplo PDF o DOCX.
    public required string Extension { get; init; }

    /// <summary>
    /// Ruta interna generada por el sistema de almacenamiento, no debe recibirse directamente desde el cliente HTTP.
    /// </summary>
    public required string RutaAlmacenamiento { get; init; }

    /// <summary>
    /// Cantidad de páginas calculada por el proceso de análisis del archivo.
    /// </summary>
    public required int CantidadPaginas { get; init; }

    /// <summary>
    /// Cantidad de copias solicitadas para este documento.
    /// </summary>
    public required int NumeroCopias { get; init; }

    /// <summary>
    /// Indica si el documento debe imprimirse a color.
    /// </summary>
    public required bool EsColor { get; init; }
}
