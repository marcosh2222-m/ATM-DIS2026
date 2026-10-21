using System;
using System.Drawing;
using System.Windows.Forms;
using CajeroATM.Presentacion.Theme;

namespace CajeroATM.Presentacion.Forms
{
    /// <summary>
    /// Marco visual común de las pantallas de operación (consulta,
    /// extracción, transferencia): barra superior con título y cuenta,
    /// "pantalla" negra para el contenido y pie con reloj.
    /// Los formularios hijos agregan sus controles a <see cref="pnlPantalla"/>
    /// (área de 378 x 528) y llaman a <see cref="ConfigurarEncabezado"/>.
    /// </summary>
    public class frmOperacionBase : Form
    {
        private readonly Panel _pnlHeader = new Panel();
        private readonly Label _lblTitulo = new Label();
        private readonly Label _lblCuenta = new Label();
        private readonly Panel _pnlFooter = new Panel();
        private readonly Label _lblFechaHora = new Label();
        private readonly System.Windows.Forms.Timer _timerReloj = new System.Windows.Forms.Timer();

        protected readonly Panel pnlPantalla = new Panel();

        /// <summary>
        /// true si en esta pantalla se llegó a ejecutar al menos una operación
        /// (exitosa o rechazada). El menú lo usa para preguntar "¿otra operación?".
        /// </summary>
        public bool OperacionRealizada { get; protected set; }

        public frmOperacionBase()
        {
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(420, 700);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = ColoresATM.FondoGabinete;

            // Header
            _pnlHeader.BackColor = ColoresATM.FondoHeader;
            _pnlHeader.Location = new Point(0, 0);
            _pnlHeader.Size = new Size(420, 90);

            _lblTitulo.AutoSize = false;
            _lblTitulo.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            _lblTitulo.ForeColor = Color.White;
            _lblTitulo.Location = new Point(0, 16);
            _lblTitulo.Size = new Size(420, 28);
            _lblTitulo.TextAlign = ContentAlignment.MiddleCenter;

            _lblCuenta.AutoSize = false;
            _lblCuenta.Font = new Font("Consolas", 10F);
            _lblCuenta.ForeColor = ColoresATM.Acento;
            _lblCuenta.Location = new Point(0, 48);
            _lblCuenta.Size = new Size(420, 22);
            _lblCuenta.TextAlign = ContentAlignment.MiddleCenter;

            _pnlHeader.Controls.Add(_lblCuenta);
            _pnlHeader.Controls.Add(_lblTitulo);

            // Pantalla
            pnlPantalla.BackColor = ColoresATM.FondoPantalla;
            pnlPantalla.BorderStyle = BorderStyle.FixedSingle;
            pnlPantalla.Location = new Point(20, 110);
            pnlPantalla.Size = new Size(380, 530);

            // Footer
            _pnlFooter.BackColor = ColoresATM.FondoHeader;
            _pnlFooter.Location = new Point(0, 660);
            _pnlFooter.Size = new Size(420, 40);

            _lblFechaHora.AutoSize = false;
            _lblFechaHora.Dock = DockStyle.Fill;
            _lblFechaHora.Font = ColoresATM.FuenteFooter;
            _lblFechaHora.ForeColor = ColoresATM.TextoFooter;
            _lblFechaHora.TextAlign = ContentAlignment.MiddleCenter;
            _pnlFooter.Controls.Add(_lblFechaHora);

            _timerReloj.Interval = 1000;
            _timerReloj.Tick += (s, e) => ActualizarFechaHora();

            Controls.Add(_pnlFooter);
            Controls.Add(pnlPantalla);
            Controls.Add(_pnlHeader);
        }

        protected void ConfigurarEncabezado(string titulo, string numeroCuentaEnmascarado)
        {
            _lblTitulo.Text = titulo;
            _lblCuenta.Text = "Cuenta: " + numeroCuentaEnmascarado;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ActualizarFechaHora();
            _timerReloj.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _timerReloj.Dispose();
            base.Dispose(disposing);
        }

        private void ActualizarFechaHora()
        {
            _lblFechaHora.Text = DateTime.Now.ToString("dddd dd/MM/yyyy   HH:mm:ss");
        }

        // ---------------------------------------------------------
        // Helpers de estilo para los controles de las pantallas hijas
        // ---------------------------------------------------------
        protected static void EstiloBoton(Button boton, string texto, Point ubicacion, Size tamano,
                                          Color colorBorde, float tamanoFuente = 11.5F)
        {
            boton.FlatStyle = FlatStyle.Flat;
            boton.BackColor = ColoresATM.FondoTecla;
            boton.ForeColor = Color.White;
            boton.Location = ubicacion;
            boton.Size = tamano;
            boton.UseVisualStyleBackColor = false;
            boton.FlatAppearance.BorderSize = 2;
            boton.FlatAppearance.BorderColor = colorBorde;
            boton.FlatAppearance.MouseOverBackColor = ColoresATM.FondoTeclaHover;
            boton.Font = new Font("Segoe UI", tamanoFuente, FontStyle.Bold);
            boton.Text = texto;
        }

        protected static void EstiloEtiqueta(Label etiqueta, string texto, Point ubicacion, Size tamano,
                                             Font fuente, Color color,
                                             ContentAlignment alineacion = ContentAlignment.MiddleLeft)
        {
            etiqueta.AutoSize = false;
            etiqueta.Text = texto;
            etiqueta.Location = ubicacion;
            etiqueta.Size = tamano;
            etiqueta.Font = fuente;
            etiqueta.ForeColor = color;
            etiqueta.TextAlign = alineacion;
        }

        protected static void EstiloCampo(TextBox campo, Point ubicacion, Size tamano, float tamanoFuente, int maxLength)
        {
            campo.BorderStyle = BorderStyle.FixedSingle;
            campo.BackColor = ColoresATM.FondoTecla;
            campo.ForeColor = ColoresATM.TextoPantalla;
            campo.Font = new Font("Consolas", tamanoFuente, FontStyle.Bold);
            campo.Location = ubicacion;
            campo.Size = tamano;
            campo.MaxLength = maxLength;
            campo.TextAlign = HorizontalAlignment.Right;
        }
    }
}
