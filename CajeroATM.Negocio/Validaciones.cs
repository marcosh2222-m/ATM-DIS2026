using System.Globalization;
using System.Linq;

namespace CajeroATM.Negocio
{
    /// <summary>
    /// Validaciones de entrada (reglas del Diccionario de Datos). Las usa
    /// la Presentación antes de llamar a Negocio, y Negocio las vuelve a
    /// aplicar por seguridad.
    /// </summary>
    public static class Validaciones
    {
        /// <summary>Denominación de billetes del cajero (DD: múltiplo de 100).</summary>
        public const decimal DENOMINACION = 100m;

        /// <summary>
        /// Valida el monto ingresado: número entero, mayor a 0 y múltiplo de $100.
        /// </summary>
        public static bool TryParseMonto(string texto, out decimal monto, out string error)
        {
            monto = 0;
            string limpio = (texto ?? "").Replace("$", "").Replace(".", "").Replace(" ", "").Trim();

            if (limpio.Length == 0)
            {
                error = "Ingrese un monto.";
                return false;
            }

            if (limpio.Length > 9 ||
                !decimal.TryParse(limpio, NumberStyles.None, CultureInfo.InvariantCulture, out monto))
            {
                monto = 0;
                error = "El monto ingresado no es válido.";
                return false;
            }

            if (monto <= 0)
            {
                error = "El monto debe ser mayor a $ 0.";
                return false;
            }

            if (monto % DENOMINACION != 0)
            {
                error = "El monto debe ser múltiplo de $ 100\n(billetes disponibles).";
                return false;
            }

            error = null;
            return true;
        }

        public const int LONGITUD_PIN = 4;

        /// <summary>PIN: exactamente 4 dígitos (DD).</summary>
        public static bool EsPinValido(string pin)
        {
            return pin != null && pin.Length == LONGITUD_PIN && pin.All(char.IsDigit);
        }

        /// <summary>
        /// Normaliza la cuenta destino al formato 0000-0000-00. Acepta que
        /// el usuario la escriba con o sin guiones.
        /// </summary>
        public static bool TryNormalizarCuenta(string texto, out string cuenta, out string error)
        {
            cuenta = null;
            string digitos = new string((texto ?? "").Where(char.IsDigit).ToArray());

            if (digitos.Length != 10)
            {
                error = "La cuenta destino debe tener 10 dígitos\n(formato 0000-0000-00).";
                return false;
            }

            cuenta = digitos.Substring(0, 4) + "-" + digitos.Substring(4, 4) + "-" + digitos.Substring(8, 2);
            error = null;
            return true;
        }
    }

    public static class Formato
    {
        private static readonly CultureInfo EsAr = new CultureInfo("es-AR");

        /// <summary>Ej: 50000 → "$ 50.000,00"</summary>
        public static string Moneda(decimal importe)
        {
            return "$ " + importe.ToString("N2", EsAr);
        }
    }

    /// <summary>
    /// Traduce los códigos de resultado de los stored procedures a un
    /// mensaje claro para el usuario.
    /// </summary>
    public static class MensajesOperacion
    {
        public static string Traducir(string resultado)
        {
            switch (resultado)
            {
                case "OK":                      return "Operación realizada con éxito.";
                case "MONTO_INVALIDO":          return "El monto ingresado no es válido.";
                case "CUENTA_NO_EXISTE":        return "No se encontró la cuenta.";
                case "CUENTA_BLOQUEADA":        return "Su cuenta se encuentra bloqueada.\nComuníquese con su sucursal.";
                case "SALDO_INSUFICIENTE":      return "Saldo insuficiente para realizar la operación.";
                case "CUENTA_DESTINO_INVALIDA": return "La cuenta destino no existe\no no está habilitada.";
                case "MISMA_CUENTA":            return "No puede transferir a su propia cuenta.";
                case "ERROR":                   return "No se pudo comunicar con la Red Central.\nIntente nuevamente en unos instantes.";
                default:                        return "No se pudo completar la operación.\nIntente nuevamente.";
            }
        }

        public static string TraducirAutenticacion(ResultadoAutenticacion resultado, int intentosRestantes)
        {
            switch (resultado)
            {
                case ResultadoAutenticacion.Ok:                 return "Bienvenido/a.";
                case ResultadoAutenticacion.TarjetaInexistente: return "Tarjeta no reconocida.";
                case ResultadoAutenticacion.TarjetaBloqueada:   return "Tarjeta bloqueada por intentos fallidos.\nDiríjase a su sucursal para desbloquearla.";
                case ResultadoAutenticacion.TarjetaVencida:     return "La tarjeta se encuentra vencida.";
                case ResultadoAutenticacion.TarjetaAnulada:     return "La tarjeta fue anulada.\nDiríjase a su sucursal.";
                case ResultadoAutenticacion.PinInvalido:        return "Ingrese los 4 dígitos del PIN.";
                case ResultadoAutenticacion.PinIncorrecto:
                    return intentosRestantes == 1
                        ? "PIN incorrecto. Le queda 1 intento."
                        : "PIN incorrecto. Le quedan " + intentosRestantes + " intentos.";
                default:                                        return "No se pudo validar la tarjeta.";
            }
        }

        public const string ERROR_INESPERADO =
            "Ocurrió un error inesperado.\nIntente nuevamente en unos instantes.";
    }
}
