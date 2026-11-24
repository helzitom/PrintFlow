namespace PrintFlow.Domain.Enums;

/// <summary>
/// Representa los estados posibles de un pedido
/// dentro del flujo de impresión de PrintFlow.
/// </summary>
public enum EstadoPedido
{
    Pendiente = 1,
    Aprobado = 2,
    Rechazado = 3,
    EnCola = 4,
    EnviadoAImpresora = 5,
    Impreso = 6,
    Error = 7

}