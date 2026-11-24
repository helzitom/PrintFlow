
namespace PrintFlow.Application.Interfaces.Documentos;

/// <summary>
/// Define las operaciones para almacenar y eliminar documentos.
/// </summary>
public interface IAlmacenamientoDocumentos
{
    /// <summary>
    /// Guarda un documento y devuelve su ruta de almacenamiento interna.
    /// </summary>
    /// <param name="contenido">Contenido del documento.</param>
    /// <param name="nombreArchivo">Nombre original del archivo.</param>
    /// <param name="extension">Extensión validada del doc</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Ruta relativa generada por el sistema</returns>
    Task<string> GuardarAsync(
        Stream contenido,
        string nombreArchivo,
        string extension,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un documento previamente almacenado.
    /// </summary>
    /// <param name="rutaAlmacenamiento">Ruta relativa del doc</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    Task EliminarAsync(
        string rutaAlmacenamiento,
        CancellationToken cancellationToken = default);
}
