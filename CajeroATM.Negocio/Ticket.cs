using System;
using System.Collections.Generic;
using System.Globalization;
using CajeroATM.Negocio.Operaciones;

namespace CajeroATM.Negocio
{
    /// <summary>
    /// Arma el texto del ticket de la sesión (DD: Ticket impreso = 1{Detalle de Operación}n).
    /// Ancho fijo pensado para fuente monoespaciada (pantalla e impresión).
    /// Se genera ANTES de Sesion.Cerrar(), que borra las operaciones.
    /// </summary>
    public static class Ticket
    {
        public const int ANCHO = 40;

        private static readonly CultureInfo EsAr = new CultureInfo("es-AR");

        public static List<string> Generar(Sesion sesion)
        {
            if (sesion == null) throw new ArgumentNullException(nameof(sesion));
            if (sesion.Cuenta == null)
                throw new InvalidOperationException("La sesión no tiene una cuenta cargada (¿ya se cerró?).");

            string doble = new string('=', ANCHO);
            string simple = new string('-', ANCHO);
            var l = new List<string>
            {
                doble,
                Centrar("CAJERO AUTOMÁTICO"),
                Centrar("Cajero N° " + sesion.IdCajero),
                doble,
                "Fecha   : " + DateTime.Now.ToString("dd/MM/yyyy HH:mm", EsAr),
                "Cliente : " + Recortar(sesion.Cliente.ObtenerNombreCompleto(), ANCHO - 10),
                "Cuenta  : " + sesion.Cuenta.ObtenerNumeroEnmascarado(),
                "Tarjeta : " + sesion.Tarjeta.ObtenerNumeroEnmascarado(),
                simple,
                "OPERACIONES DE LA SESIÓN",
                "",
            };

            if (sesion.Operaciones.Count == 0)
            {
                l.Add("  (sin operaciones realizadas)");
            }
            else
            {
                foreach (Operacion op in sesion.Operaciones)
                {
                    l.Add(op.FechaHora.ToString("dd/MM HH:mm", EsAr) + "  " + NombreVisible(op));

                    string izquierda = "  Comp. " + op.IdComprobante;
                    string derecha;
                    if (op is Consulta consulta)
                    {
                        derecha = "Saldo " + Moneda(consulta.SaldoConsultado);
                    }
                    else
                    {
                        derecha = Moneda(op.Monto);
                        if (op is Transferencia transferencia)
                            izquierda += " a " + transferencia.NumeroCuentaDestino;
                    }
                    l.Add(Columnas(izquierda, derecha));
                }
            }

            l.Add(simple);
            l.Add(Columnas("SALDO DISPONIBLE", Moneda(sesion.Cuenta.ConsultarSaldo())));
            l.Add(doble);
            l.Add(Centrar("Gracias por operar con nosotros."));
            l.Add(Centrar("Retire su tarjeta y su ticket."));
            l.Add(doble);
            return l;
        }

        private static string NombreVisible(Operacion op)
        {
            if (op is Consulta) return "Consulta de saldo";
            if (op is Extraccion) return "Extracción";
            if (op is Transferencia) return "Transferencia";
            return op.TipoTransaccion;
        }

        private static string Moneda(decimal importe)
        {
            return "$ " + importe.ToString("N2", EsAr);
        }

        private static string Centrar(string texto)
        {
            if (texto.Length >= ANCHO) return texto;
            int izquierda = (ANCHO - texto.Length) / 2;
            return new string(' ', izquierda) + texto;
        }

        /// <summary>Texto a la izquierda e importe alineado a la derecha.</summary>
        private static string Columnas(string izquierda, string derecha)
        {
            int espacio = ANCHO - izquierda.Length - derecha.Length;
            return espacio >= 1
                ? izquierda + new string(' ', espacio) + derecha
                : izquierda + " " + derecha;
        }

        private static string Recortar(string texto, int maximo)
        {
            return texto.Length <= maximo ? texto : texto.Substring(0, maximo - 1) + "…";
        }
    }
}
