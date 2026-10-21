using System;
using System.Collections.Generic;
using CajeroATM.Datos;
using CajeroATM.Datos.Resultados;
using CajeroATM.Entidades;
using CajeroATM.Negocio.Operaciones;

namespace CajeroATM.Negocio
{
    /// <summary>
    /// Sesión de un cliente en el cajero (almacén D1 del DFD: vive solo
    /// en memoria y se descarta al finalizar).
    /// Diagrama de Clases: Tarjeta 1 ── inicia ── 0..1 Sesion;
    ///                     Sesion 1 ◆── registra ── 0..* Operacion (composición).
    ///
    /// Uso típico desde la Presentación:
    ///   var sesion = new Sesion();
    ///   if (sesion.IniciarSesion(numeroTarjeta))   // Sucursal valida la tarjeta
    ///       sesion.Autenticar(pin);                 // Sucursal valida el PIN
    ///   ... new Extraccion(sesion, monto).Ejecutar() ...
    ///   sesion.Cerrar();
    /// Después de cada paso, UltimoResultado indica el motivo de un rechazo.
    /// </summary>
    public class Sesion
    {
        // Estados de sesión (Diccionario de Datos)
        public const string INICIADA = "Iniciada";          // tarjeta insertada
        public const string VALIDANDO = "Validando";        // Sucursal valida la tarjeta
        public const string AUTENTICANDO = "Autenticando";  // se pide / valida el PIN
        public const string OPERANDO = "Operando";          // menú y operaciones habilitados
        public const string CERRANDO = "Cerrando";          // ticket y devolución de tarjeta
        public const string CERRADA = "Cerrada";

        private readonly SucursalDB _sucursal;
        private readonly CentralDB _central;
        private readonly int _idCajero;

        private string _estado;
        private readonly DateTime _fechaInicio;
        private readonly List<Operacion> _operaciones = new List<Operacion>();
        private Tarjeta _tarjeta;
        private byte[] _salt;
        private Cuenta _cuenta;
        private Cliente _cliente;
        private ResultadoAutenticacion? _ultimoResultado;

        /// <summary>Sesión real: usa la base de App.config y el IdCajero configurado.</summary>
        public Sesion() : this(new SucursalDB(), new CentralDB(), ConfiguracionCajero.IdCajero) { }

        /// <summary>Permite indicar el acceso a datos y el cajero (pruebas, otra base).</summary>
        public Sesion(SucursalDB sucursal, CentralDB central, int idCajero)
        {
            _sucursal = sucursal ?? throw new ArgumentNullException(nameof(sucursal));
            _central = central ?? throw new ArgumentNullException(nameof(central));
            _idCajero = idCajero;
            _estado = INICIADA;
            _fechaInicio = DateTime.Now;
        }

        public string Estado => _estado;
        public DateTime FechaInicio => _fechaInicio;
        public Tarjeta Tarjeta => _tarjeta;
        public Cuenta Cuenta => _cuenta;
        public Cliente Cliente => _cliente;
        public IReadOnlyList<Operacion> Operaciones => _operaciones;

        /// <summary>Motivo del último IniciarSesion / Autenticar (null si todavía no se llamó).</summary>
        public ResultadoAutenticacion? UltimoResultado => _ultimoResultado;

        /// <summary>Intentos de PIN que quedan antes del bloqueo.</summary>
        public int IntentosRestantes =>
            _tarjeta == null ? 0 : Math.Max(0, Tarjeta.MAX_INTENTOS - _tarjeta.IntentosFallidos);

        // Lo usan las operaciones para hablar con Red Central.
        internal CentralDB Central => _central;
        internal int IdCajero => _idCajero;

        // =========================================================
        // +IniciarSesion(numero: string): bool
        // Paso 1 (Sucursal): la tarjeta existe, no está bloqueada,
        // anulada ni vencida. Si falla, la sesión se cierra.
        // =========================================================
        public bool IniciarSesion(string numeroTarjeta)
        {
            if (_estado != INICIADA)
                throw new InvalidOperationException("La sesión ya fue iniciada.");

            CambiarEstado(VALIDANDO);

            string numero = (numeroTarjeta ?? "").Replace(" ", "").Trim();
            if (numero.Length == 0)
                return Rechazar(ResultadoAutenticacion.TarjetaInexistente);

            DatosLogin datos = LlamarDatos(() => _sucursal.ObtenerDatosLogin(numero));

            if (datos == null)
                return Rechazar(ResultadoAutenticacion.TarjetaInexistente);

            Tarjeta tarjeta = datos.Tarjeta;
            if (tarjeta.EstaBloqueada())
                return Rechazar(ResultadoAutenticacion.TarjetaBloqueada);
            if (tarjeta.Estado == Tarjeta.ESTADO_ANULADA)
                return Rechazar(ResultadoAutenticacion.TarjetaAnulada);
            if (tarjeta.EstaVencida())
                return Rechazar(ResultadoAutenticacion.TarjetaVencida);

            _tarjeta = tarjeta;
            _salt = datos.Salt;
            _ultimoResultado = ResultadoAutenticacion.Ok;
            CambiarEstado(AUTENTICANDO);
            return true;
        }

        // =========================================================
        // +Autenticar(pin: string): bool
        // Paso 2 (Sucursal): hash PBKDF2 del PIN y validación en la base.
        // PIN incorrecto: la sesión sigue en AUTENTICANDO para reintentar.
        // Bloqueo / vencimiento: la sesión se cierra.
        // =========================================================
        public bool Autenticar(string pin)
        {
            if (_estado != AUTENTICANDO)
                throw new InvalidOperationException("Primero hay que validar la tarjeta (IniciarSesion).");

            if (!Validaciones.EsPinValido(pin))
            {
                _ultimoResultado = ResultadoAutenticacion.PinInvalido;   // no cuenta como intento
                return false;
            }

            byte[] hash = SeguridadPin.CalcularHash(pin, _salt);
            ResultadoLogin resultado;
            try
            {
                resultado = LlamarDatos(() => _sucursal.ValidarLogin(_tarjeta.IdTarjeta, hash));
            }
            finally
            {
                Array.Clear(hash, 0, hash.Length);   // no dejar el hash en memoria
            }

            switch (resultado.Codigo)
            {
                case CodigoLogin.Ok:
                    DatosCuenta datosCuenta = LlamarDatos(() => _central.ObtenerDatosCuenta(resultado.IdCuenta.Value));
                    if (datosCuenta == null)
                        throw new CajeroException("La tarjeta no tiene una cuenta asociada válida.", null);

                    _tarjeta.ReiniciarIntentos();
                    _cuenta = datosCuenta.Cuenta;
                    _cliente = datosCuenta.Cliente;
                    _salt = null;
                    _ultimoResultado = ResultadoAutenticacion.Ok;
                    CambiarEstado(OPERANDO);
                    return true;

                case CodigoLogin.PinIncorrecto:
                    _tarjeta.RegistrarIntentoFallido();
                    _ultimoResultado = ResultadoAutenticacion.PinIncorrecto;
                    return false;

                case CodigoLogin.Bloqueada:
                    _tarjeta.Bloquear();
                    return Rechazar(ResultadoAutenticacion.TarjetaBloqueada);

                case CodigoLogin.Vencida:
                    return Rechazar(ResultadoAutenticacion.TarjetaVencida);

                case CodigoLogin.Anulada:
                    return Rechazar(ResultadoAutenticacion.TarjetaAnulada);

                default:   // NoExiste: la tarjeta se borró entre un paso y otro
                    return Rechazar(ResultadoAutenticacion.TarjetaInexistente);
            }
        }

        // +AgregarOperacion(op: Operacion): void
        public void AgregarOperacion(Operacion operacion)
        {
            if (operacion == null) throw new ArgumentNullException(nameof(operacion));
            if (_estado != OPERANDO)
                throw new InvalidOperationException("Solo se pueden registrar operaciones con la sesión autenticada.");

            _operaciones.Add(operacion);
        }

        /// <summary>Líneas del ticket (antes de Cerrar, que las borra).</summary>
        public List<string> ObtenerDetalleOperaciones()
        {
            var lineas = new List<string>();
            foreach (Operacion operacion in _operaciones)
                lineas.Add(operacion.ObtenerDetalle());
            return lineas;
        }

        // +Cerrar(): void
        // Elimina los datos de autenticación de la memoria (CU Emitir Ticket, paso 5).
        public void Cerrar()
        {
            if (_estado == CERRADA) return;

            if (_estado != CERRANDO) CambiarEstado(CERRANDO);
            _operaciones.Clear();
            _tarjeta = null;
            _salt = null;
            _cuenta = null;
            _cliente = null;
            CambiarEstado(CERRADA);
        }

        // =========================================================
        // Auxiliares
        // =========================================================

        /// <summary>
        /// Cambia de estado validando que la transición sea válida.
        /// Desde cualquier estado abierto se puede pasar a CERRANDO (cancelar).
        /// </summary>
        internal void CambiarEstado(string nuevoEstado)
        {
            if (!EsTransicionValida(_estado, nuevoEstado))
                throw new InvalidOperationException("Transición de sesión inválida: " + _estado + " → " + nuevoEstado + ".");
            _estado = nuevoEstado;
        }

        internal static bool EsTransicionValida(string desde, string hacia)
        {
            if (desde == CERRADA) return false;
            if (hacia == CERRANDO) return desde != CERRANDO;

            switch (desde)
            {
                case INICIADA:     return hacia == VALIDANDO;
                case VALIDANDO:    return hacia == AUTENTICANDO;
                case AUTENTICANDO: return hacia == OPERANDO;
                case CERRANDO:     return hacia == CERRADA;
                default:           return false;   // OPERANDO solo sale a CERRANDO
            }
        }

        private bool Rechazar(ResultadoAutenticacion motivo)
        {
            _ultimoResultado = motivo;
            Cerrar();
            return false;
        }

        /// <summary>Traduce los errores técnicos de Datos a CajeroException.</summary>
        private static T LlamarDatos<T>(Func<T> llamada)
        {
            try
            {
                return llamada();
            }
            catch (DatosException ex)
            {
                throw new CajeroException(ex.Message, ex);
            }
        }
    }
}
