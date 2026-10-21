using CajeroATM.Datos.Resultados;

namespace CajeroATM.Negocio.Operaciones
{
    public class Consulta : Operacion
    {
        private decimal _saldoConsultado;

        public Consulta(Sesion sesion) : base(sesion, 0m)
        {
            _saldoConsultado = 0m;
        }

        public override string TipoTransaccion => "Consulta de Saldo";

        public decimal SaldoConsultado => _saldoConsultado;

        // +Ejecutar(): bool
        public override bool Ejecutar()
        {
            return EjecutarConControl(() =>
            {
                ResultadoConsulta respuesta = Central.ConsultarSaldo(cuenta.IdCuenta, IdTarjeta, IdCajero);
                if (!respuesta.EsOk)
                    return RegistrarRespuesta(respuesta.Codigo, null, respuesta.IdTransaccion);

                _saldoConsultado = respuesta.Cuenta.ConsultarSaldo();
                return RegistrarRespuesta(respuesta.Codigo, _saldoConsultado, respuesta.IdTransaccion);
            });
        }
    }
}
