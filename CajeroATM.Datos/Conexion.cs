using System;
using System.Configuration;
using MySqlConnector;

namespace CajeroATM.Datos
{
    /// <summary>
    /// Punto único de acceso a la conexión MySQL. La cadena se lee del
    /// App.config del proyecto de Presentación (nombre "CajeroATM").
    /// SucursalDB y CentralDB crean sus conexiones a través de AccesoDatos.
    /// </summary>
    public static class Conexion
    {
        public const string NOMBRE_CADENA = "CajeroATM";

        public static string CadenaConexion
        {
            get
            {
                ConnectionStringSettings config = ConfigurationManager.ConnectionStrings[NOMBRE_CADENA];
                if (config == null || string.IsNullOrWhiteSpace(config.ConnectionString))
                {
                    throw new InvalidOperationException(
                        "No se encontró la cadena de conexión \"" + NOMBRE_CADENA + "\" en App.config.");
                }
                return config.ConnectionString;
            }
        }

        /// <summary>Crea una conexión nueva (cerrada). Usar siempre dentro de un using.</summary>
        public static MySqlConnection CrearConexion()
        {
            return new MySqlConnection(CadenaConexion);
        }

        /// <summary>
        /// Abre y cierra una conexión para verificar que la cadena, el
        /// servidor y la base existan. No lanza excepciones.
        /// </summary>
        public static bool ProbarConexion(out string mensaje)
        {
            try
            {
                using (MySqlConnection conexion = CrearConexion())
                {
                    conexion.Open();
                    mensaje = "Conectado a MySQL " + conexion.ServerVersion + " (base: " + conexion.Database + ").";
                    return true;
                }
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }
    }
}
