using CajeroATM.Datos.Resultados;

namespace CajeroATM.Negocio.Operaciones
{
    public class Transferencia : Operacion
    {
        // -numeroCuentaDestino: string
        // (El cajero solo conoce el NÚMERO que tipea el cliente; los datos de
        // la cuenta destino son de otra persona y nunca se cargan en memoria.)
        private readonly string _numeroCuentaDestino;

        /// <param name="numeroCuentaDestino">Con o sin guiones; se normaliza a 0000-0000-00.</param>
        public Transferencia(Sesion sesion, string numeroCuentaDestino, decimal monto)
            : base(sesion, monto)
        {
            _numeroCuentaDestino = Validaciones.TryNormalizarCuenta(numeroCuentaDestino, out string normalizada, out _)
                ? normalizada
                : (numeroCuentaDestino ?? "").Trim();
        }

        public override string TipoTransaccion => "Transferencia";

        public string NumeroCuentaDestino => _numeroCuentaDestino;

        // +Ejecutar(): bool
        public override bool Ejecutar()
        {
            return EjecutarConControl(() =>
            {
                if (monto <= 0) return Rechazar("MONTO_INVALIDO");
                if (!ValidarCuentaDestino())
                    return Rechazar(_numeroCuentaDestino == cuenta.NumeroCuenta ? "MISMA_CUENTA" : "CUENTA_DESTINO_INVALIDA");
                if (!cuenta.EstaActiva()) return Rechazar("CUENTA_BLOQUEADA");

                // Red Central confirma que el destino exista y esté activo,
                // y mueve el dinero en una única transacción.
                ResultadoMovimiento respuesta = Central.Transferir(cuenta.IdCuenta, _numeroCuentaDestino,
                                                                   IdTarjeta, IdCajero, monto);
                return RegistrarRespuesta(respuesta.Codigo, respuesta.SaldoResultante, respuesta.IdTransaccion);
            });
        }

        // -ValidarCuentaDestino(): bool
        // Formato correcto y distinta de la cuenta origen. La existencia y el
        // estado los confirma USP_Transferencia (CUENTA_DESTINO_INVALIDA).
        private bool ValidarCuentaDestino()
        {
            return Validaciones.TryNormalizarCuenta(_numeroCuentaDestino, out string normalizada, out _)
                && normalizada == _numeroCuentaDestino
                && _numeroCuentaDestino != cuenta.NumeroCuenta;
        }
    }
}
