using System;

namespace CajeroATM.Entidades
{
    /// <summary>
    /// Tarjeta vinculada a una cuenta (tabla Tarjeta).
    /// Diagrama de Clases: Cuenta 1 ── posee ── 1..* Tarjeta.
    /// El PIN NO se guarda en este objeto: se valida contra el hash de la
    /// base (PBKDF2 + salt) y nunca queda en memoria en texto plano.
    /// </summary>
    public class Tarjeta
    {
        public const string ESTADO_ACTIVA = "Activa";
        public const string ESTADO_BLOQUEADA = "Bloqueada";
        public const string ESTADO_VENCIDA = "Vencida";
        public const string ESTADO_ANULADA = "Anulada";

        /// <summary>Intentos fallidos de PIN antes del bloqueo (CHECK de la tabla: 0 a 3).</summary>
        public const int MAX_INTENTOS = 3;

        private readonly int _idTarjeta;
        private readonly int _idCuenta;
        private readonly string _numero;
        private readonly DateTime _fechaVencimiento;
        private int _intentosFallidos;
        private string _estado;

        public Tarjeta(int idTarjeta, int idCuenta, string numero, DateTime fechaVencimiento,
                       int intentosFallidos, string estado)
        {
            if (string.IsNullOrWhiteSpace(numero)) throw new ArgumentException("El número de tarjeta es obligatorio.", nameof(numero));
            if (intentosFallidos < 0 || intentosFallidos > MAX_INTENTOS)
                throw new ArgumentOutOfRangeException(nameof(intentosFallidos));

            _idTarjeta = idTarjeta;
            _idCuenta = idCuenta;
            _numero = numero;
            _fechaVencimiento = fechaVencimiento;
            _intentosFallidos = intentosFallidos;
            _estado = estado;
        }

        public int IdTarjeta => _idTarjeta;
        public int IdCuenta => _idCuenta;
        public string Numero => _numero;
        public DateTime FechaVencimiento => _fechaVencimiento;
        public int IntentosFallidos => _intentosFallidos;
        public string Estado => _estado;

        // +EstaVencida(): bool
        // Vence al terminar el día de la fecha de vencimiento.
        public bool EstaVencida()
        {
            return _estado == ESTADO_VENCIDA || _fechaVencimiento.Date < DateTime.Today;
        }

        // +EstaBloqueada(): bool
        public bool EstaBloqueada()
        {
            return _estado == ESTADO_BLOQUEADA;
        }

        // +RegistrarIntentoFallido(): void
        // Al tercer intento la tarjeta queda bloqueada (misma regla que USP_ValidarLogin).
        public void RegistrarIntentoFallido()
        {
            if (EstaBloqueada()) return;

            _intentosFallidos++;
            if (_intentosFallidos >= MAX_INTENTOS)
                _estado = ESTADO_BLOQUEADA;
        }

        /// <summary>Refleja un bloqueo que ya aplicó la base (USP_ValidarLogin devolvió BLOQUEADA).</summary>
        public void Bloquear()
        {
            _estado = ESTADO_BLOQUEADA;
            if (_intentosFallidos < MAX_INTENTOS) _intentosFallidos = MAX_INTENTOS;
        }

        /// <summary>Tras un login correcto el contador vuelve a 0.</summary>
        public void ReiniciarIntentos()
        {
            _intentosFallidos = 0;
        }

        /// <summary>"4000111122223333" → "**** **** **** 3333"</summary>
        public string ObtenerNumeroEnmascarado()
        {
            string ultimos = _numero.Length >= 4 ? _numero.Substring(_numero.Length - 4) : _numero;
            return "**** **** **** " + ultimos;
        }
    }
}
