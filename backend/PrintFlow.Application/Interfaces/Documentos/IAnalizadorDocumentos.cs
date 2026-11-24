
using PrintFlow.Application.DTOs.Documentos;

namespace PrintFlow.Application.Interfaces.Documentos;

/// <summary>
/// Define las operaciones para validar y analizar los docs
/// </summary>
public interface IAnalizadorDocumentos
{
    /// <summary>
    /// Valida el contenido y obtiene los metadatos del doc
    /// </summary>
    /// <param name="contenido">Contenido del documento.</param>
    /// <param name="nombreArchivo">Nombre original del archivo</param>
    /// <param name="cancellationToken">Token para cancelar la operación</param>
    /// <returns>Resultado con la extensión y cantidad de páginas</returns>
    Task<ResultadoAnalisisDocumento> AnalizarAsync(
        Stream contenido,
        string nombreArchivo,
        CancellationToken cancellationToken = default);
}
