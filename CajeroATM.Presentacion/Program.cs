using System;
using System.Windows.Forms;
using CajeroATM.Negocio;
using CajeroATM.Presentacion.Forms;

namespace CajeroATM.Presentacion
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Sin base de datos el cajero no puede operar: se verifica al arrancar.
            if (!ServicioDiagnostico.ProbarConexion(out string mensaje))
            {
                MessageBox.Show(
                    "No se pudo conectar a MySQL:\n" + mensaje +
                    "\n\nRevisá la cadena \"CajeroATM\" en App.config y que el servicio de MySQL esté iniciado.",
                    "Cajero Automático - Fuera de servicio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Red de seguridad: cualquier error no previsto se muestra en vez de cerrar la app.
            Application.ThreadException += (s, e) =>
                MessageBox.Show("Ocurrió un error inesperado:\n" + e.Exception.Message,
                    "Cajero Automático", MessageBoxButtons.OK, MessageBoxIcon.Error);

            Application.Run(new frmLogin());
        }
    }
}
