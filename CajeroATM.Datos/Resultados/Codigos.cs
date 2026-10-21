namespace CajeroATM.Datos.Resultados
{
    /// <summary>Valores de p_Resultado de USP_ValidarLogin.</summary>
    public enum CodigoLogin
    {
        Ok,
        PinIncorrecto,
        Bloqueada,
        Vencida,
        Anulada,
        NoExiste,
    }

    /// <summary>Valores de p_Resultado de USP_Extraccion / USP_Transferencia (y USP_ConsultarSaldo).</summary>
    public enum CodigoOperacion
    {
        Ok,
        MontoInvalido,
        CuentaNoExiste,
        CuentaBloqueada,
        SaldoInsuficiente,
        CuentaDestinoInvalida,
        MismaCuenta,
    }

    /// <summary>
    /// Traducción entre el texto que devuelven los SP y los enums.
    /// Un código desconocido es un error de programación (SP y C# desalineados).
    /// </summary>
    public static class MapeoCodigos
    {
        public static CodigoLogin ALogin(string codigoSP)
        {
            switch (codigoSP)
            {
                case "OK":             return CodigoLogin.Ok;
                case "PIN_INCORRECTO": return CodigoLogin.PinIncorrecto;
                case "BLOQUEADA":      return CodigoLogin.Bloqueada;
                case "VENCIDA":        return CodigoLogin.Vencida;
                case "ANULADA":        return CodigoLogin.Anulada;
                case "NO_EXISTE":      return CodigoLogin.NoExiste;
                default: throw new DatosException("USP_ValidarLogin devolvió un código desconocido: '" + codigoSP + "'.", null);
            }
        }

        public static CodigoOperacion AOperacion(string codigoSP, string nombreSP)
        {
            switch (codigoSP)
            {
                case "OK":                      return CodigoOperacion.Ok;
                case "MONTO_INVALIDO":          return CodigoOperacion.MontoInvalido;
                case "CUENTA_NO_EXISTE":        return CodigoOperacion.CuentaNoExiste;
                case "CUENTA_BLOQUEADA":        return CodigoOperacion.CuentaBloqueada;
                case "SALDO_INSUFICIENTE":      return CodigoOperacion.SaldoInsuficiente;
                case "CUENTA_DESTINO_INVALIDA": return CodigoOperacion.CuentaDestinoInvalida;
                case "MISMA_CUENTA":            return CodigoOperacion.MismaCuenta;
                default: throw new DatosException(nombreSP + " devolvió un código desconocido: '" + codigoSP + "'.", null);
            }
        }

        /// <summary>Vuelta al texto del SP (lo usan los mensajes de pantalla y el log).</summary>
        public static string ACodigoSP(CodigoOperacion codigo)
        {
            switch (codigo)
            {
                case CodigoOperacion.Ok:                    return "OK";
                case CodigoOperacion.MontoInvalido:         return "MONTO_INVALIDO";
                case CodigoOperacion.CuentaNoExiste:        return "CUENTA_NO_EXISTE";
                case CodigoOperacion.CuentaBloqueada:       return "CUENTA_BLOQUEADA";
                case CodigoOperacion.SaldoInsuficiente:     return "SALDO_INSUFICIENTE";
                case CodigoOperacion.CuentaDestinoInvalida: return "CUENTA_DESTINO_INVALIDA";
                default:                                    return "MISMA_CUENTA";
            }
        }
    }
}
