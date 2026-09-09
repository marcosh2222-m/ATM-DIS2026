using System;
using System.Windows.Forms;

namespace CajeroATM.Presentacion.Forms
{
    public partial class frmMenuPrincipal : Form
    {
        private readonly int _idCuenta;

        /// <summary>
        /// El menú se arma con los datos que ya validó el login, para no
        /// tener que volver a consultarlos acá.
        /// </summary>
        public frmMenuPrincipal(string nombreCliente, string numeroCuentaEnmascarado, int idCuenta)
        {
            InitializeComponent();

            _idCuenta = idCuenta;
            lblBienvenida.Text = $"Hola, {nombreCliente}";
            lblCuenta.Text = $"Cuenta: {numeroCuentaEnmascarado}";

            timerReloj.Start();
            ActualizarFechaHora();
        }

        private void btnConsultarSaldo_Click(object sender, EventArgs e)
        {
            // TODO: reemplazar por la apertura real de frmConsultaSaldo,
            // que va a llamar a CuentaBL.ConsultarSaldo(_idCuenta) y
            // eso a su vez a USP_ConsultarSaldo.
            MessageBox.Show(this, "Pantalla de Consulta de Saldo — próximamente.",
                "Cajero Automático", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnExtraccion_Click(object sender, EventArgs e)
        {
            // TODO: abrir frmExtraccion, que llama a
            // TransaccionBL.Extraer(_idCuenta, monto) -> USP_Extraccion.
            MessageBox.Show(this, "Pantalla de Extracción de Dinero — próximamente.",
                "Cajero Automático", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnTransferencia_Click(object sender, EventArgs e)
        {
            // TODO: abrir frmTransferencia, que llama a
            // TransaccionBL.Transferir(_idCuenta, cuentaDestino, monto)
            // -> USP_Transferencia.
            MessageBox.Show(this, "Pantalla de Transferencia — próximamente.",
                "Cajero Automático", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnFinalizar_Click(object sender, EventArgs e)
        {
            var confirmar = MessageBox.Show(this,
                "¿Desea finalizar la operación y retirar su tarjeta?",
                "Cajero Automático", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmar == DialogResult.Yes)
            {
                this.Close(); // dispara FormClosed en frmLogin, que resetea la pantalla
            }
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
