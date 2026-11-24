
using Microsoft.AspNetCore.Mvc;
using PrintFlow.API.Contracts.Pedidos;
using PrintFlow.Application.DTOs;
using PrintFlow.Application.DTOs.Documentos;
using PrintFlow.Application.Interfaces;
using PrintFlow.Application.Interfaces.Documentos;
using PrintFlow.Application.Services;

namespace PrintFlow.API.Controllers;

/// <summary>
/// Expone los endpoints HTTP para registrar, consultar y gestionar pedidos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private const int MaximoDocumentosPorPedido = 5;

    private readonly IPedidoRepository _pedidoRepository;
    private readonly RegistrarPedidoService _registrarPedidoService;
    private readonly IAlmacenamientoDocumentos _almacenamiento;
    private readonly IAnalizadorDocumentos _analizador;
    private readonly ILogger<PedidosController> _logger;

    /// <summary>
    /// Inicializa el controlador con sus dependencias.
    /// </summary>
    public PedidosController(
        IPedidoRepository pedidoRepository,
        RegistrarPedidoService registrarPedidoService,
        IAlmacenamientoDocumentos almacenamiento,
        IAnalizadorDocumentos analizador,
        ILogger<PedidosController> logger)
    {
        _pedidoRepository = pedidoRepository;
        _registrarPedidoService = registrarPedidoService;
        _almacenamiento = almacenamiento;
        _analizador = analizador;
        _logger = logger;
    }

    /// <summary>
    /// Registra un pedido y almacena sus documentos PDF.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(110 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RegistrarAsync(
        [FromForm] RegistrarPedidoForm form,
        CancellationToken cancellationToken)
    {
        // Valida los datos principales del pedido.
        if (string.IsNullOrWhiteSpace(form.NumeroPedido) ||
            string.IsNullOrWhiteSpace(form.ClienteNombre) ||
            string.IsNullOrWhiteSpace(form.ClienteTelefono))
        {
            return BadRequest(new
            {
                mensaje = "Los datos del cliente y el número del pedido son obligatorios."
            });
        }

        // Valida la cantidad de documentos recibidos.
        if (form.Documentos is null ||
            form.Documentos.Count == 0 ||
            form.Documentos.Count > MaximoDocumentosPorPedido)
        {
            return BadRequest(new
            {
                mensaje = $"El pedido debe contener entre 1 y {MaximoDocumentosPorPedido} documentos."
            });
        }

        var documentosAnalizados =
            new List<(IFormFile Archivo, string NombreArchivo,
                ResultadoAnalisisDocumento Analisis,
                int NumeroCopias, bool EsColor)>();

        // Analiza todos los documentos antes de almacenarlos.
        foreach (var documento in form.Documentos)
        {
            if (documento?.Archivo is null ||
                documento.Archivo.Length <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "Todos los documentos deben contener un archivo válido."
                });
            }

            // Obtiene el nombre del archivo sin permitir rutas proporcionadas por el cliente.
            var nombreArchivo = Path.GetFileName(
                documento.Archivo.FileName.Replace(
                    '/',
                    Path.DirectorySeparatorChar));

            if (string.IsNullOrWhiteSpace(nombreArchivo))
            {
                return BadRequest(new
                {
                    mensaje = "Uno de los archivos tiene un nombre inválido."
                });
            }

            var extension = Path.GetExtension(nombreArchivo)
                .ToLowerInvariant();

            // Por ahora, el sistema admite únicamente documentos PDF.
            if (extension != ".pdf")
            {
                return BadRequest(new
                {
                    mensaje = $"El archivo {nombreArchivo} no tiene extensión PDF."
                });
            }

            // Valida la cantidad de copias solicitadas.
            if (documento.NumeroCopias < 1 ||
                documento.NumeroCopias > 100)
            {
                return BadRequest(new
                {
                    mensaje = "La cantidad de copias debe estar entre 1 y 100."
                });
            }

            try
            {
                await using var contenido =
                    documento.Archivo.OpenReadStream();

                var resultado = await _analizador.AnalizarAsync(
                    contenido,
                    nombreArchivo,
                    cancellationToken);

                documentosAnalizados.Add((
                    documento.Archivo,
                    nombreArchivo,
                    resultado,
                    documento.NumeroCopias,
                    documento.EsColor
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    mensaje = $"No se pudo procesar {nombreArchivo}: {ex.Message}"
                });
            }
        }

        var rutasAlmacenadas = new List<string>();

        try
        {
            var documentosPedido = new List<DocumentoPedidoRequest>();

            // Almacena los documentos y construye sus datos internos.
            foreach (var documento in documentosAnalizados)
            {
                await using var contenido =
                    documento.Archivo.OpenReadStream();

                var ruta = await _almacenamiento.GuardarAsync(
                    contenido,
                    documento.NombreArchivo,
                    documento.Analisis.Extension,
                    cancellationToken);

                rutasAlmacenadas.Add(ruta);

                documentosPedido.Add(new DocumentoPedidoRequest
                {
                    NombreArchivo = documento.NombreArchivo,
                    Extension = documento.Analisis.Extension,
                    RutaAlmacenamiento = ruta,
                    CantidadPaginas = documento.Analisis.CantidadPaginas,
                    NumeroCopias = documento.NumeroCopias,
                    EsColor = documento.EsColor
                });
            }

            // Construye la solicitud interna del caso de uso.
            var solicitud = new RegistrarPedidoRequest
            {
                NumeroPedido = form.NumeroPedido,
                ClienteNombre = form.ClienteNombre,
                ClienteTelefono = form.ClienteTelefono,
                Documentos = documentosPedido
            };

            // Registra el pedido y obtiene su identificador.
            var pedidoId = await _registrarPedidoService.RegistrarAsync(
                solicitud);

            // Genera la respuesta utilizando la ruta GET identificada.
            return CreatedAtRoute(
                "ObtenerPedidoPorId",
                new { id = pedidoId },
                new { id = pedidoId });
        }
        catch
        {
            // Elimina los archivos almacenados si falla el registro.
            foreach (var ruta in rutasAlmacenadas)
            {
                try
                {
                    await _almacenamiento.EliminarAsync(
                        ruta,
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "No se pudo eliminar el archivo {Ruta} durante la compensación.",
                        ruta);
                }
            }

            throw;
        }
    }

    /// <summary>
    /// Consulta un pedido por su identificador e incluye sus documentos.
    /// </summary>
    [HttpGet("{id:guid}", Name = "ObtenerPedidoPorId")]
    [ProducesResponseType(typeof(PedidoDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PedidoDetalleResponse>> ObtenerPorIdAsync(
        Guid id)
    {
        var pedido = await _pedidoRepository.ObtenerPorIdAsync(id);

        if (pedido is null)
            return NotFound();

        var response = new PedidoDetalleResponse
        {
            Id = pedido.Id,
            NumeroPedido = pedido.NumeroPedido,
            ClienteNombre = pedido.ClienteNombre,
            ClienteTelefono = pedido.ClienteTelefono,
            Estado = pedido.Estado.ToString(),
            FechaCreacion = pedido.FechaCreacion,
            Documentos = pedido.Documentos.Select(documento =>
                new DocumentoResponse
                {
                    Id = documento.Id,
                    NombreArchivo = documento.NombreArchivo,
                    Extension = documento.Extension,
                    CantidadPaginas = documento.CantidadPaginas,
                    NumeroCopias = documento.NumeroCopias,
                    EsColor = documento.EsColor
                }).ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Aprueba un pedido pendiente para permitir que continúe el flujo de impresión.
    /// </summary>
    [HttpPost("{id:guid}/aprobar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AprobarAsync(Guid id)
    {
        var pedido = await _pedidoRepository.ObtenerPorIdAsync(id);

        if (pedido is null)
            return NotFound();

        try
        {
            pedido.Aprobar();

            await _pedidoRepository.GuardarCambiosAsync();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Rechaza un pedido que todavía se encuentra pendiente.
    /// </summary>
    [HttpPost("{id:guid}/rechazar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RechazarAsync(Guid id)
    {
        var pedido = await _pedidoRepository.ObtenerPorIdAsync(id);

        if (pedido is null)
            return NotFound();

        try
        {
            pedido.Rechazar();

            await _pedidoRepository.GuardarCambiosAsync();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }
}
