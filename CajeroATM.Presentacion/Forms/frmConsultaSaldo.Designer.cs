using System.Drawing;
using System.Windows.Forms;
using CajeroATM.Presentacion.Theme;

namespace CajeroATM.Presentacion.Forms
{
    partial class frmConsultaSaldo
    {
        private Label lblEtiquetaSaldo;
        private Label lblSaldo;
        private Label lblTipoCuenta;
        private Label lblEstadoCuenta;
        private Label lblFechaConsulta;
        private Label lblComprobante;
        private Label lblMensaje;
        private Button btnActualizar;
        private Button btnVolver;

        private void InitializeComponent()
        {
            this.lblEtiquetaSaldo = new Label();
            this.lblSaldo = new Label();
            this.lblTipoCuenta = new Label();
            this.lblEstadoCuenta = new Label();
            this.lblFechaConsulta = new Label();
            this.lblComprobante = new Label();
            this.lblMensaje = new Label();
            this.btnActualizar = new Button();
            this.btnVolver = new Button();

            this.Text = "Cajero Automático - Consulta de saldo";

            // Fuente más chica para que entren líneas como "Consulta : 20/10/2026 19:56:01".
            var fuenteDetalle = new Font("Consolas", 11F);

            EstiloEtiqueta(this.lblEtiquetaSaldo, "SALDO DISPONIBLE", new Point(20, 30), new Size(338, 26),
                new Font("Consolas", 12F, FontStyle.Bold), ColoresATM.TextoPantallaTenue, ContentAlignment.MiddleCenter);

            EstiloEtiqueta(this.lblSaldo, "$ 0,00", new Point(10, 62), new Size(358, 64),
                new Font("Consolas", 28F, FontStyle.Bold), ColoresATM.TextoPantalla, ContentAlignment.MiddleCenter);

            EstiloEtiqueta(this.lblTipoCuenta, "", new Point(20, 160), new Size(338, 26),
                fuenteDetalle, ColoresATM.TextoPantalla);
            EstiloEtiqueta(this.lblEstadoCuenta, "", new Point(20, 190), new Size(338, 26),
                fuenteDetalle, ColoresATM.TextoPantalla);
            EstiloEtiqueta(this.lblFechaConsulta, "", new Point(20, 220), new Size(338, 26),
                fuenteDetalle, ColoresATM.TextoPantalla);
            EstiloEtiqueta(this.lblComprobante, "", new Point(20, 250), new Size(338, 26),
                fuenteDetalle, ColoresATM.TextoPantalla);

            EstiloEtiqueta(this.lblMensaje, "", new Point(20, 300), new Size(338, 80),
                new Font("Segoe UI", 11F, FontStyle.Bold), ColoresATM.Error, ContentAlignment.MiddleCenter);

            EstiloBoton(this.btnActualizar, "ACTUALIZAR SALDO", new Point(20, 400), new Size(338, 55), ColoresATM.Acento);
            this.btnActualizar.Click += new System.EventHandler(this.btnActualizar_Click);

            EstiloBoton(this.btnVolver, "VOLVER AL MENÚ", new Point(20, 465), new Size(338, 55), ColoresATM.TextoFooter);
            this.btnVolver.DialogResult = DialogResult.Cancel;   // cierra la pantalla y vuelve al menú
            this.CancelButton = this.btnVolver;

            this.pnlPantalla.Controls.Add(this.lblEtiquetaSaldo);
            this.pnlPantalla.Controls.Add(this.lblSaldo);
            this.pnlPantalla.Controls.Add(this.lblTipoCuenta);
            this.pnlPantalla.Controls.Add(this.lblEstadoCuenta);
            this.pnlPantalla.Controls.Add(this.lblFechaConsulta);
            this.pnlPantalla.Controls.Add(this.lblComprobante);
            this.pnlPantalla.Controls.Add(this.lblMensaje);
            this.pnlPantalla.Controls.Add(this.btnActualizar);
            this.pnlPantalla.Controls.Add(this.btnVolver);
        }
    }
}
