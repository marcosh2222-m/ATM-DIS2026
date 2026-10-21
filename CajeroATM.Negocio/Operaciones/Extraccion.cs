using CajeroATM.Datos.Resultados;

namespace CajeroATM.Negocio.Operaciones
{
    public class Extraccion : Operacion
    {
        /// <summary>Denominación de los billetes del cajero (DD: monto múltiplo de 100).</summary>
        public const decimal DENOMINACION = 100m;

        private int _billetesEntregados;

        public Extraccion(Sesion sesion, decimal monto) : base(sesion, monto)
        {
            _billetesEntregados = 0;
        }

        public override string TipoTransaccion => "Extraccion";

        public int BilletesEntregados => _billetesEntregados;

        // +Ejecutar(): bool   (Diagrama de Secuencia "Extraer Efectivo")
        public override bool Ejecutar()
        {
            return EjecutarConControl(() =>
            {
                if (!EsMultiploDe100()) return Rechazar("MONTO_INVALIDO");
                if (!cuenta.EstaActiva()) return Rechazar("CUENTA_BLOQUEADA");

                // Red Central valida de nuevo y descuenta con bloqueo de fila.
                ResultadoMovimiento respuesta = Central.Extraer(cuenta.IdCuenta, IdTarjeta, IdCajero, monto);
                bool ok = RegistrarRespuesta(respuesta.Codigo, respuesta.SaldoResultante, respuesta.IdTransaccion);

                if (ok) _billetesEntregados = (int)(monto / DENOMINACION);
                return ok;
            });
        }

        // -EsMultiploDe100(): bool
        private bool EsMultiploDe100()
        {
            return monto > 0 && monto % DENOMINACION == 0;
        }
    }
}
