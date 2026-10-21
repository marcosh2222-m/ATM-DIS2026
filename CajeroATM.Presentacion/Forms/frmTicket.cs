using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using CajeroATM.Negocio;

namespace CajeroATM.Presentacion.Forms
{
    /// <summary>
    /// P8 del DFD / CU "Emitir Ticket y Devolver Tarjeta": muestra el detalle
    /// de la sesión, permite imprimirlo (opcional) y termina con la
    /// devolución de la tarjeta. El menú cierra la sesión al volver.
    /// </summary>
    public partial class frmTicket : frmOperacionBase
    {
        private readonly List<string> _lineas;
        private int _lineaImpresa;

        public frmTicket(Sesion sesion)
        {
            InitializeComponent();
            if (sesion == null) throw new ArgumentNullException(nameof(sesion));

            ConfigurarEncabezado("FIN DE LA OPERACIÓN", sesion.Cuenta.ObtenerNumeroEnmascarado());

            // Se arma ahora: después de Sesion.Cerrar() las operaciones ya no existen.
            _lineas = Ticket.Generar(sesion);
            txtTicket.Text = string.Join(Environment.NewLine, _lineas);
            txtTicket.SelectionStart = 0;
        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            try
            {
                using (var documento = new PrintDocument())
                using (var vistaPrevia = new PrintPreviewDialog())
                {
                    documento.DocumentName = "Ticket Cajero Automático";
                    documento.BeginPrint += (s, args) => _lineaImpresa = 0;
                    documento.PrintPage += Documento_PrintPage;

                    // Vista previa: desde ahí se manda a la impresora (o a "Microsoft Print to PDF").
                    vistaPrevia.Document = documento;
                    vistaPrevia.Width = 600;
                    vistaPrevia.Height = 800;
                    vistaPrevia.ShowDialog(this);
                }
            }
            catch (Exception ex) when (ex is InvalidPrinterException || ex is System.ComponentModel.Win32Exception)
            {
                MessageBox.Show(this, "No hay una impresora disponible.\nPuede retirar su tarjeta igualmente.",
                    "Cajero Automático", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Documento_PrintPage(object sender, PrintPageEventArgs e)
        {
            using (var fuente = new Font("Consolas", 10F))
            {
                float alto = fuente.GetHeight(e.Graphics);
                float y = e.MarginBounds.Top;

                while (_lineaImpresa < _lineas.Count)
                {
                    if (y + alto > e.MarginBounds.Bottom)
                    {
                        e.HasMorePages = true;   // ticket muy largo: sigue en otra hoja
                        return;
                    }
                    e.Graphics.DrawString(_lineas[_lineaImpresa], fuente, Brushes.Black, e.MarginBounds.Left, y);
                    y += alto;
                    _lineaImpresa++;
                }
            }
            e.HasMorePages = false;
        }
    }
}
