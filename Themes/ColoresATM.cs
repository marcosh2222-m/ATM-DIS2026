using System.Drawing;

namespace CajeroATM.Presentacion.Theme
{
    /// <summary>
    /// Paleta de colores compartida por todos los formularios, para que
    /// el look-and-feel sea consistente en toda la aplicación (login,
    /// menú, y las pantallas de operaciones que se sumen después).
    /// </summary>
    internal static class ColoresATM
    {
        // Estructura general del "gabinete" del cajero
        public static readonly Color FondoGabinete = Color.FromArgb(20, 28, 38);   // navy casi negro
        public static readonly Color FondoHeader = Color.FromArgb(13, 18, 25);   // más oscuro, para la barra superior
        public static readonly Color Acento = Color.FromArgb(0, 168, 150);  // verde-azulado, color "banco"
        public static readonly Color AcentoOscuro = Color.FromArgb(0, 120, 107);

        // La "pantalla" del cajero (LCD)
        public static readonly Color FondoPantalla = Color.Black;
        public static readonly Color TextoPantalla = Color.FromArgb(0, 230, 118);  // verde LCD clásico
        public static readonly Color TextoPantallaTenue = Color.FromArgb(0, 140, 72);

        // Teclado numérico
        public static readonly Color FondoTecla = Color.FromArgb(40, 52, 66);
        public static readonly Color FondoTeclaHover = Color.FromArgb(55, 70, 88);
        public static readonly Color TextoTecla = Color.White;

        // Estados / mensajes
        public static readonly Color Error = Color.FromArgb(255, 82, 82);
        public static readonly Color Advertencia = Color.FromArgb(255, 193, 7);
        public static readonly Color TextoFooter = Color.FromArgb(120, 130, 140);

        // Fuentes reutilizables
        public static readonly Font FuenteTitulo = new Font("Segoe UI", 13F, FontStyle.Bold);
        public static readonly Font FuentePantalla = new Font("Consolas", 14F, FontStyle.Regular);
        public static readonly Font FuentePin = new Font("Consolas", 22F, FontStyle.Bold);
        public static readonly Font FuenteTecla = new Font("Segoe UI", 14F, FontStyle.Bold);
        public static readonly Font FuenteBoton = new Font("Segoe UI", 11F, FontStyle.Bold);
        public static readonly Font FuenteFooter = new Font("Segoe UI", 8F, FontStyle.Regular);
    }
}
