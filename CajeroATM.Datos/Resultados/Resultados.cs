using CajeroATM.Entidades;

namespace CajeroATM.Datos.Resultados
{
    /// <summary>Lo que devuelve USP_ObtenerDatosLogin. El Salt no se guarda en la entidad Tarjeta.</summary>
    public class DatosLogin
    {
        public DatosLogin(Tarjeta tarjeta, byte[] salt)
        {
            Tarjeta = tarjeta;
            Salt = salt;
        }

        public Tarjeta Tarjeta { get; }
        public byte[] Salt { get; }
    }

    /// <summary>OUT de USP_ValidarLogin.</summary>
    public class ResultadoLogin
    {
        public ResultadoLogin(CodigoLogin codigo, int? idCuenta)
        {
            Codigo = codigo;
            IdCuenta = idCuenta;
        }

        public CodigoLogin Codigo { get; }
        public int? IdCuenta { get; }            // solo viene con Codigo = Ok
        public bool EsOk => Codigo == CodigoLogin.Ok;
    }

    /// <summary>Resultado de USP_ObtenerDatosCuenta: la cuenta y su titular.</summary>
    public class DatosCuenta
    {
        public DatosCuenta(Cliente cliente, Cuenta cuenta)
        {
            Cliente = cliente;
            Cuenta = cuenta;
        }

        public Cliente Cliente { get; }
        public Cuenta Cuenta { get; }
    }

    /// <summary>Resultado de USP_ConsultarSaldo.</summary>
    public class ResultadoConsulta
    {
        public ResultadoConsulta(CodigoOperacion codigo, Cuenta cuenta, int? idTransaccion)
        {
            Codigo = codigo;
            Cuenta = cuenta;
            IdTransaccion = idTransaccion;
        }

        public CodigoOperacion Codigo { get; }
        public Cuenta Cuenta { get; }             // null si la cuenta no existe
        public int? IdTransaccion { get; }        // número de comprobante
        public bool EsOk => Codigo == CodigoOperacion.Ok;
    }

    /// <summary>OUT de USP_Extraccion y USP_Transferencia.</summary>
    public class ResultadoMovimiento
    {
        public ResultadoMovimiento(CodigoOperacion codigo, decimal? saldoResultante, int? idTransaccion)
        {
            Codigo = codigo;
            SaldoResultante = saldoResultante;
            IdTransaccion = idTransaccion;
        }

        public CodigoOperacion Codigo { get; }

        /// <summary>Saldo de la cuenta origen después de la operación (o el actual, si se rechazó).
        /// Es null cuando el SP rechaza antes de leer la cuenta (MONTO_INVALIDO, MISMA_CUENTA...).</summary>
        public decimal? SaldoResultante { get; }

        /// <summary>Número de comprobante. Los rechazos también quedan registrados,
        /// salvo los que se cortan antes de tocar la base (monto inválido, cuenta inexistente).</summary>
        public int? IdTransaccion { get; }

        public bool EsOk => Codigo == CodigoOperacion.Ok;
    }
}
