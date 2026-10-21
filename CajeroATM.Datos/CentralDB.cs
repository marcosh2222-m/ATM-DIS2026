using System;
using CajeroATM.Datos.Resultados;
using CajeroATM.Entidades;
using MySqlConnector;

namespace CajeroATM.Datos
{
    /// <summary>
    /// Actor RED CENTRAL: datos de cuenta y autorización de operaciones.
    /// Envuelve USP_ObtenerDatosCuenta, USP_ConsultarSaldo, USP_Extraccion
    /// y USP_Transferencia.
    /// </summary>
    public class CentralDB : AccesoDatos
    {
        public CentralDB() : base(null) { }

        public CentralDB(string cadenaConexion) : base(cadenaConexion) { }

        /// <summary>Cuenta + titular, para armar la sesión tras el login. null si no existe.</summary>
        public DatosCuenta ObtenerDatosCuenta(int idCuenta)
        {
            const string sp = "USP_ObtenerDatosCuenta";
            return Ejecutar(sp, conexion =>
            {
                using (MySqlCommand comando = CrearComando(conexion, sp))
                {
                    AgregarEntrada(comando, "p_IdCuenta", MySqlDbType.Int32, idCuenta);

                    using (MySqlDataReader lector = comando.ExecuteReader())
                    {
                        if (!lector.Read()) return null;

                        var cliente = new Cliente(
                            Convert.ToInt32(lector["IdCliente"]),
                            Convert.ToString(lector["Nombre"]),
                            Convert.ToString(lector["Apellido"]),
                            Convert.ToString(lector["DNI"]));

                        Cuenta cuenta = LeerCuenta(lector);
                        cliente.AgregarCuenta(cuenta);
                        return new DatosCuenta(cliente, cuenta);
                    }
                }
            });
        }

        /// <summary>Saldo actual. Cada consulta queda registrada como transacción (comprobante).</summary>
        public ResultadoConsulta ConsultarSaldo(int idCuenta, int idTarjeta, int idCajero)
        {
            const string sp = "USP_ConsultarSaldo";
            return Ejecutar(sp, conexion =>
            {
                using (MySqlCommand comando = CrearComando(conexion, sp))
                {
                    AgregarEntrada(comando, "p_IdCuenta", MySqlDbType.Int32, idCuenta);
                    AgregarEntrada(comando, "p_IdTarjeta", MySqlDbType.Int32, idTarjeta);
                    AgregarEntrada(comando, "p_IdCajero", MySqlDbType.Int32, idCajero);

                    try
                    {
                        using (MySqlDataReader lector = comando.ExecuteReader())
                        {
                            if (!lector.Read())
                                return new ResultadoConsulta(CodigoOperacion.CuentaNoExiste, null, null);

                            Cuenta cuenta = LeerCuenta(lector);
                            int idTransaccion = Convert.ToInt32(lector["IdTransaccion"]);
                            return new ResultadoConsulta(CodigoOperacion.Ok, cuenta, idTransaccion);
                        }
                    }
                    catch (MySqlException ex) when (ex.SqlState == "45000")
                    {
                        // El SP hace SIGNAL '45000' cuando la cuenta no existe:
                        // es un rechazo de negocio, no un error técnico.
                        return new ResultadoConsulta(CodigoOperacion.CuentaNoExiste, null, null);
                    }
                }
            });
        }

        /// <summary>Extracción de efectivo. El SP valida monto, estado y saldo con bloqueo de fila.</summary>
        public ResultadoMovimiento Extraer(int idCuenta, int idTarjeta, int idCajero, decimal monto)
        {
            const string sp = "USP_Extraccion";
            return Ejecutar(sp, conexion =>
            {
                using (MySqlCommand comando = CrearComando(conexion, sp))
                {
                    AgregarEntrada(comando, "p_IdCuenta", MySqlDbType.Int32, idCuenta);
                    AgregarEntrada(comando, "p_IdTarjeta", MySqlDbType.Int32, idTarjeta);
                    AgregarEntrada(comando, "p_IdCajero", MySqlDbType.Int32, idCajero);
                    AgregarEntrada(comando, "p_Monto", MySqlDbType.NewDecimal, monto);

                    return EjecutarMovimiento(comando, sp);
                }
            });
        }

        /// <summary>Transferencia a otra cuenta, indicada por NÚMERO (formato 0000-0000-00).</summary>
        public ResultadoMovimiento Transferir(int idCuentaOrigen, string numeroCuentaDestino,
                                              int idTarjeta, int idCajero, decimal monto)
        {
            const string sp = "USP_Transferencia";
            return Ejecutar(sp, conexion =>
            {
                using (MySqlCommand comando = CrearComando(conexion, sp))
                {
                    AgregarEntrada(comando, "p_IdCuentaOrigen", MySqlDbType.Int32, idCuentaOrigen);
                    AgregarEntrada(comando, "p_NumeroCuentaDestino", MySqlDbType.VarChar, numeroCuentaDestino);
                    AgregarEntrada(comando, "p_IdTarjeta", MySqlDbType.Int32, idTarjeta);
                    AgregarEntrada(comando, "p_IdCajero", MySqlDbType.Int32, idCajero);
                    AgregarEntrada(comando, "p_Monto", MySqlDbType.NewDecimal, monto);

                    return EjecutarMovimiento(comando, sp);
                }
            });
        }

        // ---------------------------------------------------------
        // Auxiliares
        // ---------------------------------------------------------

        /// <summary>Agrega los 3 OUT comunes de Extracción y Transferencia, ejecuta y mapea.</summary>
        private static ResultadoMovimiento EjecutarMovimiento(MySqlCommand comando, string nombreSP)
        {
            MySqlParameter resultado = AgregarSalida(comando, "p_Resultado", MySqlDbType.VarChar);
            MySqlParameter saldo = AgregarSalida(comando, "p_SaldoResultante", MySqlDbType.NewDecimal);
            MySqlParameter idTransaccion = AgregarSalida(comando, "p_IdTransaccion", MySqlDbType.Int32);

            comando.ExecuteNonQuery();

            return new ResultadoMovimiento(
                MapeoCodigos.AOperacion(LeerTexto(resultado), nombreSP),
                LeerDecimal(saldo),
                LeerEntero(idTransaccion));
        }

        private static Cuenta LeerCuenta(MySqlDataReader lector)
        {
            return new Cuenta(
                Convert.ToInt32(lector["IdCuenta"]),
                Convert.ToString(lector["NumeroCuenta"]),
                Convert.ToDecimal(lector["Saldo"]),
                Convert.ToString(lector["TipoCuenta"]),
                Convert.ToString(lector["Estado"]));
        }
    }
}
