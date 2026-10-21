using System;
using System.Drawing;
using System.Windows.Forms;
using CajeroATM.Negocio;
using CajeroATM.Negocio.Operaciones;
using CajeroATM.Presentacion.Theme;

namespace CajeroATM.Presentacion.Forms
{
    public partial class frmExtraccion : frmOperacionBase
    {
        private readonly Sesion _sesion;

        public frmExtraccion(Sesion sesion)
        {
            InitializeComponent();
            _sesion = sesion ?? throw new ArgumentNullException(nameof(sesion));
            ConfigurarEncabezado("EXTRACCIÓN DE DINERO", sesion.Cuenta.ObtenerNumeroEnmascarado());
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            txtMonto.Focus();
        }

        private void btnMontoRapido_Click(object sender, EventArgs e)
        {
            txtMonto.Text = ((Button)sender).Tag.ToString();
            txtMonto.SelectionStart = txtMonto.Text.Length;
            txtMonto.Focus();
            Mostrar("", ColoresATM.TextoPantalla);
        }

        // Solo dígitos: el monto es siempre un entero (múltiplo de 100).
        private void txtMonto_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void btnExtraer_Click(object sender, EventArgs e)
        {
            if (!Validaciones.TryParseMonto(txtMonto.Text, out decimal monto, out string error))
            {
                Mostrar(error, ColoresATM.Advertencia);
                txtMonto.Focus();
                return;
            }

            var confirmar = MessageBox.Show(this,
                "¿Confirma la extracción de " + Formato.Moneda(monto) + "?",
                "Cajero Automático", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmar != DialogResult.Yes) return;

            var extraccion = new Extraccion(_sesion, monto);

            Cursor = Cursors.WaitCursor;
            bool ok = extraccion.Ejecutar();
            Cursor = Cursors.Default;
            OperacionRealizada = true;

            if (ok)
            {
                Mostrar("Retire su dinero.\n" +
                        "Saldo restante: " + Formato.Moneda(extraccion.SaldoResultante ?? 0m) + "\n" +
                        "Comprobante N° " + extraccion.IdComprobante, ColoresATM.TextoPantalla);
                txtMonto.Clear();
                return;
            }

            string mensaje = MensajesOperacion.Traducir(extraccion.CodigoResultado);
            if (extraccion.CodigoResultado == "SALDO_INSUFICIENTE" && extraccion.SaldoResultante.HasValue)
                mensaje += "\nSaldo disponible: " + Formato.Moneda(extraccion.SaldoResultante.Value);
            Mostrar(mensaje, ColoresATM.Error);
        }

        private void Mostrar(string texto, Color color)
        {
            lblMensaje.ForeColor = color;
            lblMensaje.Text = texto;
        }
    }
}
