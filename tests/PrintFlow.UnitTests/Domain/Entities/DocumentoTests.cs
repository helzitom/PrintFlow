using PrintFlow.Domain.Entities;

namespace PrintFlow.UnitTests.Domain.Entities;

public class DocumentoTests
{
    [Fact]
    public void Constructor_DebeCrearDocumentoConDatosValidos()
    {
        var documento = new Documento(
            "informe.pdf",
            ".pdf",
            "pedidos/PED-001/informe.pdf",
            10,
            2,
            false
        );

        Assert.Equal("informe.pdf", documento.NombreArchivo);
        Assert.Equal(10, documento.CantidadPaginas);
        Assert.Equal(2, documento.NumeroCopias);
        Assert.False(documento.EsColor);
    }

    [Fact]
    public void Constructor_DebeLanzarError_CuandoNumeroCopiasEsCero()
    {
        Assert.Throws<ArgumentException>(() =>
            new Documento(
                "informe.pdf",
                ".pdf",
                "pedidos/PED-001/informe.pdf",
                10,
                0,
                false
            )
        );
    }

    [Fact]
    public void CambiarNumeroCopias_DebeLanzarError_CuandoEsCero()
    {
        // Arrange
        var pedido = new Pedido(
            "PED-003",
            "Cliente de prueba",
            "999999999"
        );

        var documento = new Documento(
            "informe.pdf",
            ".pdf",
            "pedidos/PED-003/informe.pdf",
            10,
            2,
            false
        );

        pedido.AgregarDocumento(documento);

        // Act y Assert
        Assert.Throws<ArgumentException>(() =>
            pedido.CambiarCopiasDocumento(documento.Id, 0)
        );

        // Verificamos que el valor original no haya cambiado.
        Assert.Equal(2, documento.NumeroCopias);
    }
}