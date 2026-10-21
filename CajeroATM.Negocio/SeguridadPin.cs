using System;
using System.Security.Cryptography;
using System.Text;

namespace CajeroATM.Negocio
{
    /// <summary>
    /// Hash del PIN con PBKDF2 (salt individual por tarjeta).
    /// Los parámetros tienen que coincidir con los que se usaron para
    /// guardar el PIN_Hash (ver datos de prueba en el script 01).
    /// </summary>
    public static class SeguridadPin
    {
        public const int ITERACIONES = 100000;
        public const int LONGITUD_HASH = 32;     // bytes (VARBINARY(64) en la tabla alcanza)
        public const int LONGITUD_SALT = 32;     // bytes (columna Salt VARBINARY(32))
        public static readonly HashAlgorithmName ALGORITMO = HashAlgorithmName.SHA256;

        public static byte[] CalcularHash(string pin, byte[] salt)
        {
            if (pin == null) throw new ArgumentNullException(nameof(pin));
            if (salt == null || salt.Length == 0) throw new ArgumentException("Falta el salt de la tarjeta.", nameof(salt));

            using (var pbkdf2 = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(pin), salt, ITERACIONES, ALGORITMO))
            {
                return pbkdf2.GetBytes(LONGITUD_HASH);
            }
        }

        /// <summary>Salt aleatorio para dar de alta una tarjeta nueva.</summary>
        public static byte[] GenerarSalt()
        {
            var salt = new byte[LONGITUD_SALT];
            using (var generador = RandomNumberGenerator.Create())
            {
                generador.GetBytes(salt);
            }
            return salt;
        }
    }
}
