using System;

namespace CajeroATM.Entidades
{
    /// <summary>
    /// Cuenta bancaria (tabla Cuenta). La fuente de verdad del saldo es
    /// la base de datos (Red Central); este objeto es la copia en memoria
    /// que usa el cajero durante la sesión (almacén D2 del DFD).
    /// </summary>
    public class Cuenta
    {
        // Valores permitidos (Diccionario de Datos)
        public const string ESTADO_ACTIVA = "Activa";
        public const string ESTADO_BLOQUEADA = "Bloqueada";
        public const string ESTADO_CERRADA = "Cerrada";

        public const string TIPO_CAJA_AHORRO = "Caja de Ahorro";
        public const string TIPO_CUENTA_CORRIENTE = "Cuenta Corriente";

        private readonly int _idCuenta;
        private readonly string _numeroCuenta;
        private decimal _saldo;
        private readonly string _tipoCuenta;
        private string _estado;

        public Cuenta(int idCuenta, string numeroCuenta, decimal saldo, string tipoCuenta, string estado)
        {
            if (string.IsNullOrWhiteSpace(numeroCuenta)) throw new ArgumentException("El número de cuenta es obligatorio.", nameof(numeroCuenta));
            if (saldo < 0) throw new ArgumentOutOfRangeException(nameof(saldo), "El saldo no puede ser negativo.");

            _idCuenta = idCuenta;
            _numeroCuenta = numeroCuenta;
            _saldo = saldo;
            _tipoCuenta = tipoCuenta;
            _estado = estado;
        }

        public int IdCuenta => _idCuenta;
        public string NumeroCuenta => _numeroCuenta;
        public string TipoCuenta => _tipoCuenta;
        public string Estado => _estado;

        // +ConsultarSaldo(): decimal
        public decimal ConsultarSaldo()
        {
            return _saldo;
        }

        // +Debitar(monto: decimal): bool
        // Devuelve false (sin tocar el saldo) si la cuenta no está activa,
        // el monto no es válido o no alcanza el saldo.
        public bool Debitar(decimal monto)
        {
            if (!EstaActiva() || monto <= 0 || monto > _saldo)
                return false;

            _saldo -= monto;
            return true;
        }

        // +Acreditar(monto: decimal): void
        public void Acreditar(decimal monto)
        {
            if (monto <= 0) throw new ArgumentOutOfRangeException(nameof(monto), "El monto a acreditar debe ser mayor a 0.");
            if (!EstaActiva()) throw new InvalidOperationException("No se puede acreditar en una cuenta que no está activa.");

            _saldo += monto;
        }

        // +EstaActiva(): bool
        public bool EstaActiva()
        {
            return _estado == ESTADO_ACTIVA;
        }

        /// <summary>
        /// Sincroniza el saldo con el valor que devolvió la base de datos
        /// (p_SaldoResultante de los SP). La usa la capa de Negocio.
        /// </summary>
        public void ActualizarSaldo(decimal saldoInformado)
        {
            if (saldoInformado < 0) throw new ArgumentOutOfRangeException(nameof(saldoInformado));
            _saldo = saldoInformado;
        }

        /// <summary>"0001-0001-01" → "**** **** 01" (para mostrar en pantalla).</summary>
        public string ObtenerNumeroEnmascarado()
        {
            string ultimos = _numeroCuenta.Length >= 2 ? _numeroCuenta.Substring(_numeroCuenta.Length - 2) : _numeroCuenta;
            return "**** **** " + ultimos;
        }
    }
}
