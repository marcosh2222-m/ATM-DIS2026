using System.Collections.Generic;

namespace CajeroATM.Presentacion.Simulacion
{
    /// <summary>
    /// ⚠️ TEMPORAL. Simula el resultado de USP_ObtenerDatosLogin +
    /// USP_ValidarLogin mientras no está lista la capa de Negocio/Datos.
    ///
    /// Cuando conectemos CajeroATM.Negocio de verdad, esta clase se
    /// elimina y frmLogin pasa a llamar directamente a algo como
    /// UsuarioBL.ValidarLogin(numeroTarjeta, pin). La firma del método
    /// de abajo está pensada para que ese reemplazo sea casi directo
    /// (mismo nombre de resultado, misma forma de datos).
    ///
    /// Tarjeta de prueba: 4000111122223333 / PIN 1234
    /// (son los mismos datos del seed en el script de base de datos)
    /// </summary>
    internal static class ServicioLoginSimulado
    {
        private class TarjetaSimulada
        {
            public string Pin;
            public int IntentosFallidos;
            public bool Bloqueada;
            public int IdCuenta;
            public string NombreCliente;
            public string NumeroCuentaEnmascarado;
        }

        private static readonly Dictionary<string, TarjetaSimulada> _tarjetas = new Dictionary<string, TarjetaSimulada>
        {
            ["4000111122223333"] = new TarjetaSimulada
            {
                Pin = "1234",
                IntentosFallidos = 0,
                Bloqueada = false,
                IdCuenta = 1,
                NombreCliente = "Marcos Pérez",
                NumeroCuentaEnmascarado = "**** **** 01",
            }
        };

        public struct ResultadoLogin
        {
            public string Resultado;              // OK | PIN_INCORRECTO | BLOQUEADA | NO_EXISTE
            public int IdCuenta;
            public string NombreCliente;
            public string NumeroCuentaEnmascarado;
        }

        public static bool ExisteTarjeta(string numeroTarjeta)
        {
            return _tarjetas.ContainsKey(numeroTarjeta);
        }

        public static ResultadoLogin ValidarLogin(string numeroTarjeta, string pin)
        {
            if (!_tarjetas.TryGetValue(numeroTarjeta, out var tarjeta))
            {
                return new ResultadoLogin { Resultado = "NO_EXISTE" };
            }

            if (tarjeta.Bloqueada)
            {
                return new ResultadoLogin { Resultado = "BLOQUEADA" };
            }

            if (tarjeta.Pin == pin)
            {
                tarjeta.IntentosFallidos = 0;
                return new ResultadoLogin
                {
                    Resultado = "OK",
                    IdCuenta = tarjeta.IdCuenta,
                    NombreCliente = tarjeta.NombreCliente,
                    NumeroCuentaEnmascarado = tarjeta.NumeroCuentaEnmascarado,
                };
            }

            tarjeta.IntentosFallidos++;
            if (tarjeta.IntentosFallidos >= 3)
            {
                tarjeta.Bloqueada = true;
                return new ResultadoLogin { Resultado = "BLOQUEADA" };
            }

            return new ResultadoLogin { Resultado = "PIN_INCORRECTO" };
        }
    }
}
