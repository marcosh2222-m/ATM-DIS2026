using System.Drawing;
using System.Windows.Forms;
using CajeroATM.Presentacion.Theme;

namespace CajeroATM.Presentacion.Forms
{
    partial class frmMenuPrincipal
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlHeader;
        private Label lblBienvenida;
        private Label lblCuenta;

        private Panel pnlOpciones;
        private Button btnConsultarSaldo;
        private Button btnExtraccion;
        private Button btnTransferencia;
        private Button btnFinalizar;

        private Panel pnlFooter;
        private Label lblFechaHora;
        private System.Windows.Forms.Timer timerReloj;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader = new Panel();
            this.lblBienvenida = new Label();
            this.lblCuenta = new Label();

            this.pnlOpciones = new Panel();
            this.btnConsultarSaldo = new Button();
            this.btnExtraccion = new Button();
            this.btnTransferencia = new Button();
            this.btnFinalizar = new Button();

            this.pnlFooter = new Panel();
            this.lblFechaHora = new Label();
            this.components = new System.ComponentModel.Container();
            this.timerReloj = new System.Windows.Forms.Timer(this.components);

            // =========================================================
            // Form
            // =========================================================
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(420, 700);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Cajero Automático - Menú principal";
            this.BackColor = ColoresATM.FondoGabinete;

            // =========================================================
            // Header
            // =========================================================
            this.pnlHeader.BackColor = ColoresATM.FondoHeader;
            this.pnlHeader.Location = new Point(0, 0);
            this.pnlHeader.Size = new Size(420, 90);

            this.lblBienvenida.AutoSize = false;
            this.lblBienvenida.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            this.lblBienvenida.ForeColor = Color.White;
            this.lblBienvenida.Location = new Point(0, 16);
            this.lblBienvenida.Size = new Size(420, 28);
            this.lblBienvenida.TextAlign = ContentAlignment.MiddleCenter;
            this.lblBienvenida.Text = "Bienvenido/a";

            this.lblCuenta.AutoSize = false;
            this.lblCuenta.Font = new Font("Consolas", 10F);
            this.lblCuenta.ForeColor = ColoresATM.Acento;
            this.lblCuenta.Location = new Point(0, 48);
            this.lblCuenta.Size = new Size(420, 22);
            this.lblCuenta.TextAlign = ContentAlignment.MiddleCenter;
            this.lblCuenta.Text = "Cuenta: **** **** ****";

            this.pnlHeader.Controls.Add(this.lblCuenta);
            this.pnlHeader.Controls.Add(this.lblBienvenida);

            // =========================================================
            // Opciones (pantalla)
            // =========================================================
            this.pnlOpciones.BackColor = ColoresATM.FondoPantalla;
            this.pnlOpciones.BorderStyle = BorderStyle.FixedSingle;
            this.pnlOpciones.Location = new Point(20, 110);
            this.pnlOpciones.Size = new Size(380, 500);

            ConfigurarBotonOperacion(this.btnConsultarSaldo, "CONSULTAR SALDO",
                "Ver el saldo disponible de tu cuenta", 30, ColoresATM.Acento);
            this.btnConsultarSaldo.Click += new System.EventHandler(this.btnConsultarSaldo_Click);

            ConfigurarBotonOperacion(this.btnExtraccion, "EXTRACCIÓN DE DINERO",
                "Retirar efectivo de tu cuenta", 145, ColoresATM.Acento);
            this.btnExtraccion.Click += new System.EventHandler(this.btnExtraccion_Click);

            ConfigurarBotonOperacion(this.btnTransferencia, "TRANSFERENCIA",
                "Enviar dinero a otra cuenta", 260, ColoresATM.Acento);
            this.btnTransferencia.Click += new System.EventHandler(this.btnTransferencia_Click);

            ConfigurarBotonOperacion(this.btnFinalizar, "FINALIZAR OPERACIÓN",
                "Retirar la tarjeta y salir", 400, ColoresATM.Error);
            this.btnFinalizar.Click += new System.EventHandler(this.btnFinalizar_Click);

            this.pnlOpciones.Controls.Add(this.btnConsultarSaldo);
            this.pnlOpciones.Controls.Add(this.btnExtraccion);
            this.pnlOpciones.Controls.Add(this.btnTransferencia);
            this.pnlOpciones.Controls.Add(this.btnFinalizar);

            // =========================================================
            // Footer
            // =========================================================
            this.pnlFooter.BackColor = ColoresATM.FondoHeader;
            this.pnlFooter.Location = new Point(0, 660);
            this.pnlFooter.Size = new Size(420, 40);

            this.lblFechaHora.AutoSize = false;
            this.lblFechaHora.Dock = DockStyle.Fill;
            this.lblFechaHora.Font = ColoresATM.FuenteFooter;
            this.lblFechaHora.ForeColor = ColoresATM.TextoFooter;
            this.lblFechaHora.TextAlign = ContentAlignment.MiddleCenter;
            this.pnlFooter.Controls.Add(this.lblFechaHora);

            this.timerReloj.Interval = 1000;
            this.timerReloj.Tick += new System.EventHandler(this.timerReloj_Tick);

            // =========================================================
            // Alta de controles en el Form
            // =========================================================
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlOpciones);
            this.Controls.Add(this.pnlHeader);
        }

        /// <summary>
        /// Arma un botón de operación "grande", con título y una
        /// bajada descriptiva chica, imitando los botones de opción
        /// de un cajero real (uno por función, bien separados).
        /// </summary>
        private void ConfigurarBotonOperacion(Button boton, string titulo, string descripcion, int y, Color colorAcento)
        {
            boton.FlatStyle = FlatStyle.Flat;
            boton.BackColor = ColoresATM.FondoTecla;
            boton.ForeColor = Color.White;
            boton.Location = new Point(20, y);
            boton.Size = new Size(340, 95);
            boton.UseVisualStyleBackColor = false;
            boton.TextAlign = ContentAlignment.MiddleCenter;
            boton.FlatAppearance.BorderSize = 2;
            boton.FlatAppearance.BorderColor = colorAcento;
            boton.FlatAppearance.MouseOverBackColor = ColoresATM.FondoTeclaHover;

            // Texto de dos líneas: título en grande + bajada chica.
            // (Button no soporta dos fuentes distintas de forma nativa,
            // así que usamos \n con un tamaño intermedio que se lea bien.)
            boton.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            boton.Text = titulo + "\n" + descripcion;
        }
    }
}
