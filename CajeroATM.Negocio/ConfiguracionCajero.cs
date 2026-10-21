using System;
using System.Configuration;

namespace CajeroATM.Negocio
{
    /// <summary>Valores de App.config que usa la capa de Negocio.</summary>
    public static class ConfiguracionCajero
    {
        /// <summary>Cajero físico donde corre la aplicación (appSettings "IdCajero").</summary>
        public static int IdCajero
        {
            get
            {
                string valor = ConfigurationManager.AppSettings["IdCajero"];
                if (!int.TryParse(valor, out int idCajero) || idCajero <= 0)
                    throw new CajeroException("Falta o es inválido el valor \"IdCajero\" en App.config (appSettings).", null);
                return idCajero;
            }
        }
    }
}
