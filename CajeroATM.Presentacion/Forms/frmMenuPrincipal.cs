using System;
using System.Windows.Forms;
using CajeroATM.Negocio;

namespace CajeroATM.Presentacion.Forms
{
    public partial class frmMenuPrincipal : Form
    {
        private readonly Sesion _sesion;
        private bool _finalizando;

        /// <summary>
        /// El menú recibe la sesión ya autenticada: de ahí salen el nombre
        /// del cliente, la cuenta y el acceso a las operaciones.
        /// </summary>
        public frmMenuPrincipal(Sesion sesion)
        {
            InitializeComponent();

            _sesion = sesion ?? throw new ArgumentNullException(nameof(sesion));
            lblBienvenida.Text = "Hola, " + sesion.Cliente.ObtenerNombreCompleto();
            lblCuenta.Text = "Cuenta: " + sesion.Cuenta.ObtenerNumeroEnmascarado();

            timerReloj.Start();
            ActualizarFechaHora();
        }

        private void btnConsultarSaldo_Click(object sender, EventArgs e)
        {
            AbrirOperacion(new frmConsultaSaldo(_sesion));
        }

        private void btnExtraccion_Click(object sender, EventArgs e)
        {
            AbrirOperacion(new frmExtraccion(_sesion));
        }

        private void btnTransferencia_Click(object sender, EventArgs e)
        {
            AbrirOperacion(new frmTransferencia(_sesion));
        }

        /// <summary>
        /// Abre la pantalla de la operación y, si se llegó a operar, pregunta
        /// si desea otra (loop "otra operación" del DFD). "No" = finalizar.
        /// </summary>
        private void AbrirOperacion(frmOperacionBase pantalla)
        {
            bool huboOperacion;
            using (pantalla)
            {
                pantalla.ShowDialog(this);
                huboOperacion = pantalla.OperacionRealizada;
            }

            if (!huboOperacion) return;

            var otra = MessageBox.Show(this, "¿Desea realizar otra operación?",
                "Cajero Automático", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (otra == DialogResult.No)
                FinalizarSesion();
        }

        private void btnFinalizar_Click(object sender, EventArgs e)
        {
            var confirmar = MessageBox.Show(this,
                "¿Desea finalizar la operación y retirar su tarjeta?",
                "Cajero Automático", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmar == DialogResult.Yes)
                FinalizarSesion();
        }

        /// <summary>P8 del DFD: ticket (opcional), devolución de tarjeta y cierre de sesión.</summary>
        private void FinalizarSesion()
        {
            _finalizando = true;
            using (var ticket = new frmTicket(_sesion))
            {
                ticket.ShowDialog(this);
            }
            _sesion.Cerrar();
            Close();
        }

        // Si se cierra la ventana con la X, igual se borran los datos de la sesión.
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_finalizando && _sesion.Estado != Sesion.CERRADA)
                _sesion.Cerrar();
            base.OnFormClosing(e);
        }

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
