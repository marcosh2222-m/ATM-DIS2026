using System;

namespace CajeroATM.Datos
{
    /// <summary>
    /// Error técnico de acceso a datos (sin conexión, credenciales mal,
    /// falta un stored procedure, etc.), con un mensaje en castellano.
    /// Las capas de arriba atrapan esta excepción y nunca necesitan
    /// conocer los tipos de MySqlConnector.
    ///
    /// Importante: los rechazos de negocio (saldo insuficiente, PIN
    /// incorrecto...) NO son excepciones: vuelven como código de resultado.
    /// </summary>
    public class DatosException : Exception
    {
        public DatosException(string mensaje, Exception causa) : base(mensaje, causa) { }
    }
}
