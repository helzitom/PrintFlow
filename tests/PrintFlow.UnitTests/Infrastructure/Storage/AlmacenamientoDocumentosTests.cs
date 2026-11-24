
using Microsoft.Extensions.Options;
using PrintFlow.Infrastructure.Storage;

namespace PrintFlow.UnitTests.Infrastructure.Storage;

/// <summary>
/// Contiene las pruebas unitarias del almacenamiento físico de documentos.
/// </summary>
public class AlmacenamientoDocumentosTests : IDisposable
{
    private readonly string _rutaTemporal;
    private readonly AlmacenamientoDocumentos _almacenamiento;

    /// <summary>
    /// Inicializa un almacenamiento aislado para las pruebas.
    /// </summary>
    public AlmacenamientoDocumentosTests()
    {
        _rutaTemporal = Path.Combine(
            Path.GetTempPath(),
            $"PrintFlowTests_{Guid.NewGuid():N}");

        var opciones = Options.Create(
            new AlmacenamientoDocumentosOptions
            {
                RutaBase = _rutaTemporal,
                TamanoMaximoBytes = 1024 * 1024
            });

        _almacenamiento = new AlmacenamientoDocumentos(opciones);
    }

    /// <summary>
    /// Verifica que un documento se guarde con un nombre generado
    /// y que su contenido se conserve correctamente.
    /// </summary>
    [Fact]
    public async Task GuardarAsync_DocumentoValido_DebeGuardarContenido()
    {
        byte[] contenidoOriginal = [1, 2, 3, 4, 5];

        await using var flujo = new MemoryStream(contenidoOriginal);

        var ruta = await _almacenamiento.GuardarAsync(
            flujo,
            "documento.pdf",
            ".pdf");

        var rutaCompleta = Path.Combine(_rutaTemporal, ruta);

        Assert.False(Path.IsPathRooted(ruta));
        Assert.True(File.Exists(rutaCompleta));

        var contenidoGuardado = await File.ReadAllBytesAsync(rutaCompleta);

        Assert.Equal(contenidoOriginal, contenidoGuardado);
    }

    /// <summary>
    /// Verifica que un documento almacenado pueda eliminarse correctamente.
    /// </summary>
    [Fact]
    public async Task EliminarAsync_DocumentoExistente_DebeEliminarArchivo()
    {
        await using var flujo = new MemoryStream([1, 2, 3]);

        var ruta = await _almacenamiento.GuardarAsync(
            flujo,
            "documento.pdf",
            ".pdf");

        var rutaCompleta = Path.Combine(_rutaTemporal, ruta);

        Assert.True(File.Exists(rutaCompleta));

        await _almacenamiento.EliminarAsync(ruta);

        Assert.False(File.Exists(rutaCompleta));
    }

    /// <summary>
    /// Elimina la carpeta temporal utilizada por las pruebas.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_rutaTemporal))
        {
            Directory.Delete(_rutaTemporal, recursive: true);
        }
    }
}
