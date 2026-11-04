# ATM-DIS2026 — Sistema de Cajero Automático

Trabajo práctico integrador de **Diseño e Implementación de Sistemas (DIS)** —
ISFT N.º 93, Tecnicatura Superior en Análisis, Desarrollo y Programación de
Aplicaciones, 2026.

Simulación de un cajero automático bancario en **C# Windows Forms
(.NET Framework 4.8)** con **MySQL 8**, organizada en 3 capas. El Cliente se
identifica con su tarjeta y su PIN y puede:

- consultar el saldo,
- extraer efectivo (montos múltiplos de $100),
- transferir dinero a otra cuenta del banco,
- y al terminar recibir un ticket (en pantalla e impreso si lo pide).

Hay dos sistemas externos simulados sobre la misma base de datos:
**Sucursal**, que valida la tarjeta, y **Red Central**, que valida el PIN,
bloquea la tarjeta al tercer intento fallido y autoriza y registra las
operaciones.

## Estructura del repositorio

```
CajeroATM.sln
├── CajeroATM.Entidades     Clases del dominio: Cliente, Cuenta, Tarjeta
├── CajeroATM.Datos         Acceso a MySQL solo con stored procedures
│                           (SucursalDB = Sucursal, CentralDB = Red Central)
├── CajeroATM.Negocio       Sesion, operaciones (Consulta, Extraccion,
│                           Transferencia), hash del PIN, ticket, validaciones
├── CajeroATM.Presentacion  Formularios Windows Forms (proyecto de inicio)
├── CajeroATM.Pruebas       Pruebas de integración (aplicación de consola)
├── BaseDeDatos/            Scripts de MySQL
└── Documentacion/          Documento del TP (Word y PDF) y diagramas
```

Las dependencias van en un solo sentido: Presentación → Negocio → Datos
(y todas usan Entidades). La Presentación nunca accede a la base.

## Requisitos

- Visual Studio 2022, con el componente "Desarrollo de escritorio de .NET"
  y el paquete de destino de **.NET Framework 4.8**.
- MySQL Server 8.0 o superior, y MySQL Workbench o la consola `mysql`.
- Conexión a internet la primera vez, para que NuGet descargue
  **MySqlConnector 2.3.7**.

## Cómo crear la base de datos

Ejecutá los scripts de `BaseDeDatos/` con un usuario administrador
(por ejemplo `root`), **en este orden**:

| Orden | Script | Qué hace |
|---|---|---|
| 1 | `01_CrearBaseDatos_CajeroATM_MySQL.sql` | Crea la base `cajero_atm_db`, las 7 tablas y los datos de prueba |
| 2 | `02_StoredProcedures_CajeroATM_MySQL.sql` | Crea los 6 stored procedures |
| 3 | `00_CrearUsuario_MySQL.sql` | Crea el usuario `cajero_app`, que **solo** puede ejecutar los stored procedures |

Scripts opcionales:

- `03_PruebasSP_MySQL.sql`: 23 controles de los stored procedures. Corrélo
  sobre una base recién creada, porque modifica los datos.
- `04_VerificarConsistencia_MySQL.sql`: verifica que los saldos coincidan con
  el registro de transacciones. Se puede correr en cualquier momento.

Para dejar la base como nueva, borrala con `DROP DATABASE cajero_atm_db;` y
volvé a correr los scripts 1, 2 y 3. El script 01 no borra una base que ya
existe.

> ⚠️ **Contraseña:** `00_CrearUsuario_MySQL.sql` y el `App.config` traen la
> contraseña de ejemplo `CambiarEsta.2026`. El repositorio es público, así
> que cambiala en los dos lugares antes de usarlo en serio.

## Configuración

`CajeroATM.Presentacion/App.config`:

```xml
<connectionStrings>
  <add name="CajeroATM"
       connectionString="Server=localhost;Port=3306;Database=cajero_atm_db;User ID=cajero_app;Password=CambiarEsta.2026;" />
</connectionStrings>
<appSettings>
  <add key="IdCajero" value="1" />
</appSettings>
```

`CajeroATM.Pruebas/App.config` usa un usuario administrador (por defecto
`root`), porque las pruebas crean y borran sus propios datos. Reemplazá
`CAMBIAR_PASSWORD` por tu contraseña de MySQL.

## Cómo ejecutar

1. Abrí `CajeroATM.sln` en Visual Studio 2022.
2. Compilá la solución con *Compilar > Compilar solución*. NuGet restaura
   MySqlConnector solo.
3. Marcá **CajeroATM.Presentacion** como proyecto de inicio y presioná **F5**.

Si la aplicación no se puede conectar a la base, muestra un aviso de "fuera
de servicio" en lugar de abrir el cajero.

### Datos de prueba

| Tarjeta | PIN | Cuenta | Titular | Saldo inicial |
|---|---|---|---|---|
| 4000111122223333 | 1234 | 0001-0001-01 | Marcos Pérez | $ 50.000 |
| 4000444455556666 | 5678 | 0002-0002-02 | Laura Gómez | $ 15.000 |

La cuenta **0003-0003-03** está bloqueada: sirve para probar el rechazo de
una transferencia. Si ingresás tres PIN incorrectos seguidos, la tarjeta
queda bloqueada; para desbloquearla, volvé a crear la base.

## Pruebas

- **Base de datos:** `BaseDeDatos/03_PruebasSP_MySQL.sql`. Resultado
  esperado: `23 / 23 TODAS LAS PRUEBAS OK`.
- **Datos y Negocio integrados:** marcá **CajeroATM.Pruebas** como proyecto
  de inicio y ejecutalo. Resultado esperado: `26 correctas, 0 fallidas`.
  Incluye 10 extracciones simultáneas sobre la misma cuenta.

## Seguridad

- El PIN nunca se guarda ni viaja en texto plano. Se usa PBKDF2 con SHA-256,
  100.000 iteraciones y un salt aleatorio de 32 bytes por tarjeta, y a Red
  Central solo se envía el hash.
- La tarjeta se bloquea al tercer PIN incorrecto. El contador lo lleva la
  base de datos.
- Las extracciones y las transferencias usan transacciones con bloqueo de
  fila (`SELECT … FOR UPDATE`), así que dos operaciones simultáneas no pueden
  dejar un saldo inconsistente.
- La aplicación se conecta con un usuario que solo puede ejecutar stored
  procedures.

## Documentación

En [`Documentacion/`](Documentacion/) están el documento del TP
([Word](Documentacion/Documentacion_CajeroATM.docx) y
[PDF](Documentacion/Documentacion_CajeroATM.pdf)) y los diagramas en PNG y
SVG:

| Diagrama | Archivo |
|---|---|
| Diagrama de Contexto | `Diagramas/01_DiagramaContexto` |
| DFD Nivel 1 | `Diagramas/02_DFD_Nivel1` |
| Casos de Uso | `Diagramas/03_DiagramaCasosUso` |
| Clases | `Diagramas/04_DiagramaClases` |
| Secuencia — Autenticar Usuario | `Diagramas/05_Secuencia_AutenticarUsuario` |
| Secuencia — Extraer Efectivo | `Diagramas/06_Secuencia_ExtraerEfectivo` |
| Estado — Sesión | `Diagramas/07_DiagramaEstado_Sesion` |
| Actividad — Sistema completo | `Diagramas/08_DiagramaActividad_Sistema` |
| Entidad-Relación | `Diagramas/09_DiagramaEntidadRelacion` |

El Diccionario de Datos y las especificaciones de los casos de uso están
dentro del documento.

## Integrantes

- Marcos Murua  ([@marcosh2222-m](https://github.com/marcosh2222-m))
- Mateo D'ambrosio
