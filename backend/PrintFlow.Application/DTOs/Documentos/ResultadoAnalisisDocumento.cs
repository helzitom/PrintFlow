namespace PrintFlow.Application.DTOs.Documentos;

/// <summary>
/// Contiene los metadatos obtenidos al analizar un documento.
/// </summary>
public sealed class ResultadoAnalisisDocumento
{
    /// <summary>
    /// Inicializa el resultado del análisis de un documento.
    /// </summary>
    /// <param name="extension">Extensión validada del documento.</param>
    /// <param name="cantidadPaginas">Cantidad de páginas detectadas.</param>
    public ResultadoAnalisisDocumento(
        string extension,
        int cantidadPaginas)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ArgumentException(
                "La extensión es obligatoria.",
                nameof(extension));
        }

        if (cantidadPaginas <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cantidadPaginas),
                "La cantidad de páginas debe ser mayor que cero.");
        }

        Extension = extension;
        CantidadPaginas = cantidadPaginas;
    }

    /// <summary>
    /// Obtiene la extensión validada del documento.
    /// </summary>
    public string Extension { get; }

    /// <summary>
    /// Obtiene la cantidad de páginas detectadas.
    /// </summary>
    public int CantidadPaginas { get; }
}
