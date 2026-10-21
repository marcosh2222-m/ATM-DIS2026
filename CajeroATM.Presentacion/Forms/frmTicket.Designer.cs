using System.Drawing;
using System.Windows.Forms;
using CajeroATM.Presentacion.Theme;

namespace CajeroATM.Presentacion.Forms
{
    partial class frmTicket
    {
        private Label lblPregunta;
        private TextBox txtTicket;
        private Button btnImprimir;
        private Button btnRetirarTarjeta;

        private void InitializeComponent()
        {
            this.lblPregunta = new Label();
            this.txtTicket = new TextBox();
            this.btnImprimir = new Button();
            this.btnRetirarTarjeta = new Button();

            this.Text = "Cajero Automático - Fin de la operación";

            EstiloEtiqueta(this.lblPregunta, "Detalle de la sesión:", new Point(20, 10), new Size(338, 24),
                new Font("Consolas", 11F), ColoresATM.TextoPantallaTenue);

            // El ticket se muestra como "papel": fondo blanco y letra negra monoespaciada.
            this.txtTicket.Multiline = true;
            this.txtTicket.ReadOnly = true;
            this.txtTicket.ScrollBars = ScrollBars.Vertical;
            this.txtTicket.BorderStyle = BorderStyle.None;
            this.txtTicket.BackColor = Color.White;
            this.txtTicket.ForeColor = Color.Black;
            this.txtTicket.Font = new Font("Consolas", 9.75F);
            this.txtTicket.Location = new Point(20, 38);
            this.txtTicket.Size = new Size(338, 350);
            this.txtTicket.TabStop = false;
            this.txtTicket.WordWrap = false;

            EstiloBoton(this.btnImprimir, "IMPRIMIR TICKET", new Point(20, 400), new Size(338, 55), ColoresATM.Acento);
            this.btnImprimir.Click += new System.EventHandler(this.btnImprimir_Click);

            EstiloBoton(this.btnRetirarTarjeta, "RETIRAR TARJETA Y SALIR", new Point(20, 465), new Size(338, 55), ColoresATM.Error);
            this.btnRetirarTarjeta.DialogResult = DialogResult.OK;
            this.AcceptButton = this.btnRetirarTarjeta;
            this.CancelButton = this.btnRetirarTarjeta;

            this.pnlPantalla.Controls.Add(this.lblPregunta);
            this.pnlPantalla.Controls.Add(this.txtTicket);
            this.pnlPantalla.Controls.Add(this.btnImprimir);
            this.pnlPantalla.Controls.Add(this.btnRetirarTarjeta);
        }
    }
}
