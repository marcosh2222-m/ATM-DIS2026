using System.Drawing;
using System.Windows.Forms;
using CajeroATM.Presentacion.Theme;

namespace CajeroATM.Presentacion.Forms
{
    partial class frmTransferencia
    {
        private Label lblDestino;
        private TextBox txtDestino;
        private Label lblMonto;
        private TextBox txtMonto;
        private Label lblMensaje;
        private Button btnTransferir;
        private Button btnVolver;

        private void InitializeComponent()
        {
            this.lblDestino = new Label();
            this.txtDestino = new TextBox();
            this.lblMonto = new Label();
            this.txtMonto = new TextBox();
            this.lblMensaje = new Label();
            this.btnTransferir = new Button();
            this.btnVolver = new Button();

            this.Text = "Cajero Automático - Transferencia";

            EstiloEtiqueta(this.lblDestino, "Cuenta destino (0000-0000-00):", new Point(20, 24), new Size(338, 24),
                new Font("Consolas", 11F), ColoresATM.TextoPantallaTenue);

            EstiloCampo(this.txtDestino, new Point(20, 52), new Size(338, 40), 16F, 12);
            this.txtDestino.TextAlign = HorizontalAlignment.Center;
            this.txtDestino.KeyPress += new KeyPressEventHandler(this.txtDestino_KeyPress);

            EstiloEtiqueta(this.lblMonto, "Monto ($, múltiplo de 100):", new Point(20, 112), new Size(338, 24),
                new Font("Consolas", 11F), ColoresATM.TextoPantallaTenue);

            EstiloCampo(this.txtMonto, new Point(20, 140), new Size(338, 40), 18F, 9);
            this.txtMonto.KeyPress += new KeyPressEventHandler(this.txtMonto_KeyPress);

            EstiloEtiqueta(this.lblMensaje, "", new Point(20, 196), new Size(338, 184),
                new Font("Segoe UI", 11F, FontStyle.Bold), ColoresATM.TextoPantalla, ContentAlignment.MiddleCenter);

            EstiloBoton(this.btnTransferir, "TRANSFERIR", new Point(20, 400), new Size(338, 55), ColoresATM.Acento);
            this.btnTransferir.Click += new System.EventHandler(this.btnTransferir_Click);
            this.AcceptButton = this.btnTransferir;

            EstiloBoton(this.btnVolver, "VOLVER AL MENÚ", new Point(20, 465), new Size(338, 55), ColoresATM.TextoFooter);
            this.btnVolver.DialogResult = DialogResult.Cancel;
            this.CancelButton = this.btnVolver;

            this.pnlPantalla.Controls.Add(this.lblDestino);
            this.pnlPantalla.Controls.Add(this.txtDestino);
            this.pnlPantalla.Controls.Add(this.lblMonto);
            this.pnlPantalla.Controls.Add(this.txtMonto);
            this.pnlPantalla.Controls.Add(this.lblMensaje);
            this.pnlPantalla.Controls.Add(this.btnTransferir);
            this.pnlPantalla.Controls.Add(this.btnVolver);
        }
    }
}
