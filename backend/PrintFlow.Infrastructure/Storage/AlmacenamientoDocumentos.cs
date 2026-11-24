
using Microsoft.Extensions.Options;
using PrintFlow.Application.Interfaces.Documentos;

namespace PrintFlow.Infrastructure.Storage;

/// <summary>
/// Implementa el almacenamiento físico y seguro de documentos.
/// </summary>
public sealed class AlmacenamientoDocumentos : IAlmacenamientoDocumentos
{
    private readonly string _rutaBase;
    private readonly long _tamanoMaximoBytes;

    /// <summary>
    /// Inicializa el servicio con la configuración de almacenamiento.
    /// </summary>
    /// <param name="options">Configuración del almacenamiento.</param>
    public AlmacenamientoDocumentos(
        IOptions<AlmacenamientoDocumentosOptions> options)
    {
        var configuracion = options.Value;

        if (string.IsNullOrWhiteSpace(configuracion.RutaBase))
        {
            throw new InvalidOperationException(
                "La ruta base de almacenamiento es obligatoria.");
        }

        if (configuracion.TamanoMaximoBytes <= 0)
        {
            throw new InvalidOperationException(
                "El tamaño máximo debe ser mayor que cero.");
        }

        _rutaBase = Path.GetFullPath(configuracion.RutaBase);
        _tamanoMaximoBytes = configuracion.TamanoMaximoBytes;

        Directory.CreateDirectory(_rutaBase);
    }

    /// <summary>
    /// Guarda un documento con un nombre generado por el servidor.
    /// </summary>
    /// <param name="contenido">Contenido del documento</param>
    /// <param name="nombreArchivo">Nombre real del archivo </param>
    /// <param name="extension">Extensión validada del doc</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Ruta relativa del archivo almacenado</returns>
    public async Task<string> GuardarAsync(
        Stream contenido,
        string nombreArchivo,
        string extension,
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

        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ArgumentException(
                "La extensión es obligatoria.",
                nameof(extension));
        }

        var extensionNormalizada = extension.Trim().ToLowerInvariant();

        if (!extensionNormalizada.StartsWith('.'))
        {
            extensionNormalizada = $".{extensionNormalizada}";
        }

        if (extensionNormalizada.Length > 10 ||
            extensionNormalizada.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '.'))
        {
            throw new ArgumentException(
                "La extensión no es válida.",
                nameof(extension));
        }

        if (contenido.CanSeek && contenido.Length > _tamanoMaximoBytes)
        {
            throw new InvalidOperationException(
                "El documento supera el tamaño máximo permitido.");
        }

        var identificador = Guid.NewGuid().ToString("N");
        var nombreGenerado = $"{identificador}{extensionNormalizada}";
        var rutaCompleta = Path.Combine(_rutaBase, nombreGenerado);

        await using var destino = new FileStream(
            rutaCompleta,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        var buffer = new byte[81920];
        long totalBytes = 0;

        try
        {
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

                await destino.WriteAsync(
                    buffer.AsMemory(0, bytesLeidos),
                    cancellationToken);
            }

            if (totalBytes == 0)
            {
                throw new InvalidOperationException(
                    "No se permite almacenar documentos vacíos.");
            }
        }
        catch
        {
            await destino.DisposeAsync();

            if (File.Exists(rutaCompleta))
            {
                File.Delete(rutaCompleta);
            }

            throw;
        }

        return nombreGenerado;
    }

    /// <summary>
    /// Elimina un documento almacenado dentro de la carpeta autorizada.
    /// </summary>
    /// <param name="rutaAlmacenamiento">Nombre relativo del documento.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    public Task EliminarAsync(
        string rutaAlmacenamiento,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(rutaAlmacenamiento) ||
            Path.IsPathRooted(rutaAlmacenamiento) ||
            !string.Equals(
                Path.GetFileName(rutaAlmacenamiento),
                rutaAlmacenamiento,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "La ruta del documento no es válida.",
                nameof(rutaAlmacenamiento));
        }

        var rutaCompleta = Path.GetFullPath(
            Path.Combine(_rutaBase, rutaAlmacenamiento));

        var prefijoAutorizado = _rutaBase.EndsWith(
            Path.DirectorySeparatorChar.ToString(),
            StringComparison.Ordinal)
            ? _rutaBase
            : _rutaBase + Path.DirectorySeparatorChar;

        if (!rutaCompleta.StartsWith(
            prefijoAutorizado,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "El documento está fuera de la carpeta autorizada.");
        }

        if (File.Exists(rutaCompleta))
        {
            File.Delete(rutaCompleta);
        }

        return Task.CompletedTask;
    }
}
