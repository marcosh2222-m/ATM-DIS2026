-- =========================================================
-- PROYECTO: Cajero Automático (3 capas - .NET Forms)
-- Script: Pruebas de los stored procedures
--
-- Ejecutar sobre una base RECIÉN CREADA (01 + 02), porque las pruebas
-- dependen de los datos iniciales y los modifican (bloquean una
-- tarjeta, mueven saldos). Al terminar, volver a correr 01 y 02 para
-- dejar la base limpia.
--
-- Cada prueba agrega una fila a la tabla temporal "resultados" con OK o
-- FALLA. Al final se muestra el detalle y el resumen.
-- =========================================================

SET NAMES utf8mb4;
USE cajero_atm_db;

DROP TEMPORARY TABLE IF EXISTS resultados;
CREATE TEMPORARY TABLE resultados (
    Nro       INT AUTO_INCREMENT PRIMARY KEY,
    Prueba    VARCHAR(80),
    Esperado  VARCHAR(60),
    Obtenido  VARCHAR(60),
    Ok        BOOLEAN
);

-- Hash PBKDF2 de los PIN del seed (ver script 01)
SET @hash1234 = UNHEX('1A1A1772D2B4B74BF3C01D538FC3F45BD8536DFA17BDF135D2FF50DE4F5E76BE');
SET @hash5678 = UNHEX('C546408B2F79C155F83407BB3BBF9D8123652BE4200B7FD4EFE712F9FC33295E');
SET @hashMal  = UNHEX('00');

-- ---------------------------------------------------------
-- LOGIN (USP_ValidarLogin)
-- ---------------------------------------------------------
CALL USP_ValidarLogin(1, @hash1234, @r, @c);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
VALUES ('Login tarjeta 1 con PIN correcto', 'OK / cuenta 1', CONCAT(@r, ' / cuenta ', IFNULL(@c, 'NULL')), @r = 'OK' AND @c = 1);

CALL USP_ValidarLogin(1, @hashMal, @r, @c);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
SELECT 'Login con PIN incorrecto suma un intento', 'PIN_INCORRECTO / 1 intento',
       CONCAT(@r, ' / ', IntentosFallidos, ' intento'), @r = 'PIN_INCORRECTO' AND IntentosFallidos = 1
FROM Tarjeta WHERE IdTarjeta = 1;

CALL USP_ValidarLogin(1, @hash1234, @r, @c);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
SELECT 'Login correcto reinicia los intentos', 'OK / 0 intentos',
       CONCAT(@r, ' / ', IntentosFallidos, ' intentos'), @r = 'OK' AND IntentosFallidos = 0
FROM Tarjeta WHERE IdTarjeta = 1;

CALL USP_ValidarLogin(2, @hashMal, @r, @c);
CALL USP_ValidarLogin(2, @hashMal, @r, @c);
CALL USP_ValidarLogin(2, @hashMal, @r, @c);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
SELECT 'Tercer PIN incorrecto bloquea la tarjeta', 'BLOQUEADA / Bloqueada',
       CONCAT(@r, ' / ', Estado), @r = 'BLOQUEADA' AND Estado = 'Bloqueada'
FROM Tarjeta WHERE IdTarjeta = 2;

CALL USP_ValidarLogin(2, @hash5678, @r, @c);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
VALUES ('Tarjeta bloqueada rechaza aunque el PIN sea correcto', 'BLOQUEADA', @r, @r = 'BLOQUEADA');

CALL USP_ValidarLogin(999, @hash1234, @r, @c);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
VALUES ('Tarjeta inexistente', 'NO_EXISTE', @r, @r = 'NO_EXISTE');

-- Tarjeta vencida (se agrega solo para esta prueba)
INSERT INTO Tarjeta (IdCuenta, NumeroTarjeta, PIN_Hash, Salt, FechaVencimiento, Estado)
VALUES (2, '4000999900000001', @hash5678, UNHEX('00'), '2024-01-31', 'Activa');
SET @idVencida = LAST_INSERT_ID();
CALL USP_ValidarLogin(@idVencida, @hash5678, @r, @c);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
SELECT 'Tarjeta vencida queda marcada como Vencida', 'VENCIDA / Vencida',
       CONCAT(@r, ' / ', Estado), @r = 'VENCIDA' AND Estado = 'Vencida'
FROM Tarjeta WHERE IdTarjeta = @idVencida;

-- ---------------------------------------------------------
-- CONSULTA (USP_ConsultarSaldo) - devuelve un resultado y registra la consulta
-- ---------------------------------------------------------
CALL USP_ConsultarSaldo(1, 1, 1);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
SELECT 'Consulta de saldo queda registrada', '1 consulta de $ 50000',
       CONCAT(COUNT(*), ' consulta de $ ', MAX(SaldoResultante)), COUNT(*) = 1 AND MAX(SaldoResultante) = 50000
FROM Transaccion WHERE IdTipoTransaccion = 1 AND IdCuentaOrigen = 1;

-- ---------------------------------------------------------
-- EXTRACCIÓN (USP_Extraccion)
-- ---------------------------------------------------------
CALL USP_Extraccion(1, 1, 1, 1000, @r, @s, @t);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
VALUES ('Extracción de $ 1000', 'OK / 49000.00 / con comprobante', CONCAT(@r, ' / ', @s, ' / ', IF(@t IS NULL, 'sin', 'con'), ' comprobante'),
        @r = 'OK' AND @s = 49000 AND @t IS NOT NULL);

CALL USP_Extraccion(1, 1, 1, 150, @r, @s, @t);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
VALUES ('Extracción no múltiplo de 100', 'MONTO_INVALIDO', @r, @r = 'MONTO_INVALIDO');

CALL USP_Extraccion(1, 1, 1, 0, @r, @s, @t);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
VALUES ('Extracción de $ 0', 'MONTO_INVALIDO', @r, @r = 'MONTO_INVALIDO');

CALL USP_Extraccion(1, 1, 1, 999900, @r, @s, @t);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
VALUES ('Extracción mayor al saldo', 'SALDO_INSUFICIENTE / 49000.00', CONCAT(@r, ' / ', @s), @r = 'SALDO_INSUFICIENTE' AND @s = 49000);

CALL USP_Extraccion(3, 1, 1, 100, @r, @s, @t);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
VALUES ('Extracción de cuenta bloqueada', 'CUENTA_BLOQUEADA', @r, @r = 'CUENTA_BLOQUEADA');

CALL USP_Extraccion(999, 1, 1, 100, @r, @s, @t);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
VALUES ('Extracción de cuenta inexistente', 'CUENTA_NO_EXISTE', @r, @r = 'CUENTA_NO_EXISTE');

-- ---------------------------------------------------------
-- TRANSFERENCIA (USP_Transferencia)
-- ---------------------------------------------------------
CALL USP_Transferencia(1, '0002-0002-02', 1, 1, 2000, @r, @s, @t);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
SELECT 'Transferencia de $ 2000 a 0002-0002-02', 'OK / origen 47000.00 / destino 17000.00',
       CONCAT(@r, ' / origen ', @s, ' / destino ', Saldo), @r = 'OK' AND @s = 47000 AND Saldo = 17000
FROM Cuenta WHERE IdCuenta = 2;

CALL USP_Transferencia(1, '0003-0003-03', 1, 1, 100, @r, @s, @t);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
VALUES ('Transferencia a cuenta bloqueada', 'CUENTA_DESTINO_INVALIDA', @r, @r = 'CUENTA_DESTINO_INVALIDA');

CALL USP_Transferencia(1, '0001-0001-01', 1, 1, 100, @r, @s, @t);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
VALUES ('Transferencia a la misma cuenta', 'MISMA_CUENTA', @r, @r = 'MISMA_CUENTA');

CALL USP_Transferencia(1, '9999-9999-99', 1, 1, 100, @r, @s, @t);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
VALUES ('Transferencia a cuenta inexistente', 'CUENTA_DESTINO_INVALIDA', @r, @r = 'CUENTA_DESTINO_INVALIDA');

CALL USP_Transferencia(1, '0002-0002-02', 1, 1, 0, @r, @s, @t);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
VALUES ('Transferencia de $ 0', 'MONTO_INVALIDO', @r, @r = 'MONTO_INVALIDO');

CALL USP_Transferencia(1, '0002-0002-02', 1, 1, 999999, @r, @s, @t);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
VALUES ('Transferencia mayor al saldo', 'SALDO_INSUFICIENTE / 47000.00', CONCAT(@r, ' / ', @s), @r = 'SALDO_INSUFICIENTE' AND @s = 47000);

CALL USP_Transferencia(3, '0001-0001-01', 1, 1, 100, @r, @s, @t);
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
VALUES ('Transferencia desde cuenta bloqueada', 'CUENTA_BLOQUEADA', @r, @r = 'CUENTA_BLOQUEADA');

-- ---------------------------------------------------------
-- LOG DE TRANSACCIONES
-- 1 consulta + 3 extracciones registradas (OK, saldo insuf., bloqueada)
-- + 4 transferencias registradas (OK, destino bloqueado, saldo insuf., origen bloqueado).
-- Los montos inválidos, cuentas inexistentes y misma cuenta no llegan al log.
-- ---------------------------------------------------------
INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
SELECT 'Cantidad de transacciones registradas', '8 (3 exitosas, 5 rechazadas)',
       CONCAT(COUNT(*), ' (', SUM(Estado = 'Exitosa'), ' exitosas, ', SUM(Estado = 'Rechazada'), ' rechazadas)'),
       COUNT(*) = 8 AND SUM(Estado = 'Exitosa') = 3
FROM Transaccion;

INSERT INTO resultados (Prueba, Esperado, Obtenido, Ok)
SELECT 'Ningún saldo negativo', '0 cuentas', CONCAT(COUNT(*), ' cuentas'), COUNT(*) = 0
FROM Cuenta WHERE Saldo < 0;

-- ---------------------------------------------------------
-- RESULTADO
-- ---------------------------------------------------------
SELECT Nro, Prueba, Esperado, Obtenido, IF(Ok, 'OK', 'FALLA') AS Resultado FROM resultados ORDER BY Nro;

SELECT COUNT(*) AS Pruebas, SUM(Ok) AS Correctas, COUNT(*) - SUM(Ok) AS Fallidas,
       IF(SUM(Ok) = COUNT(*), 'TODAS LAS PRUEBAS OK', 'HAY PRUEBAS FALLIDAS') AS Resumen
FROM resultados;
