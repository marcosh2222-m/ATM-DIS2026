using System;
using System.Drawing;
using System.Windows.Forms;
using CajeroATM.Negocio;
using CajeroATM.Negocio.Operaciones;
using CajeroATM.Presentacion.Theme;

namespace CajeroATM.Presentacion.Forms
{
    public partial class frmTransferencia : frmOperacionBase
    {
        private readonly Sesion _sesion;

        public frmTransferencia(Sesion sesion)
        {
            InitializeComponent();
            _sesion = sesion ?? throw new ArgumentNullException(nameof(sesion));
            ConfigurarEncabezado("TRANSFERENCIA", sesion.Cuenta.ObtenerNumeroEnmascarado());
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            txtDestino.Focus();
        }

        private void txtDestino_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '-')
                e.Handled = true;
        }

        private void txtMonto_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void btnTransferir_Click(object sender, EventArgs e)
        {
            if (!Validaciones.TryNormalizarCuenta(txtDestino.Text, out string cuentaDestino, out string errorCuenta))
            {
                Mostrar(errorCuenta, ColoresATM.Advertencia);
                txtDestino.Focus();
                return;
            }

            if (!Validaciones.TryParseMonto(txtMonto.Text, out decimal monto, out string errorMonto))
            {
                Mostrar(errorMonto, ColoresATM.Advertencia);
                txtMonto.Focus();
                return;
            }

            var confirmar = MessageBox.Show(this,
                "¿Confirma la transferencia?\n\n" +
                "Cuenta destino: " + cuentaDestino + "\n" +
                "Monto: " + Formato.Moneda(monto),
                "Cajero Automático", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmar != DialogResult.Yes) return;

            var transferencia = new Transferencia(_sesion, cuentaDestino, monto);

            Cursor = Cursors.WaitCursor;
            bool ok = transferencia.Ejecutar();
            Cursor = Cursors.Default;
            OperacionRealizada = true;

            if (ok)
            {
                Mostrar("Transferencia realizada.\n" +
                        "Destino: " + transferencia.NumeroCuentaDestino + "\n" +
                        "Monto: " + Formato.Moneda(monto) + "\n" +
                        "Saldo restante: " + Formato.Moneda(transferencia.SaldoResultante ?? 0m) + "\n" +
                        "Comprobante N° " + transferencia.IdComprobante, ColoresATM.TextoPantalla);
                txtDestino.Clear();
                txtMonto.Clear();
                return;
            }

            string mensaje = MensajesOperacion.Traducir(transferencia.CodigoResultado);
            if (transferencia.CodigoResultado == "SALDO_INSUFICIENTE" && transferencia.SaldoResultante.HasValue)
                mensaje += "\nSaldo disponible: " + Formato.Moneda(transferencia.SaldoResultante.Value);
            Mostrar(mensaje, ColoresATM.Error);
        }

        private void Mostrar(string texto, Color color)
        {
            lblMensaje.ForeColor = color;
            lblMensaje.Text = texto;
        }
    }
}
