using System;
using System.Text;
using System.Windows.Forms;
using CajeroATM.Presentacion.Simulacion;
using CajeroATM.Presentacion.Theme;

namespace CajeroATM.Presentacion.Forms
{
    public partial class frmLogin : Form
    {
        private const int PIN_LONGITUD = 4;
        private readonly StringBuilder _pinIngresado = new StringBuilder();
        private string _numeroTarjetaActual = "";

        public frmLogin()
        {
            InitializeComponent();
            timerReloj.Start();
            ActualizarFechaHora();
        }

        // =========================================================
        // Paso 1: insertar tarjeta
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

            // TODO (integración real): acá va la llamada a
            // UsuarioBL.ObtenerDatosLogin(numeroTarjeta), que internamente
            // ejecuta USP_ObtenerDatosLogin. Por ahora solo verificamos
            // que exista en el servicio simulado.
            if (!ServicioLoginSimulado.ExisteTarjeta(numeroTarjeta))
            {
                MessageBox.Show(this, "Tarjeta no reconocida.", "Cajero Automático",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _numeroTarjetaActual = numeroTarjeta;
            MostrarPantallaPin();
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
            _pinIngresado.Clear();
            _numeroTarjetaActual = "";
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
            // ● = dígito cargado, ○ = posición vacía. Nunca se muestra
            // el número real en pantalla, ni siquiera acá.
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
            // El botón CONFIRMAR se usa tanto para validar el PIN como,
            // en un cajero real, para otras confirmaciones. Acá solo nos
            // importa el caso "estamos en la pantalla de PIN".
            if (!pnlIngresarPin.Visible) return;

            if (_pinIngresado.Length != PIN_LONGITUD)
            {
                lblMensajePin.ForeColor = ColoresATM.Advertencia;
                lblMensajePin.Text = "Ingrese los 4 dígitos del PIN.";
                return;
            }

            ValidarLogin();
        }

        private void ValidarLogin()
        {
            // TODO (integración real): reemplazar esta llamada por
            // UsuarioBL.ValidarLogin(_numeroTarjetaActual, pin), que por
            // dentro calcula el hash con PBKDF2 y ejecuta USP_ValidarLogin.
            var resultado = ServicioLoginSimulado.ValidarLogin(_numeroTarjetaActual, _pinIngresado.ToString());

            switch (resultado.Resultado)
            {
                case "OK":
                    AbrirMenuPrincipal(resultado.NombreCliente, resultado.NumeroCuentaEnmascarado, resultado.IdCuenta);
                    break;

                case "PIN_INCORRECTO":
                    lblMensajePin.ForeColor = ColoresATM.Error;
                    lblMensajePin.Text = "PIN incorrecto. Intente nuevamente.";
                    _pinIngresado.Clear();
                    ActualizarDisplayPin();
                    break;

                case "BLOQUEADA":
                    MessageBox.Show(this,
                        "Tarjeta bloqueada por intentos fallidos.\nDiríjase a su sucursal para desbloquearla.",
                        "Cajero Automático", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MostrarPantallaInsertarTarjeta();
                    break;

                case "VENCIDA":
                    MessageBox.Show(this, "La tarjeta se encuentra vencida.", "Cajero Automático",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MostrarPantallaInsertarTarjeta();
                    break;

                default: // NO_EXISTE u otro caso no contemplado
                    MessageBox.Show(this, "No se pudo validar la tarjeta.", "Cajero Automático",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MostrarPantallaInsertarTarjeta();
                    break;
            }
        }

        private void AbrirMenuPrincipal(string nombreCliente, string numeroCuentaEnmascarado, int idCuenta)
        {
            var menu = new frmMenuPrincipal(nombreCliente, numeroCuentaEnmascarado, idCuenta);
            menu.FormClosed += (s, e) => MostrarPantallaInsertarTarjeta(); // al cerrar sesión, vuelve al login limpio
            this.Hide();
            menu.ShowDialog();
            this.Show();
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
