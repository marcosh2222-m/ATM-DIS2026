using System;
using System.Collections.Generic;

namespace CajeroATM.Pruebas
{
    /// <summary>Mini corredor de pruebas: sin librerías externas, salida en consola.</summary>
    internal static class Corredor
    {
        private static int _ok;
        private static readonly List<string> _fallas = new List<string>();

        public static void Grupo(string nombre)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("== " + nombre + " ==");
            Console.ResetColor();
        }

        public static void Prueba(string nombre, Action cuerpo)
        {
            try
            {
                cuerpo();
                _ok++;
                Escribir(ConsoleColor.Green, "  OK     ", nombre);
            }
            catch (Exception ex)
            {
                _fallas.Add(nombre + ": " + ex.Message);
                Escribir(ConsoleColor.Red, "  FALLA  ", nombre + "\n           -> " + ex.Message);
            }
        }

        public static void Verificar(bool condicion, string detalle)
        {
            if (!condicion) throw new Exception(detalle);
        }

        public static void Igual<T>(T esperado, T obtenido, string que)
        {
            if (!Equals(esperado, obtenido))
                throw new Exception(que + ": se esperaba " + esperado + " y se obtuvo " + obtenido);
        }

        public static T Lanza<T>(Action accion) where T : Exception
        {
            try { accion(); }
            catch (T ex) { return ex; }
            throw new Exception("se esperaba una excepción " + typeof(T).Name + " y no se lanzó");
        }

        public static int Resumen()
        {
            Console.WriteLine();
            Console.WriteLine(new string('-', 60));
            Console.ForegroundColor = _fallas.Count == 0 ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine("  " + _ok + " correctas, " + _fallas.Count + " fallidas");
            Console.ResetColor();
            foreach (string f in _fallas) Console.WriteLine("  - " + f);
            return _fallas.Count;
        }

        private static void Escribir(ConsoleColor color, string etiqueta, string texto)
        {
            Console.ForegroundColor = color;
            Console.Write(etiqueta);
            Console.ResetColor();
            Console.WriteLine(texto);
        }
    }
}
