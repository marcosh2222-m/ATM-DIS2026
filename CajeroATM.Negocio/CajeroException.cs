using System;

namespace CajeroATM.Negocio
{
    /// <summary>
    /// Error técnico visto desde la Presentación (sin conexión, configuración
    /// incompleta...). Envuelve a DatosException para que los formularios
    /// no dependan de la capa de Datos. El mensaje ya está en castellano.
    /// </summary>
    public class CajeroException : Exception
    {
        public CajeroException(string mensaje, Exception causa) : base(mensaje, causa) { }
    }
}
