using System.Drawing;
using System.Windows.Forms;
using CajeroATM.Presentacion.Theme;

namespace CajeroATM.Presentacion.Forms
{
    partial class frmLogin
    {
        private System.ComponentModel.IContainer components = null;

        // --- Estructura general ---
        private Panel pnlHeader;
        private Label lblTitulo;
        private Label lblSucursal;

        private Panel pnlPantalla;
        private Panel pnlFooter;
        private Label lblFechaHora;
        private System.Windows.Forms.Timer timerReloj;

        // --- Pantalla: paso 1, insertar tarjeta ---
        private Panel pnlInsertarTarjeta;
        private Label lblBienvenida;
        private Label lblInstruccionTarjeta;
        private Panel pnlRanura;
        private ComboBox cmbNumeroTarjeta;
        private Button btnInsertar;

        // --- Pantalla: paso 2, ingresar PIN ---
        private Panel pnlIngresarPin;
        private Label lblInstruccionPin;
        private Label lblPinDisplay;
        private Label lblMensajePin;

        // --- Teclado numérico ---
        private TableLayoutPanel tlpTeclado;
        private Button btnConfirmar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader = new Panel();
            this.lblTitulo = new Label();
            this.lblSucursal = new Label();
            this.pnlPantalla = new Panel();
            this.pnlFooter = new Panel();
            this.lblFechaHora = new Label();
            this.components = new System.ComponentModel.Container();
            this.timerReloj = new System.Windows.Forms.Timer(this.components);

            this.pnlInsertarTarjeta = new Panel();
            this.lblBienvenida = new Label();
            this.lblInstruccionTarjeta = new Label();
            this.pnlRanura = new Panel();
            this.cmbNumeroTarjeta = new ComboBox();
            this.btnInsertar = new Button();

            this.pnlIngresarPin = new Panel();
            this.lblInstruccionPin = new Label();
            this.lblPinDisplay = new Label();
            this.lblMensajePin = new Label();

            this.tlpTeclado = new TableLayoutPanel();
            this.btnConfirmar = new Button();

            // =========================================================
            // Form
            // =========================================================
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(420, 700);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Cajero Automático";
            this.BackColor = ColoresATM.FondoGabinete;

            // =========================================================
            // Header
            // =========================================================
            this.pnlHeader.BackColor = ColoresATM.FondoHeader;
            this.pnlHeader.Location = new Point(0, 0);
            this.pnlHeader.Size = new Size(420, 65);

            this.lblTitulo.AutoSize = false;
            this.lblTitulo.Font = ColoresATM.FuenteTitulo;
            this.lblTitulo.ForeColor = Color.White;
            this.lblTitulo.Location = new Point(0, 12);
            this.lblTitulo.Size = new Size(420, 26);
            this.lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            this.lblTitulo.Text = "CAJERO AUTOMÁTICO";

            this.lblSucursal.AutoSize = false;
            this.lblSucursal.Font = ColoresATM.FuenteFooter;
            this.lblSucursal.ForeColor = ColoresATM.TextoFooter;
            this.lblSucursal.Location = new Point(0, 40);
            this.lblSucursal.Size = new Size(420, 18);
            this.lblSucursal.TextAlign = ContentAlignment.MiddleCenter;
            this.lblSucursal.Text = "Sucursal Centro  ·  ATM-0001";

            this.pnlHeader.Controls.Add(this.lblTitulo);
            this.pnlHeader.Controls.Add(this.lblSucursal);

            // =========================================================
            // Pantalla (LCD)
            // =========================================================
            this.pnlPantalla.BackColor = ColoresATM.FondoPantalla;
            this.pnlPantalla.BorderStyle = BorderStyle.FixedSingle;
            this.pnlPantalla.Location = new Point(20, 85);
            this.pnlPantalla.Size = new Size(380, 250);

            // --- Paso 1: insertar tarjeta ---
            this.pnlInsertarTarjeta.BackColor = ColoresATM.FondoPantalla;
            this.pnlInsertarTarjeta.Dock = DockStyle.Fill;
            this.pnlInsertarTarjeta.Visible = true;

            this.pnlRanura.BackColor = ColoresATM.FondoTecla;
            this.pnlRanura.Location = new Point(140, 20);
            this.pnlRanura.Size = new Size(100, 10);

            this.lblBienvenida.AutoSize = false;
            this.lblBienvenida.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            this.lblBienvenida.ForeColor = ColoresATM.TextoPantalla;
            this.lblBienvenida.Location = new Point(15, 50);
            this.lblBienvenida.Size = new Size(350, 30);
            this.lblBienvenida.TextAlign = ContentAlignment.MiddleCenter;
            this.lblBienvenida.Text = "Bienvenido";

            this.lblInstruccionTarjeta.AutoSize = false;
            this.lblInstruccionTarjeta.Font = ColoresATM.FuentePantalla;
            this.lblInstruccionTarjeta.ForeColor = ColoresATM.TextoPantallaTenue;
            this.lblInstruccionTarjeta.Location = new Point(15, 90);
            this.lblInstruccionTarjeta.Size = new Size(350, 50);
            this.lblInstruccionTarjeta.TextAlign = ContentAlignment.MiddleCenter;
            this.lblInstruccionTarjeta.Text = "Inserte su tarjeta para comenzar";

            this.cmbNumeroTarjeta.Font = new Font("Consolas", 11F);
            this.cmbNumeroTarjeta.Location = new Point(60, 145);
            this.cmbNumeroTarjeta.Size = new Size(260, 26);
            this.cmbNumeroTarjeta.FlatStyle = FlatStyle.Flat;
            this.cmbNumeroTarjeta.DropDownStyle = ComboBoxStyle.DropDown;
            // NOTA: en un cajero real esto no existe (la tarjeta se lee con
            // hardware). Lo dejamos como combo editable para poder simular
            // distintas tarjetas de prueba durante el desarrollo.
            // Tarjetas de los datos de prueba (script 01): PIN 1234 y PIN 5678
            this.cmbNumeroTarjeta.Items.Add("4000111122223333");
            this.cmbNumeroTarjeta.Items.Add("4000444455556666");

            this.btnInsertar.BackColor = ColoresATM.Acento;
            this.btnInsertar.FlatStyle = FlatStyle.Flat;
            this.btnInsertar.FlatAppearance.BorderSize = 0;
            this.btnInsertar.Font = ColoresATM.FuenteBoton;
            this.btnInsertar.ForeColor = Color.White;
            this.btnInsertar.Location = new Point(90, 190);
            this.btnInsertar.Size = new Size(200, 40);
            this.btnInsertar.Text = "INSERTAR TARJETA";
            this.btnInsertar.UseVisualStyleBackColor = false;
            this.btnInsertar.Click += new System.EventHandler(this.btnInsertar_Click);

            this.pnlInsertarTarjeta.Controls.Add(this.btnInsertar);
            this.pnlInsertarTarjeta.Controls.Add(this.cmbNumeroTarjeta);
            this.pnlInsertarTarjeta.Controls.Add(this.lblInstruccionTarjeta);
            this.pnlInsertarTarjeta.Controls.Add(this.lblBienvenida);
            this.pnlInsertarTarjeta.Controls.Add(this.pnlRanura);

            // --- Paso 2: ingresar PIN ---
            this.pnlIngresarPin.BackColor = ColoresATM.FondoPantalla;
            this.pnlIngresarPin.Dock = DockStyle.Fill;
            this.pnlIngresarPin.Visible = false;

            this.lblInstruccionPin.AutoSize = false;
            this.lblInstruccionPin.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblInstruccionPin.ForeColor = ColoresATM.TextoPantalla;
            this.lblInstruccionPin.Location = new Point(15, 30);
            this.lblInstruccionPin.Size = new Size(350, 30);
            this.lblInstruccionPin.TextAlign = ContentAlignment.MiddleCenter;
            this.lblInstruccionPin.Text = "Ingrese su PIN";

            this.lblPinDisplay.AutoSize = false;
            this.lblPinDisplay.Font = ColoresATM.FuentePin;
            this.lblPinDisplay.ForeColor = ColoresATM.TextoPantalla;
            this.lblPinDisplay.Location = new Point(15, 80);
            this.lblPinDisplay.Size = new Size(350, 45);
            this.lblPinDisplay.TextAlign = ContentAlignment.MiddleCenter;
            this.lblPinDisplay.Text = "○ ○ ○ ○";

            this.lblMensajePin.AutoSize = false;
            this.lblMensajePin.Font = ColoresATM.FuenteFooter;
            this.lblMensajePin.ForeColor = ColoresATM.Error;
            this.lblMensajePin.Location = new Point(15, 140);
            this.lblMensajePin.Size = new Size(350, 60);
            this.lblMensajePin.TextAlign = ContentAlignment.MiddleCenter;
            this.lblMensajePin.Text = "";

            this.pnlIngresarPin.Controls.Add(this.lblMensajePin);
            this.pnlIngresarPin.Controls.Add(this.lblPinDisplay);
            this.pnlIngresarPin.Controls.Add(this.lblInstruccionPin);

            this.pnlPantalla.Controls.Add(this.pnlIngresarPin);
            this.pnlPantalla.Controls.Add(this.pnlInsertarTarjeta);

            // =========================================================
            // Teclado numérico
            // =========================================================
            this.tlpTeclado.Location = new Point(20, 350);
            this.tlpTeclado.Size = new Size(380, 220);
            this.tlpTeclado.ColumnCount = 3;
            this.tlpTeclado.RowCount = 4;
            this.tlpTeclado.BackColor = ColoresATM.FondoGabinete;
            for (int i = 0; i < 3; i++)
                this.tlpTeclado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            for (int i = 0; i < 4; i++)
                this.tlpTeclado.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

            AgregarTecla("1", 0, 0);
            AgregarTecla("2", 1, 0);
            AgregarTecla("3", 2, 0);
            AgregarTecla("4", 0, 1);
            AgregarTecla("5", 1, 1);
            AgregarTecla("6", 2, 1);
            AgregarTecla("7", 0, 2);
            AgregarTecla("8", 1, 2);
            AgregarTecla("9", 2, 2);
            AgregarTeclaEspecial("CANCELAR", 0, 3, ColoresATM.Error, this.btnCancelar_Click);
            AgregarTecla("0", 1, 3);
            AgregarTeclaEspecial("BORRAR", 2, 3, ColoresATM.Advertencia, this.btnBorrar_Click);

            // =========================================================
            // Confirmar
            // =========================================================
            this.btnConfirmar.BackColor = ColoresATM.Acento;
            this.btnConfirmar.FlatStyle = FlatStyle.Flat;
            this.btnConfirmar.FlatAppearance.BorderSize = 0;
            this.btnConfirmar.Font = ColoresATM.FuenteBoton;
            this.btnConfirmar.ForeColor = Color.White;
            this.btnConfirmar.Location = new Point(20, 580);
            this.btnConfirmar.Size = new Size(380, 42);
            this.btnConfirmar.Text = "CONFIRMAR";
            this.btnConfirmar.UseVisualStyleBackColor = false;
            this.btnConfirmar.Click += new System.EventHandler(this.btnConfirmar_Click);

            // =========================================================
            // Footer
            // =========================================================
            this.pnlFooter.BackColor = ColoresATM.FondoHeader;
            this.pnlFooter.Location = new Point(0, 660);
            this.pnlFooter.Size = new Size(420, 40);

            this.lblFechaHora.AutoSize = false;
            this.lblFechaHora.Font = ColoresATM.FuenteFooter;
            this.lblFechaHora.ForeColor = ColoresATM.TextoFooter;
            this.lblFechaHora.Dock = DockStyle.Fill;
            this.lblFechaHora.TextAlign = ContentAlignment.MiddleCenter;
            this.pnlFooter.Controls.Add(this.lblFechaHora);

            this.timerReloj.Interval = 1000;
            this.timerReloj.Tick += new System.EventHandler(this.timerReloj_Tick);

            // =========================================================
            // Alta de controles en el Form
            // =========================================================
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.btnConfirmar);
            this.Controls.Add(this.tlpTeclado);
            this.Controls.Add(this.pnlPantalla);
            this.Controls.Add(this.pnlHeader);
        }

        /// <summary>Crea una tecla numérica estándar y la agrega a la grilla del teclado.</summary>
        private void AgregarTecla(string numero, int columna, int fila)
        {
            var boton = new Button
            {
                Text = numero,
                Font = ColoresATM.FuenteTecla,
                ForeColor = ColoresATM.TextoTecla,
                BackColor = ColoresATM.FondoTecla,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(4),
                UseVisualStyleBackColor = false,
                Tag = numero,
            };
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = ColoresATM.FondoTeclaHover;
            boton.Click += new System.EventHandler(this.btnDigito_Click);
            this.tlpTeclado.Controls.Add(boton, columna, fila);
        }

        /// <summary>Crea una tecla de función (Cancelar / Borrar) con color distintivo.</summary>
        private void AgregarTeclaEspecial(string texto, int columna, int fila, Color color, System.EventHandler manejador)
        {
            var boton = new Button
            {
                Text = texto,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = color,
                BackColor = ColoresATM.FondoTecla,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(4),
                UseVisualStyleBackColor = false,
            };
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = ColoresATM.FondoTeclaHover;
            boton.Click += manejador;
            this.tlpTeclado.Controls.Add(boton, columna, fila);
        }
    }
}
