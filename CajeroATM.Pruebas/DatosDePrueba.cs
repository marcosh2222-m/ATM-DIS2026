using System;
using CajeroATM.Datos;
using CajeroATM.Negocio;
using MySqlConnector;

namespace CajeroATM.Pruebas
{
    /// <summary>
    /// Crea y borra los datos propios de las pruebas, sin tocar los del seed.
    /// (Es el único lugar del proyecto con SQL directo: la aplicación usa
    /// exclusivamente stored procedures.)
    /// </summary>
    internal static class DatosDePrueba
    {
        public const string CUENTA_A = "9901-0001-01";   // Activa, $ 100.000 (la que opera)
        public const string CUENTA_B = "9901-0002-02";   // Activa, $ 0 (destino)
        public const string CUENTA_C = "9901-0003-03";   // Bloqueada, $ 5.000

        public const string TARJETA_OK = "4999000000000001";       // cuenta A, PIN 4321
        public const string TARJETA_BLOQUEO = "4999000000000002";  // cuenta A, PIN 1111 (se bloquea en las pruebas)
        public const string TARJETA_VENCIDA = "4999000000000003";  // cuenta A, vencida

        public const string PIN_OK = "4321";
        public const string PIN_BLOQUEO = "1111";

        public const decimal SALDO_INICIAL_A = 100000m;

        public static int IdCuentaA { get; private set; }
        public static int IdCuentaB { get; private set; }

        public static void Crear()
        {
            Borrar();
            Ejecutar(conexion =>
            {
                long idCliente = Insertar(conexion,
                    "INSERT INTO Cliente (Nombre, Apellido, DNI) VALUES ('Prueba', 'Automática', '99999999')");

                IdCuentaA = (int)InsertarCuenta(conexion, idCliente, CUENTA_A, SALDO_INICIAL_A, "Activa");
                IdCuentaB = (int)InsertarCuenta(conexion, idCliente, CUENTA_B, 0m, "Activa");
                InsertarCuenta(conexion, idCliente, CUENTA_C, 5000m, "Bloqueada");

                InsertarTarjeta(conexion, IdCuentaA, TARJETA_OK, PIN_OK, DateTime.Today.AddYears(3));
                InsertarTarjeta(conexion, IdCuentaA, TARJETA_BLOQUEO, PIN_BLOQUEO, DateTime.Today.AddYears(3));
                InsertarTarjeta(conexion, IdCuentaA, TARJETA_VENCIDA, PIN_OK, DateTime.Today.AddDays(-1));
            });
        }

        public static void Borrar()
        {
            Ejecutar(conexion =>
            {
                Comando(conexion, @"DELETE t FROM Transaccion t
                                    LEFT JOIN Tarjeta ta ON ta.IdTarjeta = t.IdTarjeta
                                    LEFT JOIN Cuenta co ON co.IdCuenta = t.IdCuentaOrigen
                                    LEFT JOIN Cuenta cd ON cd.IdCuenta = t.IdCuentaDestino
                                    WHERE ta.NumeroTarjeta LIKE '4999%' OR co.NumeroCuenta LIKE '9901-%' OR cd.NumeroCuenta LIKE '9901-%'").ExecuteNonQuery();
                Comando(conexion, "DELETE FROM Tarjeta WHERE NumeroTarjeta LIKE '4999%'").ExecuteNonQuery();
                Comando(conexion, "DELETE FROM Cuenta WHERE NumeroCuenta LIKE '9901-%'").ExecuteNonQuery();
                Comando(conexion, "DELETE FROM Cliente WHERE DNI = '99999999'").ExecuteNonQuery();
            });
        }

        /// <summary>Saldo directo de la tabla (para comparar con lo que informa la aplicación).</summary>
        public static decimal SaldoEnBase(string numeroCuenta)
        {
            decimal saldo = 0;
            Ejecutar(conexion =>
            {
                MySqlCommand c = Comando(conexion, "SELECT Saldo FROM Cuenta WHERE NumeroCuenta = @n");
                c.Parameters.AddWithValue("@n", numeroCuenta);
                saldo = Convert.ToDecimal(c.ExecuteScalar());
            });
            return saldo;
        }

        /// <summary>SaldoResultante del último movimiento registrado con la cuenta como origen.</summary>
        public static decimal SaldoDelUltimoMovimiento(string numeroCuenta)
        {
            decimal saldo = 0;
            Ejecutar(conexion =>
            {
                MySqlCommand c = Comando(conexion, @"SELECT t.SaldoResultante FROM Transaccion t
                    INNER JOIN Cuenta c ON c.IdCuenta = t.IdCuentaOrigen
                    WHERE c.NumeroCuenta = @n ORDER BY t.IdTransaccion DESC LIMIT 1");
                c.Parameters.AddWithValue("@n", numeroCuenta);
                saldo = Convert.ToDecimal(c.ExecuteScalar());
            });
            return saldo;
        }

        // ---------------------------------------------------------
        private static long InsertarCuenta(MySqlConnection conexion, long idCliente, string numero, decimal saldo, string estado)
        {
            MySqlCommand c = Comando(conexion,
                "INSERT INTO Cuenta (IdCliente, NumeroCuenta, Saldo, TipoCuenta, Estado) VALUES (@cl, @n, @s, 'Caja de Ahorro', @e)");
            c.Parameters.AddWithValue("@cl", idCliente);
            c.Parameters.AddWithValue("@n", numero);
            c.Parameters.AddWithValue("@s", saldo);
            c.Parameters.AddWithValue("@e", estado);
            c.ExecuteNonQuery();
            return c.LastInsertedId;
        }

        private static void InsertarTarjeta(MySqlConnection conexion, int idCuenta, string numero, string pin, DateTime vencimiento)
        {
            byte[] salt = SeguridadPin.GenerarSalt();   // igual que un alta real: salt aleatorio
            MySqlCommand c = Comando(conexion,
                "INSERT INTO Tarjeta (IdCuenta, NumeroTarjeta, PIN_Hash, Salt, FechaVencimiento, Estado) VALUES (@c, @n, @h, @s, @v, 'Activa')");
            c.Parameters.AddWithValue("@c", idCuenta);
            c.Parameters.AddWithValue("@n", numero);
            c.Parameters.AddWithValue("@h", SeguridadPin.CalcularHash(pin, salt));
            c.Parameters.AddWithValue("@s", salt);
            c.Parameters.AddWithValue("@v", vencimiento);
            c.ExecuteNonQuery();
        }

        private static long Insertar(MySqlConnection conexion, string sql)
        {
            MySqlCommand c = Comando(conexion, sql);
            c.ExecuteNonQuery();
            return c.LastInsertedId;
        }

        private static MySqlCommand Comando(MySqlConnection conexion, string sql)
        {
            return new MySqlCommand(sql, conexion);
        }

        private static void Ejecutar(Action<MySqlConnection> accion)
        {
            using (MySqlConnection conexion = Conexion.CrearConexion())
            {
                conexion.Open();
                accion(conexion);
            }
        }
    }
}
