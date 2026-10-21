using System;
using System.Text;
using System.Windows.Forms;
using CajeroATM.Negocio;
using CajeroATM.Presentacion.Theme;

namespace CajeroATM.Presentacion.Forms
{
    public partial class frmLogin : Form
    {
        private const int PIN_LONGITUD = 4;
        private readonly StringBuilder _pinIngresado = new StringBuilder();
        private Sesion _sesion;

        public frmLogin()
        {
            InitializeComponent();
            timerReloj.Start();
            ActualizarFechaHora();
        }

        // =========================================================
        // Paso 1: insertar tarjeta (Sucursal valida la tarjeta)
        // =========================================================
        private void btnInsertar_Click(object sender, EventArgs e)
        {
            string numeroTarjeta = cmbNumeroTarjeta.Text.Trim();

            if (string.IsNullOrWhiteSpace(numeroTarjeta))
            {
                MessageBox.Show(this, "Ingrese un número de tarjeta.", "Cajero Automático",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _sesion = new Sesion();

                Cursor = Cursors.WaitCursor;
                bool tarjetaValida = _sesion.IniciarSesion(numeroTarjeta);
                Cursor = Cursors.Default;

                if (!tarjetaValida)
                {
                    MostrarRechazoTarjeta();
                    MostrarPantallaInsertarTarjeta();
                    return;
                }

                MostrarPantallaPin();
            }
            catch (CajeroException ex)
            {
                Cursor = Cursors.Default;
                MostrarErrorSistema(ex.Message);
                MostrarPantallaInsertarTarjeta();
            }
        }

        private void MostrarPantallaPin()
        {
            _pinIngresado.Clear();
            ActualizarDisplayPin();
            lblMensajePin.Text = "";

            pnlInsertarTarjeta.Visible = false;
            pnlIngresarPin.Visible = true;
        }

        private void MostrarPantallaInsertarTarjeta()
        {
            // Si había una sesión a medio iniciar, se descarta (devolución de tarjeta).
            if (_sesion != null && _sesion.Estado != Sesion.CERRADA)
                _sesion.Cerrar();
            _sesion = null;

            _pinIngresado.Clear();
            cmbNumeroTarjeta.Text = "";
            lblMensajePin.Text = "";

            pnlIngresarPin.Visible = false;
            pnlInsertarTarjeta.Visible = true;
        }

        // =========================================================
        // Paso 2: teclado numérico de PIN
        // =========================================================
        private void btnDigito_Click(object sender, EventArgs e)
        {
            if (_pinIngresado.Length >= PIN_LONGITUD) return;

            var boton = (Button)sender;
            _pinIngresado.Append(boton.Tag.ToString());
            ActualizarDisplayPin();
        }

        private void btnBorrar_Click(object sender, EventArgs e)
        {
            if (_pinIngresado.Length == 0) return;
            _pinIngresado.Length -= 1;
            ActualizarDisplayPin();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            MostrarPantallaInsertarTarjeta();
        }

        private void ActualizarDisplayPin()
        {
            // ● = dígito cargado, ○ = posición vacía. Nunca se muestra el número real.
            var display = new StringBuilder();
            for (int i = 0; i < PIN_LONGITUD; i++)
            {
                display.Append(i < _pinIngresado.Length ? "●" : "○");
                if (i < PIN_LONGITUD - 1) display.Append(' ');
            }
            lblPinDisplay.Text = display.ToString();
        }

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            if (!pnlIngresarPin.Visible || _sesion == null) return;

            if (_pinIngresado.Length != PIN_LONGITUD)
            {
                lblMensajePin.ForeColor = ColoresATM.Advertencia;
                lblMensajePin.Text = "Ingrese los 4 dígitos del PIN.";
                return;
            }

            ValidarPin();
        }

        private void ValidarPin()
        {
            string pin = _pinIngresado.ToString();
            _pinIngresado.Clear();
            ActualizarDisplayPin();

            bool autenticado;
            try
            {
                Cursor = Cursors.WaitCursor;
                autenticado = _sesion.Autenticar(pin);
            }
            catch (CajeroException ex)
            {
                MostrarErrorSistema(ex.Message);
                MostrarPantallaInsertarTarjeta();
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            if (autenticado)
            {
                AbrirMenuPrincipal();
                return;
            }

            ResultadoAutenticacion motivo = _sesion.UltimoResultado ?? ResultadoAutenticacion.PinIncorrecto;
            if (motivo == ResultadoAutenticacion.PinIncorrecto || motivo == ResultadoAutenticacion.PinInvalido)
            {
                // Puede reintentar: sigue en la pantalla de PIN.
                lblMensajePin.ForeColor = motivo == ResultadoAutenticacion.PinIncorrecto ? ColoresATM.Error : ColoresATM.Advertencia;
                lblMensajePin.Text = MensajesOperacion.TraducirAutenticacion(motivo, _sesion.IntentosRestantes);
                return;
            }

            // Bloqueada / vencida / anulada: la sesión ya se cerró.
            MostrarRechazoTarjeta();
            MostrarPantallaInsertarTarjeta();
        }

        private void AbrirMenuPrincipal()
        {
            Sesion sesion = _sesion;
            using (var menu = new frmMenuPrincipal(sesion))
            {
                Hide();
                menu.ShowDialog();
            }
            // El menú cierra la sesión al salir; acá solo se vuelve a "inserte su tarjeta".
            MostrarPantallaInsertarTarjeta();
            Show();
        }

        private void MostrarRechazoTarjeta()
        {
            ResultadoAutenticacion motivo = _sesion?.UltimoResultado ?? ResultadoAutenticacion.TarjetaInexistente;
            MessageBox.Show(this, MensajesOperacion.TraducirAutenticacion(motivo, 0), "Cajero Automático",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void MostrarErrorSistema(string detalle)
        {
            MessageBox.Show(this,
                "El cajero no está disponible en este momento.\n\n" + detalle,
                "Cajero Automático - Fuera de servicio", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // =========================================================
        // Footer: reloj
        // =========================================================
        private void timerReloj_Tick(object sender, EventArgs e)
        {
            ActualizarFechaHora();
        }

        private void ActualizarFechaHora()
        {
            lblFechaHora.Text = DateTime.Now.ToString("dddd dd/MM/yyyy   HH:mm:ss");
        }
    }
}
