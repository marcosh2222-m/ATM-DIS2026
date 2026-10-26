using System;
using CajeroATM.Datos.Resultados;
using CajeroATM.Entidades;
using MySqlConnector;

namespace CajeroATM.Datos
{
    /// <summary>
    /// Actor SUCURSAL: flujo raíz del login. Valida la tarjeta
    /// (CU Autenticar Usuario, pasos 3 y 4). Envuelve USP_ObtenerDatosLogin.
    /// El PIN lo valida Red Central (CentralDB.ValidarPIN).
    /// </summary>
    public class SucursalDB : AccesoDatos
    {
        public SucursalDB() : base(null) { }

        public SucursalDB(string cadenaConexion) : base(cadenaConexion) { }

        /// <summary>
        /// Paso 1 del login: trae la tarjeta y su Salt (para calcular el hash del PIN).
        /// Devuelve null si el número de tarjeta no existe.
        /// </summary>
        public DatosLogin ObtenerDatosLogin(string numeroTarjeta)
        {
            if (string.IsNullOrWhiteSpace(numeroTarjeta))
                throw new ArgumentException("El número de tarjeta es obligatorio.", nameof(numeroTarjeta));

            const string sp = "USP_ObtenerDatosLogin";
            return Ejecutar(sp, conexion =>
            {
                using (MySqlCommand comando = CrearComando(conexion, sp))
                {
                    AgregarEntrada(comando, "p_NumeroTarjeta", MySqlDbType.VarChar, numeroTarjeta.Trim());

                    using (MySqlDataReader lector = comando.ExecuteReader())
                    {
                        if (!lector.Read()) return null;

                        var tarjeta = new Tarjeta(
                            idTarjeta:         Convert.ToInt32(lector["IdTarjeta"]),
                            idCuenta:          Convert.ToInt32(lector["IdCuenta"]),
                            numero:            Convert.ToString(lector["NumeroTarjeta"]),
                            fechaVencimiento:  Convert.ToDateTime(lector["FechaVencimiento"]),
                            intentosFallidos:  Convert.ToInt32(lector["IntentosFallidos"]),
                            estado:            Convert.ToString(lector["Estado"]));

                        byte[] salt = (byte[])lector["Salt"];
                        return new DatosLogin(tarjeta, salt);
                    }
                }
            });
        }
    }
}
