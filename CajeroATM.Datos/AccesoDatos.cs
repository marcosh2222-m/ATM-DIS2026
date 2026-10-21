using System;
using System.Data;
using MySqlConnector;

namespace CajeroATM.Datos
{
    /// <summary>
    /// Base de SucursalDB y CentralDB: abre la conexión, arma comandos
    /// de stored procedure y traduce los errores de MySQL a DatosException.
    /// Todo el acceso a la base pasa por stored procedures (nunca SQL embebido).
    /// </summary>
    public abstract class AccesoDatos
    {
        private readonly string _cadenaConexion;

        /// <param name="cadenaConexion">
        /// null = usar la cadena "CajeroATM" de App.config (lo normal).
        /// Se puede pasar otra para pruebas.
        /// </param>
        protected AccesoDatos(string cadenaConexion)
        {
            _cadenaConexion = cadenaConexion;
        }

        /// <summary>
        /// Abre una conexión, ejecuta la acción y la cierra siempre.
        /// Cualquier error técnico sale como DatosException.
        /// </summary>
        protected T Ejecutar<T>(string nombreSP, Func<MySqlConnection, T> accion)
        {
            try
            {
                using (MySqlConnection conexion = string.IsNullOrWhiteSpace(_cadenaConexion)
                           ? Conexion.CrearConexion()
                           : new MySqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    return accion(conexion);
                }
            }
            catch (MySqlException ex)
            {
                throw new DatosException(TraducirError(ex, nombreSP), ex);
            }
            catch (InvalidOperationException ex)   // p. ej. falta la cadena en App.config
            {
                throw new DatosException(ex.Message, ex);
            }
        }

        protected static MySqlCommand CrearComando(MySqlConnection conexion, string nombreSP)
        {
            return new MySqlCommand(nombreSP, conexion) { CommandType = CommandType.StoredProcedure };
        }

        protected static void AgregarEntrada(MySqlCommand comando, string nombre, MySqlDbType tipo, object valor)
        {
            comando.Parameters.Add(new MySqlParameter(nombre, tipo)
            {
                Direction = ParameterDirection.Input,
                Value = valor ?? DBNull.Value,
            });
        }

        protected static MySqlParameter AgregarSalida(MySqlCommand comando, string nombre, MySqlDbType tipo)
        {
            var parametro = new MySqlParameter(nombre, tipo) { Direction = ParameterDirection.Output };
            comando.Parameters.Add(parametro);
            return parametro;
        }

        // ---------------------------------------------------------
        // Lectura segura de parámetros OUT (pueden volver NULL)
        // ---------------------------------------------------------
        protected static string LeerTexto(MySqlParameter parametro)
        {
            return EsNulo(parametro.Value) ? null : Convert.ToString(parametro.Value);
        }

        protected static int? LeerEntero(MySqlParameter parametro)
        {
            return EsNulo(parametro.Value) ? (int?)null : Convert.ToInt32(parametro.Value);
        }

        protected static decimal? LeerDecimal(MySqlParameter parametro)
        {
            return EsNulo(parametro.Value) ? (decimal?)null : Convert.ToDecimal(parametro.Value);
        }

        protected static bool EsNulo(object valor)
        {
            return valor == null || valor == DBNull.Value;
        }

        /// <summary>Errores frecuentes de MySQL traducidos a algo accionable.</summary>
        private static string TraducirError(MySqlException ex, string nombreSP)
        {
            switch (ex.Number)
            {
                case 1042:   // no se pudo resolver/llegar al host
                    return "No se pudo conectar al servidor MySQL. Verificá que el servicio esté iniciado y el Server/Port de App.config.";
                case 1044:
                    return "El usuario de MySQL no tiene permisos sobre la base (o la base no existe). Revisá Database y User ID en App.config.";
                case 1045:
                    return "MySQL rechazó el usuario o la contraseña de la cadena de conexión (App.config).";
                case 1049:
                    return "No existe la base de datos. Ejecutá 01_CrearBaseDatos_CajeroATM_MySQL.sql.";
                case 1305:
                    return "No existe el procedimiento " + nombreSP + ". Ejecutá 02_StoredProcedures_CajeroATM_MySQL.sql.";
                case 1452:
                    return "La operación hace referencia a un registro inexistente (tarjeta, cajero o cuenta).";
                case 1205:
                case 1213:
                    return "La cuenta está siendo usada por otra operación. Intente nuevamente.";
                default:
                    return "Error de base de datos en " + nombreSP + ": " + ex.Message;
            }
        }
    }
}
