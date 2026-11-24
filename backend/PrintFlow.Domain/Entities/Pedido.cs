using PrintFlow.Domain.Enums;

namespace PrintFlow.Domain.Entities;

/// <summary>
/// Representa una solicitud de impresión realizada por un cliente
/// Un pedido puede contener varios documentos 
/// </summary>

public class Pedido
{
    // Identificador único del pedido
    public Guid Id {get; private set;}

    // Número para identificar el pedido
    public string NumeroPedido {get; private set;}
    
    // Nombre del cliente que solicita la impresión de los docs
    public string? ClienteNombre {get; private set;}

    // Número telefónico del cliente
    public string ClienteTelefono {get; private set;}

    // Estado del pedido
    public EstadoPedido Estado {get; private set;}

    //Fecha y hora de la creación del pedido
    public DateTime FechaCreacion {get; private set;}

    // Documentos asociados al pedido
    private readonly List<Documento> _documentos  = new();

    // las demás clasese pueden consultar los documentos, pero no modificar directamente la colección
    public IReadOnlyCollection<Documento> Documentos => _documentos.AsReadOnly();

    // Constructor para poder crear el pedido
    public Pedido(string numeroPedido, string clienteNombre, string clienteTelefono)
    {   
        // Validación de que exista un número de pedido asignado
        if (string.IsNullOrWhiteSpace(numeroPedido))
            throw new ArgumentException("El número de pedido es obligatorio");
        

        // Validación de existencia del nombre del cliente
        if (string.IsNullOrWhiteSpace(clienteNombre))
            throw new ArgumentException("El nombre del cliente es obligatorio");
    

        // Validación del número del cliente
        if (string.IsNullOrWhiteSpace(clienteTelefono))
            throw new ArgumentException("El número del cliente es obligatorio");

        Id = Guid.NewGuid();
        NumeroPedido = numeroPedido;
        ClienteNombre = clienteNombre;
        ClienteTelefono = clienteTelefono;
        
        // Todo pedido nuevo inicia en pendiente
        Estado = EstadoPedido.Pendiente;

        FechaCreacion = DateTime.UtcNow;
    }   

    /// <summary>
    /// Agrega un documento al pedido.
    /// </summary>
    public void AgregarDocumento(Documento documento)
    {
        //Válidacion para no recibir documentos vacios
        if (documento is null)
        throw new ArgumentNullException(nameof(documento));

        // Validación para que solo perimit agregar docs mientras el pedido está pendiente
        if ( Estado != EstadoPedido.Pendiente)
            throw new InvalidOperationException("No se pueden agregar documentos a un pedido que ya fue procesado");

        // Validación para evitar 2 veces el mismo doc
        if(_documentos.Contains(documento))
        throw new InvalidOperationException("El documentto ya está agregado al pedido");

        // Un doc no puede pertenecer a dos pedidos
        documento.AsociarPedido(Id);

        _documentos.Add(documento);
    }

     /// <summary>
    /// Aprueba el pedido para continuar con el proceso de impresión.
    /// </summary>
    public void Aprobar()
    {   

        // Validación que el estado del pedido se encuentre en penddiente antes de aprobar
        if (Estado != EstadoPedido.Pendiente)
            throw new InvalidOperationException("El pedido ya no se encuentra pendiente");

        // Validación para que un pedido no se encuentre vacio (sin documentos)
        if(_documentos.Count == 0)
            throw new InvalidOperationException("No se puede aprobar un pedido sin docs");
    
        Estado = EstadoPedido.Aprobado;
    }

    /// <summary>
    /// Rechaza un pedido que todavía se encuentra pendiente.
    /// </summary>
    public void Rechazar()
    {
        // Validación para que solo se puedan rechazar pendientes
        if (Estado != EstadoPedido.Pendiente)
        {
            throw new InvalidOperationException("SOlamente se pueden rechazar pedidos pendientes");
        }

        Estado = EstadoPedido.Rechazado;
    }

    /// <summary>
    /// Modifica la cantidad de copias de un documento asociad al pedido, siempre que este permanezca pendiente.
    /// </summary>
    public void CambiarCopiasDocumento(Guid documentoId, int numeroCopias)
    {
        if (Estado != EstadoPedido.Pendiente)
            throw new InvalidOperationException(
                "No se pueden modificar documentos de un pedido procesado.");

        var documento = _documentos.FirstOrDefault(d => d.Id == documentoId);

        if (documento is null)
            throw new InvalidOperationException(
                "El documento no pertenece a este pedido.");

        documento.CambiarNumeroCopias(numeroCopias);
    }
    
    /// <summary>
    /// Modifica la preferencia de impresión a color de un documento asociado al pedido, siempre que este permanezca pendiente.
    /// </summary>
    public void CambiarColorDocumento(Guid documentoId, bool esColor)
    {
        if (Estado != EstadoPedido.Pendiente)
            throw new InvalidOperationException(
                "No se pueden modificar documentos de un pedido procesado.");

        var documento = _documentos.FirstOrDefault(d => d.Id == documentoId);

        if (documento is null)
            throw new InvalidOperationException(
                "El documento no pertenece a este pedido.");

        documento.CambiarPreferenciaColor(esColor);
    }
}