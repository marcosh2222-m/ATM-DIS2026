namespace CajeroATM.Negocio
{
    /// <summary>Resultado de Sesion.IniciarSesion y Sesion.Autenticar (ver Sesion.UltimoResultado).</summary>
    public enum ResultadoAutenticacion
    {
        Ok,
        TarjetaInexistente,
        TarjetaBloqueada,
        TarjetaVencida,
        TarjetaAnulada,
        PinInvalido,        // formato incorrecto: no cuenta como intento
        PinIncorrecto,
    }
}
