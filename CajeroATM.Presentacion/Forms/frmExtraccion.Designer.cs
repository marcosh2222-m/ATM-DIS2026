using System.Drawing;
using System.Windows.Forms;
using CajeroATM.Presentacion.Theme;

namespace CajeroATM.Presentacion.Forms
{
    partial class frmExtraccion
    {
        private Label lblIndicacion;
        private Button btnMonto1000;
        private Button btnMonto2000;
        private Button btnMonto5000;
        private Button btnMonto10000;
        private Button btnMonto20000;
        private Button btnMonto50000;
        private Label lblMonto;
        private TextBox txtMonto;
        private Label lblMensaje;
        private Button btnExtraer;
        private Button btnVolver;

        private void InitializeComponent()
        {
            this.lblIndicacion = new Label();
            this.btnMonto1000 = new Button();
            this.btnMonto2000 = new Button();
            this.btnMonto5000 = new Button();
            this.btnMonto10000 = new Button();
            this.btnMonto20000 = new Button();
            this.btnMonto50000 = new Button();
            this.lblMonto = new Label();
            this.txtMonto = new TextBox();
            this.lblMensaje = new Label();
            this.btnExtraer = new Button();
            this.btnVolver = new Button();

            this.Text = "Cajero Automático - Extracción de dinero";

            EstiloEtiqueta(this.lblIndicacion, "Elija un monto rápido o ingrese otro:", new Point(20, 14), new Size(338, 24),
                new Font("Consolas", 11F), ColoresATM.TextoPantallaTenue);

            ConfigurarMontoRapido(this.btnMonto1000, 1000, 20, 44);
            ConfigurarMontoRapido(this.btnMonto2000, 2000, 198, 44);
            ConfigurarMontoRapido(this.btnMonto5000, 5000, 20, 100);
            ConfigurarMontoRapido(this.btnMonto10000, 10000, 198, 100);
            ConfigurarMontoRapido(this.btnMonto20000, 20000, 20, 156);
            ConfigurarMontoRapido(this.btnMonto50000, 50000, 198, 156);

            EstiloEtiqueta(this.lblMonto, "Monto a extraer ($, múltiplo de 100):", new Point(20, 224), new Size(338, 24),
                new Font("Consolas", 11F), ColoresATM.TextoPantallaTenue);

            EstiloCampo(this.txtMonto, new Point(20, 252), new Size(338, 40), 18F, 9);
            this.txtMonto.KeyPress += new KeyPressEventHandler(this.txtMonto_KeyPress);

            EstiloEtiqueta(this.lblMensaje, "", new Point(20, 302), new Size(338, 88),
                new Font("Segoe UI", 11F, FontStyle.Bold), ColoresATM.TextoPantalla, ContentAlignment.MiddleCenter);

            EstiloBoton(this.btnExtraer, "EXTRAER", new Point(20, 400), new Size(338, 55), ColoresATM.Acento);
            this.btnExtraer.Click += new System.EventHandler(this.btnExtraer_Click);
            this.AcceptButton = this.btnExtraer;

            EstiloBoton(this.btnVolver, "VOLVER AL MENÚ", new Point(20, 465), new Size(338, 55), ColoresATM.TextoFooter);
            this.btnVolver.DialogResult = DialogResult.Cancel;
            this.CancelButton = this.btnVolver;

            this.pnlPantalla.Controls.Add(this.lblIndicacion);
            this.pnlPantalla.Controls.Add(this.btnMonto1000);
            this.pnlPantalla.Controls.Add(this.btnMonto2000);
            this.pnlPantalla.Controls.Add(this.btnMonto5000);
            this.pnlPantalla.Controls.Add(this.btnMonto10000);
            this.pnlPantalla.Controls.Add(this.btnMonto20000);
            this.pnlPantalla.Controls.Add(this.btnMonto50000);
            this.pnlPantalla.Controls.Add(this.lblMonto);
            this.pnlPantalla.Controls.Add(this.txtMonto);
            this.pnlPantalla.Controls.Add(this.lblMensaje);
            this.pnlPantalla.Controls.Add(this.btnExtraer);
            this.pnlPantalla.Controls.Add(this.btnVolver);
        }

        private void ConfigurarMontoRapido(Button boton, int monto, int x, int y)
        {
            EstiloBoton(boton, "$ " + monto.ToString("N0", new System.Globalization.CultureInfo("es-AR")),
                new Point(x, y), new Size(160, 46), ColoresATM.Acento, 12F);
            boton.Tag = monto.ToString();
            boton.Click += new System.EventHandler(this.btnMontoRapido_Click);
        }
    }
}
