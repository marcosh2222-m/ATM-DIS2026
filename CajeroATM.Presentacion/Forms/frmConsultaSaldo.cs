using System;
using System.Windows.Forms;
using CajeroATM.Negocio;
using CajeroATM.Negocio.Operaciones;
using CajeroATM.Presentacion.Theme;

namespace CajeroATM.Presentacion.Forms
{
    public partial class frmConsultaSaldo : frmOperacionBase
    {
        private readonly Sesion _sesion;

        public frmConsultaSaldo(Sesion sesion)
        {
            InitializeComponent();
            _sesion = sesion ?? throw new ArgumentNullException(nameof(sesion));
            ConfigurarEncabezado("CONSULTA DE SALDO", sesion.Cuenta.ObtenerNumeroEnmascarado());
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Consultar();   // al abrir la pantalla ya se muestra el saldo
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            Consultar();
        }

        private void Consultar()
        {
            // Cada consulta es una operación nueva y queda registrada (USP_ConsultarSaldo).
            var consulta = new Consulta(_sesion);

            Cursor = Cursors.WaitCursor;
            bool ok = consulta.Ejecutar();
            Cursor = Cursors.Default;
            OperacionRealizada = true;

            if (!ok)
            {
                MostrarError(MensajesOperacion.Traducir(consulta.CodigoResultado));
                return;
            }

            lblSaldo.Text = Formato.Moneda(consulta.SaldoConsultado);
            lblTipoCuenta.Text = "Tipo de cuenta : " + _sesion.Cuenta.TipoCuenta;
            lblEstadoCuenta.Text = "Estado         : " + _sesion.Cuenta.Estado;
            lblEstadoCuenta.ForeColor = _sesion.Cuenta.EstaActiva() ? ColoresATM.TextoPantalla : ColoresATM.Advertencia;
            lblFechaConsulta.Text = "Consulta       : " + consulta.FechaHora.ToString("dd/MM/yyyy HH:mm:ss");
            lblComprobante.Text = "Comprobante N° : " + consulta.IdComprobante;
            lblMensaje.Text = "";
        }

        private void MostrarError(string mensaje)
        {
            lblSaldo.Text = "—";
            lblTipoCuenta.Text = "";
            lblEstadoCuenta.Text = "";
            lblFechaConsulta.Text = "";
            lblComprobante.Text = "";
            lblMensaje.ForeColor = ColoresATM.Error;
            lblMensaje.Text = mensaje;
        }
    }
}
