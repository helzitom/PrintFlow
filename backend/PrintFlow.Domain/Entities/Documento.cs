namespace PrintFlow.Domain.Entities;


/// <summary>
/// Representa un archivo individual que forma parte
/// de un pedido de impresión.
/// </summary>
public class Documento
{
    // Identificador del doc
    public Guid Id {get ; private set;}

    // Nombre original del archivo recibido
    public string? NombreArchivo {get; private set;}

    // Extensión del archivo
    public string? Extension { get; private set;}

    // Ubicación donde se almacena del archivo
    public string? RutaAlmacenamiento {get; private set;}

    // Cantidad total de páginas del doc
    public int CantidadPaginas {get; private set;}

    // Número de copias solicitadas del documento
    public int NumeroCopias {get; private set;}

    // Se indica si es a color o no
    public bool EsColor {get; private set;}

    // Identificador del pedido al cual pertenece
    public Guid PedidoId {get; private set;}

    // Constructor para crear un documento válido 
    public Documento (string nombreArchivo, string extension, string rutaAlmacenamiento, int cantidadPaginas, int numeroCopias, bool esColor)
    {
        // Se valida que el nombre del archivo no llegue vacio o contenga espacios en blanco
        if (string.IsNullOrWhiteSpace(nombreArchivo))
            throw new ArgumentException("El nombre del archivo es obligatorio");

        // Se valida que la extensión del doc no llegue vacia o tenga espacios en blanco
        if (string.IsNullOrWhiteSpace(extension))
            throw new ArgumentException("La extensión es obligatoria");
    
        // Se valida que la ruta de almacenamiento sea la aedcuada
        if (string.IsNullOrWhiteSpace(rutaAlmacenamiento))
            throw new ArgumentException("La ruta de almacenamiento es obligatoria.");

        // Se valida que la cantidad de páginas no sea menor o igual a 0
        if (cantidadPaginas <= 0)
            throw new ArgumentException("La cantidad de páginas debe ser mayor a 0");
        
        // Se valida que el número de copias no sea menor o igual a 0
        if (numeroCopias <= 0)
            throw new ArgumentException("La cantidad de copias debe ser mayor a 0");
        
        Id = Guid.NewGuid();
        NombreArchivo = nombreArchivo;
        Extension = extension;
        RutaAlmacenamiento = rutaAlmacenamiento;
        CantidadPaginas = cantidadPaginas;
        NumeroCopias = numeroCopias;
        EsColor = esColor;
    }

    /// <summary>
    /// Acá se asocia el documento con un pedido.
    /// Solo puede invocarse desde el Domain.
    /// </summary>
    /// 
    internal void AsociarPedido(Guid pedidoId)
    {
        if (pedidoId == Guid.Empty)
            throw new ArgumentException(" El ID del pedido no es válido");
    
        if (PedidoId != Guid.Empty)
            throw new InvalidOperationException("El documento ya pertenece a otro pedido");

            PedidoId = pedidoId;
    }

    /// <summary>
    /// Esta función es para modificar la cantidad de copias solicitadas.
    /// </summary>
    internal void CambiarNumeroCopias(int numeroCopias)
    {
        if(numeroCopias <= 0)
            throw new ArgumentException("El número de copias debe ser mayor a 0");
    
        NumeroCopias = numeroCopias;
    }

    /// <summary>
    /// Modifica l preferencia de impresión a color
    ///
    internal void CambiarPreferenciaColor(bool esColor)
    {
        EsColor = esColor;
    }

}
