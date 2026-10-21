using System;
using System.Globalization;
using CajeroATM.Datos;
using CajeroATM.Datos.Resultados;
using CajeroATM.Entidades;

namespace CajeroATM.Negocio.Operaciones
{
    /// <summary>
    /// Clase abstracta base de toda operación del cajero.
    /// Diagrama de Clases: Operacion * ── opera sobre ── 1 Cuenta;
    /// Consulta, Extraccion y Transferencia heredan de ella.
    ///
    /// Cada operación se ejecuta una sola vez. Si sale bien, se registra
    /// sola en la sesión (Sesion.AgregarOperacion), como en el Diagrama
    /// de Secuencia. Los rechazos y errores NO lanzan excepciones:
    /// Ejecutar() devuelve false y CodigoResultado / MensajeError dicen por qué.
    /// </summary>
    public abstract class Operacion
    {
        // Resultado de Operación (Diccionario de Datos): [ Exitosa | Rechazada | Error ]
        public const string RESULTADO_EXITOSA = "Exitosa";
        public const string RESULTADO_RECHAZADA = "Rechazada";
        public const string RESULTADO_ERROR = "Error";

        /// <summary>CodigoResultado cuando falla la conexión u otro error técnico.</summary>
        public const string CODIGO_ERROR = "ERROR";

        private static readonly CultureInfo EsAr = new CultureInfo("es-AR");

        // Atributos protegidos ("#" en el diagrama): accesibles desde las hijas.
        protected int idComprobante;
        protected DateTime fechaHora;
        protected decimal monto;
        protected string resultado;
        protected string codigoResultado;    // código del SP: OK, SALDO_INSUFICIENTE, etc.
        protected decimal? saldoResultante;
        protected string mensajeError;
        protected readonly Cuenta cuenta;
        protected readonly Sesion sesion;

        protected Operacion(Sesion sesion, decimal monto)
        {
            if (sesion == null) throw new ArgumentNullException(nameof(sesion));
            if (sesion.Estado != Sesion.OPERANDO)
                throw new InvalidOperationException("No hay una sesión autenticada para operar.");
            if (monto < 0) throw new ArgumentOutOfRangeException(nameof(monto), "El monto no puede ser negativo.");

            this.sesion = sesion;
            this.cuenta = sesion.Cuenta;
            this.monto = monto;
            fechaHora = DateTime.Now;
        }

        /// <summary>Nombre igual al de la tabla TipoTransaccion.</summary>
        public abstract string TipoTransaccion { get; }

        public int IdComprobante => idComprobante;
        public DateTime FechaHora => fechaHora;
        public decimal Monto => monto;
        public string Resultado => resultado;               // Exitosa | Rechazada | Error (null = sin ejecutar)
        public string CodigoResultado => codigoResultado;   // para MensajesOperacion.Traducir
        public decimal? SaldoResultante => saldoResultante; // saldo de la cuenta después de operar
        public string MensajeError => mensajeError;         // detalle técnico si Resultado = Error
        public Cuenta Cuenta => cuenta;
        public bool FueExitosa => resultado == RESULTADO_EXITOSA;

        // +Ejecutar(): bool   (abstracto: cada operación lo resuelve a su manera)
        public abstract bool Ejecutar();

        // +ObtenerDetalle(): string   (una línea del ticket)
        public string ObtenerDetalle()
        {
            return string.Format(EsAr, "{0:dd/MM/yyyy HH:mm}  {1,-17}  $ {2,12:N2}  {3,-9}  Comp. {4}",
                fechaHora, TipoTransaccion, monto, resultado ?? "-", idComprobante);
        }

        // ---------------------------------------------------------
        // Soporte para las clases hijas
        // ---------------------------------------------------------

        /// <summary>
        /// Esqueleto común de Ejecutar(): controla que se ejecute una sola
        /// vez y con la sesión activa, atrapa errores técnicos y, si salió
        /// bien, registra la operación en la sesión.
        /// </summary>
        protected bool EjecutarConControl(Func<bool> accion)
        {
            if (resultado != null)
                throw new InvalidOperationException("La operación ya fue ejecutada.");
            if (sesion.Estado != Sesion.OPERANDO)
                throw new InvalidOperationException("La sesión ya no está activa.");

            bool exito;
            try
            {
                exito = accion();
            }
            catch (DatosException ex)
            {
                RegistrarError(ex.Message);
                return false;
            }

            if (exito) sesion.AgregarOperacion(this);
            return exito;
        }

        /// <summary>Rechazo detectado en Negocio, antes de llegar a la base.</summary>
        protected bool Rechazar(string codigo)
        {
            codigoResultado = codigo;
            resultado = RESULTADO_RECHAZADA;
            fechaHora = DateTime.Now;
            return false;
        }

        /// <summary>Guarda lo que devolvió Red Central y sincroniza el saldo local de la cuenta.</summary>
        protected bool RegistrarRespuesta(CodigoOperacion codigo, decimal? saldo, int? comprobante)
        {
            codigoResultado = MapeoCodigos.ACodigoSP(codigo);
            resultado = codigo == CodigoOperacion.Ok ? RESULTADO_EXITOSA : RESULTADO_RECHAZADA;
            idComprobante = comprobante ?? 0;
            saldoResultante = saldo;
            fechaHora = DateTime.Now;

            // La base es la fuente de verdad: el saldo en memoria se pisa con el informado.
            if (saldo.HasValue) cuenta.ActualizarSaldo(saldo.Value);

            return codigo == CodigoOperacion.Ok;
        }

        protected void RegistrarError(string detalle)
        {
            codigoResultado = CODIGO_ERROR;
            resultado = RESULTADO_ERROR;
            mensajeError = detalle;
            fechaHora = DateTime.Now;
        }

        protected int IdTarjeta => sesion.Tarjeta.IdTarjeta;
        protected int IdCajero => sesion.IdCajero;
        protected CentralDB Central => sesion.Central;
    }
}
