
using PrintFlow.Application.DTOs;
using PrintFlow.Application.Services;
using PrintFlow.Domain.Enums;
using PrintFlow.UnitTests.Fakes;

namespace PrintFlow.UnitTests.Services;

/// <summary>
/// Contiene las pruebas unitarias del servicio encargado
/// de registrar pedidos de impresión.
/// </summary>
public class RegistrarPedidoServiceTests
{
    /// <summary>
    /// Comprueba que se registre correctamente un pedido
    /// con varios documentos asociados.
    /// </summary>
    [Fact]
    public async Task RegistrarAsync_ConPedidoValido_DebeRegistrarPedidoYDocumentos()
    {
        // Arrange: prepara el repositorio falso y el servicio.
        var repositorio = new FakePedidoRepository();
        var servicio = new RegistrarPedidoService(repositorio);

        // Prepara una solicitud con dos documentos.
        var request = new RegistrarPedidoRequest
        {
            NumeroPedido = "PED-001",
            ClienteNombre = "Cliente de prueba",
            ClienteTelefono = "999999999",
            Documentos = new List<DocumentoPedidoRequest>
            {
                new()
                {
                    NombreArchivo = "documento1.pdf",
                    Extension = ".pdf",
                    RutaAlmacenamiento = "pedidos/PED-001/documento1.pdf",
                    CantidadPaginas = 5,
                    NumeroCopias = 2,
                    EsColor = false
                },
                new()
                {
                    NombreArchivo = "documento2.pdf",
                    Extension = ".pdf",
                    RutaAlmacenamiento = "pedidos/PED-001/documento2.pdf",
                    CantidadPaginas = 3,
                    NumeroCopias = 1,
                    EsColor = true
                }
            }
        };

        // Act: ejecuta el registro del pedido.
        var pedidoId = await servicio.RegistrarAsync(request);

        // Assert: comprueba que se haya generado el identificador.
        Assert.NotEqual(Guid.Empty, pedidoId);

        // Comprueba que el pedido se haya agregado al repositorio.
        var pedido = Assert.Single(repositorio.Pedidos);

        Assert.Equal(pedidoId, pedido.Id);
        Assert.Equal("PED-001", pedido.NumeroPedido);
        Assert.Equal(EstadoPedido.Pendiente, pedido.Estado);

        // Comprueba que ambos documentos estén asociados al pedido.
        Assert.Equal(2, pedido.Documentos.Count);

        Assert.All(
            pedido.Documentos,
            documento => Assert.Equal(pedido.Id, documento.PedidoId));

        // Comprueba que se haya solicitado guardar los cambios.
        Assert.Equal(1, repositorio.GuardarCambiosCount);
    }

    /// <summary>
    /// Comprueba que no se permita registrar un pedido que no contiene documentos.
    /// </summary>
    [Fact]
    public async Task RegistrarAsync_SinDocumentos_DebeLanzarExcepcion()
    {
        // Arrange: prepara el servicio y una solicitud vacía.
        var repositorio = new FakePedidoRepository();
        var servicio = new RegistrarPedidoService(repositorio);

        var request = new RegistrarPedidoRequest
        {
            NumeroPedido = "PED-002",
            ClienteNombre = "Cliente de prueba",
            ClienteTelefono = "999999999",
            Documentos = new List<DocumentoPedidoRequest>()
        };

        // Act y Assert: verifica que se lance la excepción esperada.
        await Assert.ThrowsAsync<ArgumentException>(
            () => servicio.RegistrarAsync(request));

        // Comprueba que no se haya guardado ningún pedido.
        Assert.Empty(repositorio.Pedidos);
        Assert.Equal(0, repositorio.GuardarCambiosCount);
    }

    /// <summary>
    /// Comprueba que un documento con una cantidad de copias
    /// inválida impida registrar el pedido.
    /// </summary>
    [Fact]
    public async Task RegistrarAsync_ConCopiasInvalidas_DebeLanzarExcepcion()
    {
        // Arrange: prepara el servicio y un documento inválido.
        var repositorio = new FakePedidoRepository();
        var servicio = new RegistrarPedidoService(repositorio);

        var request = new RegistrarPedidoRequest
        {
            NumeroPedido = "PED-003",
            ClienteNombre = "Cliente de prueba",
            ClienteTelefono = "999999999",
            Documentos = new List<DocumentoPedidoRequest>
            {
                new()
                {
                    NombreArchivo = "documento.pdf",
                    Extension = ".pdf",
                    RutaAlmacenamiento = "pedidos/PED-003/documento.pdf",
                    CantidadPaginas = 5,
                    NumeroCopias = 0,
                    EsColor = false
                }
            }
        };

        // Act y Assert: el constructor de Documento debe rechazar
        // una cantidad de copias igual a cero.
        await Assert.ThrowsAsync<ArgumentException>(
            () => servicio.RegistrarAsync(request));

        // Comprueba que el pedido no se haya guardado.
        Assert.Empty(repositorio.Pedidos);
        Assert.Equal(0, repositorio.GuardarCambiosCount);
    }
}
