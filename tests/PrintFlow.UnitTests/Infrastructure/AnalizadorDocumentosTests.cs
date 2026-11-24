
using System.Text;
using Microsoft.Extensions.Options;
using PrintFlow.Infrastructure.Storage;

namespace PrintFlow.UnitTests.Infrastructure.Storage;

/// <summary>
/// Contiene las pruebas unitarias del analizador de documentos PDF.
/// </summary>
public class AnalizadorDocumentosTests
{
    /// <summary>
    /// Verifica que se rechacen archivos con extensiones diferentes de PDF.
    /// </summary>
    [Fact]
    public async Task AnalizarAsync_ExtensionIncorrecta_DebeRechazarDocumento()
    {
        var analizador = CrearAnalizador();
        await using var contenido = new MemoryStream(
            Encoding.ASCII.GetBytes("contenido"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => analizador.AnalizarAsync(
                contenido,
                "documento.docx"));
    }

    /// <summary>
    /// Verifica que se rechacen archivos que no contienen una cabecera PDF.
    /// </summary>
    [Fact]
    public async Task AnalizarAsync_ContenidoNoPDF_DebeRechazarDocumento()
    {
        var analizador = CrearAnalizador();
        await using var contenido = new MemoryStream(
            Encoding.ASCII.GetBytes("ESTO NO ES UN PDF"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => analizador.AnalizarAsync(
                contenido,
                "documento.pdf"));
    }

    /// <summary>
    /// Verifica que se rechacen documentos vacíos.
    /// </summary>
    [Fact]
    public async Task AnalizarAsync_ContenidoVacio_DebeRechazarDocumento()
    {
        var analizador = CrearAnalizador();
        await using var contenido = new MemoryStream();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => analizador.AnalizarAsync(
                contenido,
                "documento.pdf"));
    }

    /// <summary>
    /// Verifica que se rechacen documentos que superan el tamaño permitido.
    /// </summary>
    [Fact]
    public async Task AnalizarAsync_TamanoExcedido_DebeRechazarDocumento()
    {
        var analizador = CrearAnalizador(10);
        await using var contenido = new MemoryStream(
            Encoding.ASCII.GetBytes("12345678901"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => analizador.AnalizarAsync(
                contenido,
                "documento.pdf"));
    }

    /// <summary>
    /// Verifica que el analizador restaure la posición original del flujo.
    /// </summary>
    [Fact]
    public async Task AnalizarAsync_ContenidoInvalido_DebeRestaurarPosicion()
    {
        var analizador = CrearAnalizador();

        await using var contenido = new MemoryStream(
            Encoding.ASCII.GetBytes("XX%PDF-"));

        contenido.Position = 2;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => analizador.AnalizarAsync(
                contenido,
                "documento.pdf"));

        Assert.Equal(2, contenido.Position);
    }

    /// <summary>
    /// Construye un analizador con el tamaño máximo indicado.
    /// </summary>
    /// <param name="tamanoMaximo">Tamaño máximo permitido en bytes.</param>
    /// <returns>Instancia configurada del analizador.</returns>
    private static AnalizadorDocumentos CrearAnalizador(
        long tamanoMaximo = 1024 * 1024)
    {
        var opciones = Options.Create(
            new AlmacenamientoDocumentosOptions
            {
                RutaBase = "App_Data/Documentos",
                TamanoMaximoBytes = tamanoMaximo
            });

        return new AnalizadorDocumentos(opciones);
    }



    /// <summary>
    /// Verifica que se analice correctamente un PDF válido de dos páginas.
    /// </summary>
    [Fact]
    public async Task AnalizarAsync_PdfValido_DebeObtenerCantidadPaginas()
    {
        var analizador = CrearAnalizador();
        await using var contenido = new MemoryStream(CrearPdfValido());

        var resultado = await analizador.AnalizarAsync(
            contenido,
            "documento.pdf");

        Assert.Equal(".pdf", resultado.Extension);
        Assert.Equal(2, resultado.CantidadPaginas);
        Assert.Equal(0, contenido.Position);
    }

    /// <summary>
    /// Genera un PDF mínimo válido de dos páginas para las pruebas.
    /// </summary>
    /// <returns>Contenido binario del PDF generado.</returns>
    private static byte[] CrearPdfValido()
    {
        var pdf = new StringBuilder();
        var posiciones = new List<long>();

        pdf.Append("%PDF-1.4\n");

        var objetos = new[]
        {
            "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n",
            "2 0 obj\n<< /Type /Pages /Kids [3 0 R 4 0 R] /Count 2 >>\nendobj\n",
            "3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << >> >>\nendobj\n",
            "4 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << >> >>\nendobj\n"
        };

        foreach (var objeto in objetos)
        {
            posiciones.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
            pdf.Append(objeto);
        }

        var posicionXref = Encoding.ASCII.GetByteCount(pdf.ToString());

        pdf.Append("xref\n0 5\n");
        pdf.Append("0000000000 65535 f \n");

        foreach (var posicion in posiciones)
        {
            pdf.Append($"{posicion:D10} 00000 n \n");
        }

        pdf.Append("trailer\n<< /Size 5 /Root 1 0 R >>\n");
        pdf.Append($"startxref\n{posicionXref}\n");
        pdf.Append("%%EOF\n");

        return Encoding.ASCII.GetBytes(pdf.ToString());
    }

}
