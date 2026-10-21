-- =========================================================
-- PROYECTO: Cajero Automático (3 capas - .NET Forms)
-- Motor: MySQL 8.0+
-- Script: Stored Procedures
-- Requiere haber corrido 01_CrearBaseDatos_CajeroATM_MySQL.sql
--
-- v2 (Etapa 2 - capa de Datos):
-- - USP_ObtenerDatosLogin devuelve también IdCuenta y NumeroTarjeta
-- - USP_ConsultarSaldo devuelve IdTransaccion (número de comprobante)
-- - USP_Extraccion y USP_Transferencia: nuevo OUT p_IdTransaccion
-- - USP_Transferencia registra también el rechazo por cuenta destino
--   inactiva (antes era el único rechazo que no quedaba en el log)
-- - Nuevo USP_ObtenerDatosCuenta (datos de cuenta + titular para el
--   menú, sin registrar una consulta de saldo)
--
-- CAMBIOS RESPECTO A LA VERSIÓN SQL SERVER:
-- - CREATE OR ALTER PROCEDURE no existe en MySQL -> se usa
--   DROP PROCEDURE IF EXISTS + CREATE PROCEDURE
-- - Los parámetros de salida se declaran como OUT dentro de
--   la firma, no con la palabra OUTPUT al final
-- - WITH (UPDLOCK, ROWLOCK) de SQL Server -> SELECT ... FOR UPDATE
-- - No hay TRY/CATCH: se usa DECLARE ... HANDLER FOR SQLEXCEPTION
-- - THROW -> RESIGNAL (deja que el error real siga explotando
--   hacia C#, igual que el THROW original)
-- - RAISERROR -> SIGNAL SQLSTATE
-- - Como no hay RETURN anticipado dentro de un BEGIN normal (sin
--   bloque etiquetado), toda la lógica de "cortar acá" se resolvió
--   con cadenas IF / ELSEIF / ELSE en vez de RETURN, para que solo
--   se ejecute una rama y no haga falta LEAVE con label
-- - DELIMITER $$ es necesario porque el cuerpo del procedure usa
--   ";" internamente; sin cambiar el delimitador, MySQL cortaría
--   el CREATE PROCEDURE en el primer ";" que encuentre
-- =========================================================

SET NAMES utf8mb4;
USE cajero_atm_db;

DELIMITER $$

-- =========================================================
-- 1. USP_ObtenerDatosLogin
-- Actor: SUCURSAL (flujo raíz de validación).
-- Paso 1 del login: trae el Salt de la tarjeta para que la
-- capa de Negocio calcule el hash del PIN ingresado (PBKDF2)
-- ANTES de mandarlo a validar. La BD nunca recibe el PIN en
-- texto plano.
-- =========================================================
DROP PROCEDURE IF EXISTS USP_ObtenerDatosLogin $$
CREATE PROCEDURE USP_ObtenerDatosLogin (
    IN p_NumeroTarjeta VARCHAR(20)
)
BEGIN
    SELECT
        IdTarjeta,
        IdCuenta,
        NumeroTarjeta,
        Salt,
        Estado,
        IntentosFallidos,
        FechaVencimiento
    FROM Tarjeta
    WHERE NumeroTarjeta = p_NumeroTarjeta;
END $$

-- =========================================================
-- 2. USP_ValidarLogin
-- Actor: SUCURSAL.
-- Paso 2 del login: recibe el hash YA calculado en C# y lo
-- compara. Maneja intentos fallidos y bloqueo automático.
--
-- p_Resultado posibles: OK | PIN_INCORRECTO | BLOQUEADA |
--                        VENCIDA | ANULADA | NO_EXISTE
-- =========================================================
DROP PROCEDURE IF EXISTS USP_ValidarLogin $$
CREATE PROCEDURE USP_ValidarLogin (
    IN  p_IdTarjeta         INT,
    IN  p_PINHashCalculado  VARBINARY(64),
    OUT p_Resultado         VARCHAR(30),
    OUT p_IdCuenta          INT
)
BEGIN
    DECLARE v_PINHashGuardado VARBINARY(64);
    DECLARE v_Estado          VARCHAR(20);
    DECLARE v_Intentos        TINYINT;
    DECLARE v_Vencimiento     DATE;
    DECLARE v_IdCuentaTarjeta INT;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

    -- FOR UPDATE: bloquea la fila mientras dura la validación,
    -- así dos intentos simultáneos con la misma tarjeta no pisan
    -- el conteo de IntentosFallidos entre sí.
    SELECT PIN_Hash, Estado, IntentosFallidos, FechaVencimiento, IdCuenta
      INTO v_PINHashGuardado, v_Estado, v_Intentos, v_Vencimiento, v_IdCuentaTarjeta
    FROM Tarjeta
    WHERE IdTarjeta = p_IdTarjeta
    FOR UPDATE;

    IF v_Estado IS NULL THEN
        ROLLBACK;
        SET p_Resultado = 'NO_EXISTE';

    ELSEIF v_Estado = 'Bloqueada' THEN
        ROLLBACK;
        SET p_Resultado = 'BLOQUEADA';

    ELSEIF v_Estado = 'Anulada' THEN
        ROLLBACK;
        SET p_Resultado = 'ANULADA';

    ELSEIF v_Vencimiento < CURDATE() THEN
        UPDATE Tarjeta SET Estado = 'Vencida' WHERE IdTarjeta = p_IdTarjeta;
        COMMIT;
        SET p_Resultado = 'VENCIDA';

    ELSEIF v_PINHashGuardado = p_PINHashCalculado THEN
        -- Login correcto: resetea contador de intentos
        UPDATE Tarjeta SET IntentosFallidos = 0 WHERE IdTarjeta = p_IdTarjeta;
        COMMIT;
        SET p_Resultado = 'OK';
        SET p_IdCuenta = v_IdCuentaTarjeta;

    ELSE
        -- PIN incorrecto: suma intento. Al 3ro, bloquea la tarjeta.
        SET v_Intentos = v_Intentos + 1;

        IF v_Intentos >= 3 THEN
            UPDATE Tarjeta
               SET IntentosFallidos = v_Intentos, Estado = 'Bloqueada'
             WHERE IdTarjeta = p_IdTarjeta;
            COMMIT;
            SET p_Resultado = 'BLOQUEADA';
        ELSE
            UPDATE Tarjeta SET IntentosFallidos = v_Intentos WHERE IdTarjeta = p_IdTarjeta;
            COMMIT;
            SET p_Resultado = 'PIN_INCORRECTO';
        END IF;
    END IF;
END $$

-- =========================================================
-- 3. USP_ConsultarSaldo
-- Actor: RED CENTRAL.
-- Trae el saldo actual y deja registro de la consulta
-- (Monto = 0, sirve como log de auditoría de accesos).
-- =========================================================
DROP PROCEDURE IF EXISTS USP_ConsultarSaldo $$
CREATE PROCEDURE USP_ConsultarSaldo (
    IN p_IdCuenta  INT,
    IN p_IdTarjeta INT,
    IN p_IdCajero  INT
)
BEGIN
    DECLARE v_IdTipoConsulta INT;
    DECLARE v_Saldo DECIMAL(14,2);

    SELECT IdTipoTransaccion INTO v_IdTipoConsulta
    FROM TipoTransaccion WHERE Nombre = 'Consulta de Saldo';

    SELECT Saldo INTO v_Saldo FROM Cuenta WHERE IdCuenta = p_IdCuenta;

    IF v_Saldo IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La cuenta no existe.';
    END IF;

    INSERT INTO Transaccion
        (IdTarjeta, IdCajero, IdTipoTransaccion, IdCuentaOrigen, IdCuentaDestino, Monto, SaldoResultante, Estado)
    VALUES
        (p_IdTarjeta, p_IdCajero, v_IdTipoConsulta, p_IdCuenta, NULL, 0, v_Saldo, 'Exitosa');

    SELECT
        IdCuenta,
        NumeroCuenta,
        Saldo,
        TipoCuenta,
        Estado,
        LAST_INSERT_ID() AS IdTransaccion   -- comprobante de la consulta
    FROM Cuenta
    WHERE IdCuenta = p_IdCuenta;
END $$

-- =========================================================
-- 3b. USP_ObtenerDatosCuenta
-- Actor: RED CENTRAL.
-- Datos de la cuenta y su titular, para armar la sesión después
-- del login (nombre en el menú, número enmascarado). A diferencia
-- de USP_ConsultarSaldo, NO registra una transacción.
-- =========================================================
DROP PROCEDURE IF EXISTS USP_ObtenerDatosCuenta $$
CREATE PROCEDURE USP_ObtenerDatosCuenta (
    IN p_IdCuenta INT
)
BEGIN
    SELECT
        cu.IdCuenta,
        cu.NumeroCuenta,
        cu.Saldo,
        cu.TipoCuenta,
        cu.Estado,
        cl.IdCliente,
        cl.Nombre,
        cl.Apellido,
        cl.DNI
    FROM Cuenta cu
    INNER JOIN Cliente cl ON cl.IdCliente = cu.IdCliente
    WHERE cu.IdCuenta = p_IdCuenta;
END $$

-- =========================================================
-- 4. USP_Extraccion
-- Actor: RED CENTRAL (valida saldo/estado y autoriza).
--
-- p_Resultado posibles: OK | MONTO_INVALIDO | CUENTA_NO_EXISTE
--                        | CUENTA_BLOQUEADA | SALDO_INSUFICIENTE
-- =========================================================
DROP PROCEDURE IF EXISTS USP_Extraccion $$
CREATE PROCEDURE USP_Extraccion (
    IN  p_IdCuenta         INT,
    IN  p_IdTarjeta        INT,
    IN  p_IdCajero         INT,
    IN  p_Monto            DECIMAL(14,2),
    OUT p_Resultado        VARCHAR(30),
    OUT p_SaldoResultante  DECIMAL(14,2),
    OUT p_IdTransaccion    INT
)
BEGIN
    DECLARE v_IdTipoExtraccion INT;
    DECLARE v_SaldoActual DECIMAL(14,2);
    DECLARE v_EstadoCuenta VARCHAR(20);

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    SET p_IdTransaccion = NULL;

    SELECT IdTipoTransaccion INTO v_IdTipoExtraccion
    FROM TipoTransaccion WHERE Nombre = 'Extraccion';

    -- Regla de negocio de ejemplo: el cajero solo entrega billetes
    -- de $100. Ajustar según denominaciones reales.
    IF p_Monto <= 0 OR MOD(p_Monto, 100) <> 0 THEN
        SET p_Resultado = 'MONTO_INVALIDO';

    ELSE
        START TRANSACTION;

        -- FOR UPDATE: evita que dos extracciones simultáneas de la
        -- misma cuenta lean el mismo saldo "viejo" y ambas pasen la
        -- validación (condición de carrera clásica de ATMs).
        SELECT Saldo, Estado INTO v_SaldoActual, v_EstadoCuenta
        FROM Cuenta
        WHERE IdCuenta = p_IdCuenta
        FOR UPDATE;

        IF v_EstadoCuenta IS NULL THEN
            ROLLBACK;
            SET p_Resultado = 'CUENTA_NO_EXISTE';

        ELSEIF v_EstadoCuenta <> 'Activa' THEN
            ROLLBACK;

            INSERT INTO Transaccion
                (IdTarjeta, IdCajero, IdTipoTransaccion, IdCuentaOrigen, IdCuentaDestino, Monto, SaldoResultante, Estado)
            VALUES
                (p_IdTarjeta, p_IdCajero, v_IdTipoExtraccion, p_IdCuenta, NULL, p_Monto, v_SaldoActual, 'Rechazada');
            SET p_IdTransaccion = LAST_INSERT_ID();

            SET p_Resultado = 'CUENTA_BLOQUEADA';
            SET p_SaldoResultante = v_SaldoActual;

        ELSEIF v_SaldoActual < p_Monto THEN
            ROLLBACK;

            INSERT INTO Transaccion
                (IdTarjeta, IdCajero, IdTipoTransaccion, IdCuentaOrigen, IdCuentaDestino, Monto, SaldoResultante, Estado)
            VALUES
                (p_IdTarjeta, p_IdCajero, v_IdTipoExtraccion, p_IdCuenta, NULL, p_Monto, v_SaldoActual, 'Rechazada');
            SET p_IdTransaccion = LAST_INSERT_ID();

            SET p_Resultado = 'SALDO_INSUFICIENTE';
            SET p_SaldoResultante = v_SaldoActual;

        ELSE
            UPDATE Cuenta SET Saldo = Saldo - p_Monto WHERE IdCuenta = p_IdCuenta;
            SET p_SaldoResultante = v_SaldoActual - p_Monto;

            INSERT INTO Transaccion
                (IdTarjeta, IdCajero, IdTipoTransaccion, IdCuentaOrigen, IdCuentaDestino, Monto, SaldoResultante, Estado)
            VALUES
                (p_IdTarjeta, p_IdCajero, v_IdTipoExtraccion, p_IdCuenta, NULL, p_Monto, p_SaldoResultante, 'Exitosa');
            SET p_IdTransaccion = LAST_INSERT_ID();

            COMMIT;
            SET p_Resultado = 'OK';
        END IF;
    END IF;
END $$

-- =========================================================
-- 5. USP_Transferencia
-- Actor: RED CENTRAL (valida ambas cuentas y autoriza).
--
-- Recibe el NÚMERO de cuenta destino (lo que tipearía el
-- usuario), no el Id interno.
--
-- p_Resultado posibles: OK | MONTO_INVALIDO | CUENTA_BLOQUEADA
--                        | SALDO_INSUFICIENTE
--                        | CUENTA_DESTINO_INVALIDA | MISMA_CUENTA
-- =========================================================
DROP PROCEDURE IF EXISTS USP_Transferencia $$
CREATE PROCEDURE USP_Transferencia (
    IN  p_IdCuentaOrigen       INT,
    IN  p_NumeroCuentaDestino  VARCHAR(20),
    IN  p_IdTarjeta            INT,
    IN  p_IdCajero             INT,
    IN  p_Monto                DECIMAL(14,2),
    OUT p_Resultado            VARCHAR(30),
    OUT p_SaldoResultante      DECIMAL(14,2),
    OUT p_IdTransaccion        INT
)
BEGIN
    DECLARE v_IdTipoTransferencia INT;
    DECLARE v_IdCuentaDestino INT;
    DECLARE v_SaldoOrigen DECIMAL(14,2);
    DECLARE v_EstadoOrigen VARCHAR(20);
    DECLARE v_SaldoDestino DECIMAL(14,2);
    DECLARE v_EstadoDestino VARCHAR(20);

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    SET p_IdTransaccion = NULL;

    SELECT IdTipoTransaccion INTO v_IdTipoTransferencia
    FROM TipoTransaccion WHERE Nombre = 'Transferencia';

    -- Resolver la cuenta destino ANTES de abrir la transacción: es
    -- una simple lectura y todavía no necesita lock.
    SELECT IdCuenta INTO v_IdCuentaDestino
    FROM Cuenta WHERE NumeroCuenta = p_NumeroCuentaDestino;

    IF p_Monto <= 0 THEN
        SET p_Resultado = 'MONTO_INVALIDO';

    ELSEIF v_IdCuentaDestino IS NULL THEN
        SET p_Resultado = 'CUENTA_DESTINO_INVALIDA';

    ELSEIF v_IdCuentaDestino = p_IdCuentaOrigen THEN
        SET p_Resultado = 'MISMA_CUENTA';

    ELSE
        START TRANSACTION;

        -- Se bloquean ambas cuentas SIEMPRE en el mismo orden (por
        -- IdCuenta ascendente), sin importar cuál es origen y cuál
        -- destino. Si dos transferencias cruzadas ocurren al mismo
        -- tiempo (A->B y B->A), tomar los locks en orden consistente
        -- evita el deadlock clásico.
        IF p_IdCuentaOrigen < v_IdCuentaDestino THEN
            SELECT Saldo, Estado INTO v_SaldoOrigen, v_EstadoOrigen
            FROM Cuenta WHERE IdCuenta = p_IdCuentaOrigen FOR UPDATE;

            SELECT Saldo, Estado INTO v_SaldoDestino, v_EstadoDestino
            FROM Cuenta WHERE IdCuenta = v_IdCuentaDestino FOR UPDATE;
        ELSE
            SELECT Saldo, Estado INTO v_SaldoDestino, v_EstadoDestino
            FROM Cuenta WHERE IdCuenta = v_IdCuentaDestino FOR UPDATE;

            SELECT Saldo, Estado INTO v_SaldoOrigen, v_EstadoOrigen
            FROM Cuenta WHERE IdCuenta = p_IdCuentaOrigen FOR UPDATE;
        END IF;

        IF v_EstadoOrigen <> 'Activa' THEN
            ROLLBACK;

            INSERT INTO Transaccion
                (IdTarjeta, IdCajero, IdTipoTransaccion, IdCuentaOrigen, IdCuentaDestino, Monto, SaldoResultante, Estado)
            VALUES
                (p_IdTarjeta, p_IdCajero, v_IdTipoTransferencia, p_IdCuentaOrigen, v_IdCuentaDestino, p_Monto, v_SaldoOrigen, 'Rechazada');
            SET p_IdTransaccion = LAST_INSERT_ID();

            SET p_Resultado = 'CUENTA_BLOQUEADA';
            SET p_SaldoResultante = v_SaldoOrigen;

        ELSEIF v_EstadoDestino <> 'Activa' THEN
            ROLLBACK;

            INSERT INTO Transaccion
                (IdTarjeta, IdCajero, IdTipoTransaccion, IdCuentaOrigen, IdCuentaDestino, Monto, SaldoResultante, Estado)
            VALUES
                (p_IdTarjeta, p_IdCajero, v_IdTipoTransferencia, p_IdCuentaOrigen, v_IdCuentaDestino, p_Monto, v_SaldoOrigen, 'Rechazada');
            SET p_IdTransaccion = LAST_INSERT_ID();

            SET p_Resultado = 'CUENTA_DESTINO_INVALIDA';
            SET p_SaldoResultante = v_SaldoOrigen;

        ELSEIF v_SaldoOrigen < p_Monto THEN
            ROLLBACK;

            INSERT INTO Transaccion
                (IdTarjeta, IdCajero, IdTipoTransaccion, IdCuentaOrigen, IdCuentaDestino, Monto, SaldoResultante, Estado)
            VALUES
                (p_IdTarjeta, p_IdCajero, v_IdTipoTransferencia, p_IdCuentaOrigen, v_IdCuentaDestino, p_Monto, v_SaldoOrigen, 'Rechazada');
            SET p_IdTransaccion = LAST_INSERT_ID();

            SET p_Resultado = 'SALDO_INSUFICIENTE';
            SET p_SaldoResultante = v_SaldoOrigen;

        ELSE
            UPDATE Cuenta SET Saldo = Saldo - p_Monto WHERE IdCuenta = p_IdCuentaOrigen;
            UPDATE Cuenta SET Saldo = Saldo + p_Monto WHERE IdCuenta = v_IdCuentaDestino;

            SET p_SaldoResultante = v_SaldoOrigen - p_Monto;

            INSERT INTO Transaccion
                (IdTarjeta, IdCajero, IdTipoTransaccion, IdCuentaOrigen, IdCuentaDestino, Monto, SaldoResultante, Estado)
            VALUES
                (p_IdTarjeta, p_IdCajero, v_IdTipoTransferencia, p_IdCuentaOrigen, v_IdCuentaDestino, p_Monto, p_SaldoResultante, 'Exitosa');
            SET p_IdTransaccion = LAST_INSERT_ID();

            COMMIT;
            SET p_Resultado = 'OK';
        END IF;
    END IF;
END $$

DELIMITER ;

-- =========================================================
-- PRUEBA RÁPIDA (comentada). Con la tarjeta del seed:
-- NumeroTarjeta = '4000111122223333', PIN = '1234'
--
-- CALL USP_ObtenerDatosLogin('4000111122223333');
--
-- (hash PBKDF2 del PIN 1234 con el salt del seed; ver script 01)
-- CALL USP_ValidarLogin(1, UNHEX('1A1A1772D2B4B74BF3C01D538FC3F45BD8536DFA17BDF135D2FF50DE4F5E76BE'), @resultado, @idCuenta);
-- SELECT @resultado, @idCuenta;
-- =========================================================
