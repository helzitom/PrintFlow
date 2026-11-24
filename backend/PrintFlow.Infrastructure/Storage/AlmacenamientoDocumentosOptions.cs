
namespace PrintFlow.Infrastructure.Storage;

/// <summary>
/// Define la configuración del almacenamiento de documentos.
/// </summary>
public sealed class AlmacenamientoDocumentosOptions
{
    /// <summary>
    /// Obtiene o establece la carpeta base donde se guardan los docs
    /// </summary>
    public string RutaBase { get; set; } = "App_Data/Documentos";

    /// <summary>
    /// Obtiene o establece el tamaño máximo permitido por archivo en bytess
    /// </summary>
    public long TamanoMaximoBytes { get; set; } = 20 * 1024 * 1024;
}
