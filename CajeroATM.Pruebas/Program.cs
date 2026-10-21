using System;
using System.Linq;
using System.Threading.Tasks;
using CajeroATM.Datos;
using CajeroATM.Datos.Resultados;
using CajeroATM.Negocio;
using CajeroATM.Negocio.Operaciones;
using static CajeroATM.Pruebas.Corredor;
using D = CajeroATM.Pruebas.DatosDePrueba;

namespace CajeroATM.Pruebas
{
    /// <summary>
    /// Pruebas de integración del cajero contra la base MySQL de App.config.
    /// Uso: ejecutar este proyecto (clic derecho > Establecer como proyecto de
    /// inicio > F5, o "dotnet run"). Crea sus datos, prueba y los borra.
    /// El código de salida es la cantidad de pruebas fallidas.
    /// </summary>
    internal static class Program
    {
        private static int Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Pruebas de integración - Cajero Automático");

            if (!Conexion.ProbarConexion(out string mensaje))
            {
                Console.WriteLine("No se pudo conectar a MySQL: " + mensaje);
                Console.WriteLine("Revisá la cadena \"CajeroATM\" en el App.config de CajeroATM.Pruebas.");
                return 1;
            }
            Console.WriteLine(mensaje);

            try
            {
                D.Crear();
                PruebasDatos();
                PruebasNegocio();
                PruebasConcurrencia();
                PruebasErrores();
            }
            finally
            {
                D.Borrar();
            }

            return Resumen();
        }

        // =========================================================
        private static void PruebasDatos()
        {
            Grupo("Capa de Datos (SucursalDB / CentralDB)");
            var sucursal = new SucursalDB();
            var central = new CentralDB();

            Prueba("ObtenerDatosLogin trae la tarjeta y un salt de 32 bytes", () =>
            {
                DatosLogin datos = sucursal.ObtenerDatosLogin(D.TARJETA_OK);
                Verificar(datos != null, "no encontró la tarjeta");
                Igual(D.IdCuentaA, datos.Tarjeta.IdCuenta, "cuenta de la tarjeta");
                Igual(32, datos.Salt.Length, "largo del salt");
                Verificar(!datos.Tarjeta.EstaBloqueada() && !datos.Tarjeta.EstaVencida(), "la tarjeta debería estar activa");
            });

            Prueba("Tarjeta inexistente devuelve null", () =>
                Verificar(sucursal.ObtenerDatosLogin("1234567890123456") == null, "no devolvió null"));

            Prueba("ValidarLogin con el hash PBKDF2 correcto -> Ok", () =>
            {
                DatosLogin datos = sucursal.ObtenerDatosLogin(D.TARJETA_OK);
                ResultadoLogin r = sucursal.ValidarLogin(datos.Tarjeta.IdTarjeta, SeguridadPin.CalcularHash(D.PIN_OK, datos.Salt));
                Igual(CodigoLogin.Ok, r.Codigo, "código");
                Igual((int?)D.IdCuentaA, r.IdCuenta, "IdCuenta");
            });

            Prueba("ObtenerDatosCuenta trae cuenta y titular", () =>
            {
                DatosCuenta dc = central.ObtenerDatosCuenta(D.IdCuentaA);
                Igual("Prueba Automática", dc.Cliente.ObtenerNombreCompleto(), "titular");
                Igual(D.CUENTA_A, dc.Cuenta.NumeroCuenta, "número de cuenta");
            });

            int idTarjeta = sucursal.ObtenerDatosLogin(D.TARJETA_OK).Tarjeta.IdTarjeta;

            Prueba("ConsultarSaldo devuelve el saldo y un comprobante", () =>
            {
                ResultadoConsulta r = central.ConsultarSaldo(D.IdCuentaA, idTarjeta, 1);
                Igual(CodigoOperacion.Ok, r.Codigo, "código");
                Igual(D.SALDO_INICIAL_A, r.Cuenta.ConsultarSaldo(), "saldo");
                Verificar(r.IdTransaccion > 0, "sin comprobante");
            });

            Prueba("Extraer $ 1.000 descuenta el saldo", () =>
            {
                ResultadoMovimiento r = central.Extraer(D.IdCuentaA, idTarjeta, 1, 1000m);
                Igual(CodigoOperacion.Ok, r.Codigo, "código");
                Igual((decimal?)99000m, r.SaldoResultante, "saldo resultante");
                Igual(99000m, D.SaldoEnBase(D.CUENTA_A), "saldo en la base");
            });

            Prueba("Extraer $ 150 -> MontoInvalido", () =>
                Igual(CodigoOperacion.MontoInvalido, central.Extraer(D.IdCuentaA, idTarjeta, 1, 150m).Codigo, "código"));

            Prueba("Extraer más que el saldo -> SaldoInsuficiente (sin tocar el saldo)", () =>
            {
                ResultadoMovimiento r = central.Extraer(D.IdCuentaA, idTarjeta, 1, 500000m);
                Igual(CodigoOperacion.SaldoInsuficiente, r.Codigo, "código");
                Igual(99000m, D.SaldoEnBase(D.CUENTA_A), "saldo en la base");
            });

            Prueba("Transferir $ 2.000 mueve el dinero entre las dos cuentas", () =>
            {
                ResultadoMovimiento r = central.Transferir(D.IdCuentaA, D.CUENTA_B, idTarjeta, 1, 2000m);
                Igual(CodigoOperacion.Ok, r.Codigo, "código");
                Igual(97000m, D.SaldoEnBase(D.CUENTA_A), "saldo origen");
                Igual(2000m, D.SaldoEnBase(D.CUENTA_B), "saldo destino");
            });

            Prueba("Transferir a cuenta bloqueada -> CuentaDestinoInvalida", () =>
                Igual(CodigoOperacion.CuentaDestinoInvalida, central.Transferir(D.IdCuentaA, D.CUENTA_C, idTarjeta, 1, 100m).Codigo, "código"));

            Prueba("Transferir a la misma cuenta -> MismaCuenta", () =>
                Igual(CodigoOperacion.MismaCuenta, central.Transferir(D.IdCuentaA, D.CUENTA_A, idTarjeta, 1, 100m).Codigo, "código"));
        }

        // =========================================================
        private static void PruebasNegocio()
        {
            Grupo("Capa de Negocio (Sesion / Operaciones / Ticket)");
            var sesion = new Sesion();   // usa App.config, igual que la aplicación

            Prueba("Login completo: tarjeta, PIN con letras (no cuenta) y PIN correcto", () =>
            {
                Verificar(sesion.IniciarSesion(D.TARJETA_OK), "rechazó la tarjeta: " + sesion.UltimoResultado);
                Verificar(!sesion.Autenticar("43a1"), "aceptó un PIN con letras");
                Igual((ResultadoAutenticacion?)ResultadoAutenticacion.PinInvalido, sesion.UltimoResultado, "motivo");
                Igual(3, sesion.IntentosRestantes, "intentos restantes");
                Verificar(sesion.Autenticar(D.PIN_OK), "rechazó el PIN correcto: " + sesion.UltimoResultado);
                Igual(Sesion.OPERANDO, sesion.Estado, "estado");
                Igual("Prueba Automática", sesion.Cliente.ObtenerNombreCompleto(), "titular");
            });

            Prueba("Consulta de saldo", () =>
            {
                var c = new Consulta(sesion);
                Verificar(c.Ejecutar(), "falló: " + c.CodigoResultado);
                Igual(97000m, c.SaldoConsultado, "saldo");
            });

            Prueba("Extracción de $ 1.000 (10 billetes) y no se puede ejecutar dos veces", () =>
            {
                var e = new Extraccion(sesion, 1000m);
                Verificar(e.Ejecutar(), "falló: " + e.CodigoResultado);
                Igual(10, e.BilletesEntregados, "billetes");
                Igual(96000m, sesion.Cuenta.ConsultarSaldo(), "saldo en memoria");
                Lanza<InvalidOperationException>(() => e.Ejecutar());
            });

            Prueba("Extracción de $ 150 rechazada en Negocio", () =>
            {
                var e = new Extraccion(sesion, 150m);
                Verificar(!e.Ejecutar(), "la aceptó");
                Igual("MONTO_INVALIDO", e.CodigoResultado, "código");
            });

            Prueba("Transferencia escribiendo la cuenta sin guiones", () =>
            {
                var t = new Transferencia(sesion, D.CUENTA_B.Replace("-", ""), 1000m);
                Verificar(t.Ejecutar(), "falló: " + t.CodigoResultado);
                Igual(95000m, t.SaldoResultante.Value, "saldo");
            });

            Prueba("Transferencias rechazadas: misma cuenta y destino bloqueado", () =>
            {
                var misma = new Transferencia(sesion, D.CUENTA_A, 100m);
                Verificar(!misma.Ejecutar(), "aceptó la misma cuenta");
                Igual("MISMA_CUENTA", misma.CodigoResultado, "código misma cuenta");
                var bloqueada = new Transferencia(sesion, D.CUENTA_C, 100m);
                Verificar(!bloqueada.Ejecutar(), "aceptó destino bloqueado");
                Igual("CUENTA_DESTINO_INVALIDA", bloqueada.CodigoResultado, "código destino bloqueado");
            });

            Prueba("La sesión registra solo las 3 operaciones exitosas", () =>
                Igual(3, sesion.Operaciones.Count, "operaciones"));

            Prueba("Ticket: ancho fijo, todas las operaciones y saldo final", () =>
            {
                var lineas = Ticket.Generar(sesion);
                Verificar(lineas.All(l => l.Length <= Ticket.ANCHO), "hay líneas más anchas que " + Ticket.ANCHO);
                string texto = string.Join("\n", lineas);
                Verificar(texto.Contains("Extracción") && texto.Contains("Transferencia") && texto.Contains("Consulta"), "faltan operaciones");
                Verificar(texto.Contains("$ 95.000,00"), "no muestra el saldo final $ 95.000,00");
            });

            Prueba("El saldo de la base coincide con el último movimiento registrado", () =>
                Igual(D.SaldoEnBase(D.CUENTA_A), D.SaldoDelUltimoMovimiento(D.CUENTA_A), "saldo vs log"));

            Prueba("Cerrar borra los datos y no permite seguir operando", () =>
            {
                sesion.Cerrar();
                Igual(Sesion.CERRADA, sesion.Estado, "estado");
                Verificar(sesion.Cuenta == null && sesion.Tarjeta == null && sesion.Operaciones.Count == 0, "quedaron datos en memoria");
                Lanza<InvalidOperationException>(() => new Extraccion(sesion, 100m));
            });

            Prueba("Tres PIN incorrectos bloquean la tarjeta", () =>
            {
                var s = new Sesion();
                Verificar(s.IniciarSesion(D.TARJETA_BLOQUEO), "rechazó la tarjeta");
                Verificar(!s.Autenticar("0000") && s.IntentosRestantes == 2, "intento 1");
                Verificar(!s.Autenticar("0000") && s.IntentosRestantes == 1, "intento 2");
                Verificar(!s.Autenticar("0000"), "intento 3");
                Igual((ResultadoAutenticacion?)ResultadoAutenticacion.TarjetaBloqueada, s.UltimoResultado, "motivo");
                Igual(Sesion.CERRADA, s.Estado, "estado");

                var otra = new Sesion();
                Verificar(!otra.IniciarSesion(D.TARJETA_BLOQUEO), "una nueva sesión aceptó la tarjeta bloqueada");
            });

            Prueba("Tarjeta vencida e inexistente se rechazan en el primer paso", () =>
            {
                var v = new Sesion();
                Verificar(!v.IniciarSesion(D.TARJETA_VENCIDA), "aceptó la vencida");
                Igual((ResultadoAutenticacion?)ResultadoAutenticacion.TarjetaVencida, v.UltimoResultado, "motivo vencida");
                var i = new Sesion();
                Verificar(!i.IniciarSesion("1234567890123456"), "aceptó la inexistente");
                Igual((ResultadoAutenticacion?)ResultadoAutenticacion.TarjetaInexistente, i.UltimoResultado, "motivo inexistente");
            });
        }

        // =========================================================
        private static void PruebasConcurrencia()
        {
            Grupo("Concurrencia");

            Prueba("10 extracciones simultáneas de $ 10.000 sobre $ 95.000: salen 9, saldo final $ 5.000", () =>
            {
                int idTarjeta = new SucursalDB().ObtenerDatosLogin(D.TARJETA_OK).Tarjeta.IdTarjeta;
                Task<ResultadoMovimiento>[] tareas = Enumerable.Range(0, 10)
                    .Select(_ => Task.Run(() => new CentralDB().Extraer(D.IdCuentaA, idTarjeta, 1, 10000m)))
                    .ToArray();
                Task.WaitAll(tareas);

                Igual(9, tareas.Count(t => t.Result.EsOk), "extracciones exitosas");
                Igual(5000m, D.SaldoEnBase(D.CUENTA_A), "saldo final");
            });
        }

        // =========================================================
        private static void PruebasErrores()
        {
            Grupo("Errores técnicos");
            string cadenaMala = "Server=localhost;Database=cajero_atm_db;User ID=no_existe;Password=mal;";

            Prueba("Credenciales incorrectas -> DatosException con mensaje en castellano", () =>
            {
                DatosException ex = Lanza<DatosException>(() => new SucursalDB(cadenaMala).ObtenerDatosLogin(D.TARJETA_OK));
                Verificar(ex.Message.Contains("contraseña"), "mensaje inesperado: " + ex.Message);
            });

            Prueba("Falla de la base durante el login -> CajeroException (sin tipos de Datos)", () =>
            {
                var s = new Sesion(new SucursalDB(cadenaMala), new CentralDB(cadenaMala), 1);
                Lanza<CajeroException>(() => s.IniciarSesion(D.TARJETA_OK));
            });
        }
    }
}
