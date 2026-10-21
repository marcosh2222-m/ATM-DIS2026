using System;
using System.Collections.Generic;

namespace CajeroATM.Entidades
{
    /// <summary>
    /// Titular de una o más cuentas (tabla Cliente).
    /// Diagrama de Clases: Cliente 1 ── es titular de ── 1..* Cuenta.
    /// </summary>
    public class Cliente
    {
        // Atributos (privados, como en el diagrama: "-")
        private readonly int _idCliente;
        private readonly string _nombre;
        private readonly string _apellido;
        private readonly string _dni;
        private readonly List<Cuenta> _cuentas = new List<Cuenta>();

        public Cliente(int idCliente, string nombre, string apellido, string dni)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
            if (string.IsNullOrWhiteSpace(apellido)) throw new ArgumentException("El apellido es obligatorio.", nameof(apellido));

            _idCliente = idCliente;
            _nombre = nombre.Trim();
            _apellido = apellido.Trim();
            _dni = dni;
        }

        // Propiedades de solo lectura: es la forma de C# de exponer un
        // atributo privado sin permitir que se modifique desde afuera.
        public int IdCliente => _idCliente;
        public string Nombre => _nombre;
        public string Apellido => _apellido;
        public string Dni => _dni;
        public IReadOnlyList<Cuenta> Cuentas => _cuentas;

        // +ObtenerNombreCompleto(): string
        public string ObtenerNombreCompleto()
        {
            return _nombre + " " + _apellido;
        }

        public void AgregarCuenta(Cuenta cuenta)
        {
            if (cuenta == null) throw new ArgumentNullException(nameof(cuenta));
            _cuentas.Add(cuenta);
        }
    }
}
