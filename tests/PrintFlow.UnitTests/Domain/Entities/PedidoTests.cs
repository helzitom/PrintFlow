using PrintFlow.Domain.Entities;
using PrintFlow.Domain.Enums;

namespace PrintFlow.UnitTests.Domain.Entities;
    
//Usando el patrón AAA para testear
public class PedidoTests
{   


    [Fact]
    public void Constructor_DebeCrearPedidoEnEstadoPendiente()
    {
        
        //Arrange: preparamos los datos del pedido
        var pedido = new Pedido("PED-001", "Cliente de pruebitas","999999999");

        // Assert: verificamos el estado inicial
        Assert.Equal(EstadoPedido.Pendiente, pedido.Estado);
    }

    [Fact]
    public void Aprobar_DebeLanzarError_CuandoPedidoNoTieneDoc()
    {
        // Arrange
        var pedido = new Pedido( "PED-002", "Cliente de prueba", "999999999");

        // Act y Assert
        Assert.Throws<InvalidOperationException>(() => pedido.Aprobar());

        Assert.Equal(EstadoPedido.Pendiente, pedido.Estado);
    } 

    [Fact]
    public void Aprobar_DebeCambiarEstadoAprobado_CuandoTieneDocumentos()
    {
        // Arrange
        var pedido = new Pedido("PED-003", "Cliente de prueba", "999999999"
        );

        var documento = new Documento("informe.pdf", ".pdf", "pedidos/PED-003/informe.pdf", 10, 2, false
        );

        pedido.AgregarDocumento(documento);

        // Act
        pedido.Aprobar();

        // Assert
        Assert.Equal(EstadoPedido.Aprobado, pedido.Estado);
    }

    [Fact]
    public void CambiarNumeroCopias_DebeActualizarCantidad_CuandoPedidoEstaPendiente()
    {
        // Arrange
        var pedido = new Pedido(
            "PED-004",
            "Cliente de prueba",
            "999999999"
        );

        var documento = new Documento(
            "informe.pdf",
            ".pdf",
            "pedidos/PED-004/informe.pdf",
            10,
            2,
            false
        );

        pedido.AgregarDocumento(documento);

        // Act
        pedido.CambiarCopiasDocumento(documento.Id, 5);

        // Assert
        Assert.Equal(5, documento.NumeroCopias);
    }

    [Fact]
    public void CambiarNumeroCopias_DebeLanzarError_CuandoPedidoEstaAprobado()
    {
        // Arrange
        var pedido = new Pedido(
            "PED-005",
            "Cliente de prueba",
            "999999999"
        );

        var documento = new Documento(
            "informe.pdf",
            ".pdf",
            "pedidos/PED-005/informe.pdf",
            10,
            2,
            false
        );

        pedido.AgregarDocumento(documento);
        pedido.Aprobar();

        // Act y Assert
        Assert.Throws<InvalidOperationException>(() =>
            pedido.CambiarCopiasDocumento(documento.Id, 5)
        );

        Assert.Equal(2, documento.NumeroCopias);
    }

    [Fact]
    public void CambiarNumeroCopias_DebeLanzarError_CuandoDocumentoNoPerteneceAlPedido()
    {
        // Arrange
        var pedido1 = new Pedido(
            "PED-006",
            "Cliente 1",
            "999999999"
        );

        var pedido2 = new Pedido(
            "PED-007",
            "Cliente 2",
            "988888888"
        );

        var documento = new Documento(
            "informe.pdf",
            ".pdf",
            "pedidos/PED-007/informe.pdf",
            10,
            2,
            false
        );

        pedido2.AgregarDocumento(documento);

        // Act y Assert
        Assert.Throws<InvalidOperationException>(() =>
            pedido1.CambiarCopiasDocumento(documento.Id, 5)
        );

        Assert.Equal(2, documento.NumeroCopias);
    }
}