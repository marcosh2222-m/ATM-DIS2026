using CajeroATM.Datos;

namespace CajeroATM.Negocio
{
    /// <summary>
    /// Le permite a Presentación verificar la conexión a la base sin
    /// referenciar a la capa de Datos (Presentación → Negocio → Datos).
    /// </summary>
    public static class ServicioDiagnostico
    {
        public static bool ProbarConexion(out string mensaje)
        {
            return Conexion.ProbarConexion(out mensaje);
        }
    }
}
