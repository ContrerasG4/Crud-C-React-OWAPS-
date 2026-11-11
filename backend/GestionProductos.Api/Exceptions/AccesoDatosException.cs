namespace GestionProductos.Api.Exceptions;

/// <summary>Falla de base de datos. El detalle técnico se registra en el log, nunca se envía al cliente.</summary>
public class AccesoDatosException(string mensaje, Exception inner) : Exception(mensaje, inner);
