
using System.Text;
using Microsoft.Extensions.Options;
using PrintFlow.Application.DTOs.Documentos;
using PrintFlow.Application.Interfaces.Documentos;
using UglyToad.PdfPig;

namespace PrintFlow.Infrastructure.Storage;

/// <summary>
/// Implementa la validación y el análisis de documentos PDF.
/// </summary>
public sealed class AnalizadorDocumentos : IAnalizadorDocumentos
{
    private readonly long _tamanoMaximoBytes;

    /// <summary>
    /// Inicializa el analizador con los límites de almacenamiento configurados.
    /// </summary>
    /// <param name="options">Configuración del almacenamiento.</param>
    public AnalizadorDocumentos(
        IOptions<AlmacenamientoDocumentosOptions> options)
    {
        _tamanoMaximoBytes = options.Value.TamanoMaximoBytes;

        if (_tamanoMaximoBytes <= 0)
        {
            throw new InvalidOperationException(
                "El tamaño máximo debe ser mayor que cero.");
        }
    }

    /// <summary>
    /// Valida el contenido de un PDF y obtiene su cantidad de páginas.
    /// </summary>
    /// <param name="contenido">Contenido del documento.</param>
    /// <param name="nombreArchivo">Nombre original del archivo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado con la extensión y cantidad de páginas.</returns>
    public async Task<ResultadoAnalisisDocumento> AnalizarAsync(
        Stream contenido,
        string nombreArchivo,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contenido);

        if (!contenido.CanRead)
        {
            throw new ArgumentException(
                "El contenido no permite lectura.",
                nameof(contenido));
        }

        if (string.IsNullOrWhiteSpace(nombreArchivo))
        {
            throw new ArgumentException(
                "El nombre del archivo es obligatorio.",
                nameof(nombreArchivo));
        }

        var extension = Path.GetExtension(
            Path.GetFileName(nombreArchivo));

        if (!string.Equals(
            extension,
            ".pdf",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Por el momento solo se permiten documentos PDF.");
        }

        long posicionInicial = contenido.CanSeek
            ? contenido.Position
            : 0;

        try
        {
            await using var memoria = new MemoryStream();

            var buffer = new byte[81920];
            long totalBytes = 0;
            int bytesLeidos;

            while ((bytesLeidos = await contenido.ReadAsync(
                buffer.AsMemory(),
                cancellationToken)) > 0)
            {
                totalBytes += bytesLeidos;

                if (totalBytes > _tamanoMaximoBytes)
                {
                    throw new InvalidOperationException(
                        "El documento supera el tamaño máximo permitido.");
                }

                await memoria.WriteAsync(
                    buffer.AsMemory(0, bytesLeidos),
                    cancellationToken);
            }

            if (totalBytes == 0)
            {
                throw new InvalidOperationException(
                    "El documento está vacío.");
            }

            memoria.Position = 0;

            var encabezado = new byte[5];

            int bytesEncabezado = await memoria.ReadAsync(
                encabezado.AsMemory(),
                cancellationToken);

            if (bytesEncabezado != 5 ||
                Encoding.ASCII.GetString(encabezado) != "%PDF-")
            {
                throw new InvalidOperationException(
                    "El contenido no corresponde a un archivo PDF.");
            }

            memoria.Position = 0;

            int cantidadPaginas;

            try
            {
                using var documento = PdfDocument.Open(memoria);
                cantidadPaginas = documento.NumberOfPages;
            }
            catch (Exception excepcion)
            {
                throw new InvalidOperationException(
                    "El PDF está dañado, no es válido o no puede analizarse.",
                    excepcion);
            }

            if (cantidadPaginas <= 0)
            {
                throw new InvalidOperationException(
                    "El PDF debe contener al menos una página.");
            }

            return new ResultadoAnalisisDocumento(
                ".pdf",
                cantidadPaginas);
        }
        finally
        {
            if (contenido.CanSeek)
            {
                contenido.Position = posicionInicial;
            }
        }
    }
}
