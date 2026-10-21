-- =========================================================
-- PROYECTO: Cajero Automático (3 capas - .NET Forms)
-- Script: Verificación de consistencia de saldos y log
--
-- Se puede correr en cualquier momento (no modifica nada), por
-- ejemplo después de usar la aplicación un rato. Cada verificación
-- debería dar 0 problemas.
-- =========================================================

SET NAMES utf8mb4;
USE cajero_atm_db;

DROP TEMPORARY TABLE IF EXISTS verificacion;
CREATE TEMPORARY TABLE verificacion (
    Nro          INT AUTO_INCREMENT PRIMARY KEY,
    Verificacion VARCHAR(90),
    Problemas    INT
);

-- 1. Saldos negativos (nunca deberían existir: CHECK + FOR UPDATE en los SP)
INSERT INTO verificacion (Verificacion, Problemas)
SELECT 'Cuentas con saldo negativo', COUNT(*) FROM Cuenta WHERE Saldo < 0;

-- 2. El saldo actual coincide con el último movimiento registrado de la cuenta.
--    Se toma la última transacción donde la cuenta fue ORIGEN (consulta,
--    extracción o transferencia, exitosa o rechazada: todas guardan el
--    saldo en SaldoResultante). Si después recibió una transferencia
--    como DESTINO, no se puede comparar (el log no guarda el saldo del
--    destino) y la cuenta se omite.
INSERT INTO verificacion (Verificacion, Problemas)
SELECT 'Saldo actual distinto al del último movimiento registrado', COUNT(*)
FROM Cuenta c
INNER JOIN Transaccion t
        ON t.IdTransaccion = (SELECT MAX(x.IdTransaccion) FROM Transaccion x
                              WHERE x.IdCuentaOrigen = c.IdCuenta
                                 OR (x.IdCuentaDestino = c.IdCuenta AND x.Estado = 'Exitosa'))
WHERE t.IdCuentaOrigen = c.IdCuenta
  AND t.SaldoResultante <> c.Saldo;

-- 3. Transacciones exitosas de dinero con monto inválido
INSERT INTO verificacion (Verificacion, Problemas)
SELECT 'Extracciones/transferencias exitosas con monto <= 0', COUNT(*)
FROM Transaccion t
INNER JOIN TipoTransaccion tt ON tt.IdTipoTransaccion = t.IdTipoTransaccion
WHERE tt.Nombre IN ('Extraccion', 'Transferencia') AND t.Estado = 'Exitosa' AND t.Monto <= 0;

-- 4. Extracciones exitosas que no son múltiplo de 100
INSERT INTO verificacion (Verificacion, Problemas)
SELECT 'Extracciones exitosas que no son múltiplo de $ 100', COUNT(*)
FROM Transaccion t
INNER JOIN TipoTransaccion tt ON tt.IdTipoTransaccion = t.IdTipoTransaccion
WHERE tt.Nombre = 'Extraccion' AND t.Estado = 'Exitosa' AND MOD(t.Monto, 100) <> 0;

-- 5. Transferencias sin destino o a la misma cuenta
INSERT INTO verificacion (Verificacion, Problemas)
SELECT 'Transferencias sin destino o con origen = destino', COUNT(*)
FROM Transaccion t
INNER JOIN TipoTransaccion tt ON tt.IdTipoTransaccion = t.IdTipoTransaccion
WHERE tt.Nombre = 'Transferencia'
  AND (t.IdCuentaDestino IS NULL OR t.IdCuentaDestino = t.IdCuentaOrigen);

-- 6. Consultas y extracciones con cuenta destino (no corresponde)
INSERT INTO verificacion (Verificacion, Problemas)
SELECT 'Consultas/extracciones con cuenta destino', COUNT(*)
FROM Transaccion t
INNER JOIN TipoTransaccion tt ON tt.IdTipoTransaccion = t.IdTipoTransaccion
WHERE tt.Nombre IN ('Consulta de Saldo', 'Extraccion') AND t.IdCuentaDestino IS NOT NULL;

-- 7. Tarjetas con 3 intentos fallidos que siguen activas
INSERT INTO verificacion (Verificacion, Problemas)
SELECT 'Tarjetas con 3 intentos fallidos sin bloquear', COUNT(*)
FROM Tarjeta WHERE IntentosFallidos >= 3 AND Estado = 'Activa';

-- 8. Movimientos exitosos sobre cuentas que no están activas
INSERT INTO verificacion (Verificacion, Problemas)
SELECT 'Movimientos exitosos sobre cuentas no activas (origen)', COUNT(*)
FROM Transaccion t
INNER JOIN TipoTransaccion tt ON tt.IdTipoTransaccion = t.IdTipoTransaccion
INNER JOIN Cuenta c ON c.IdCuenta = t.IdCuentaOrigen
WHERE tt.Nombre IN ('Extraccion', 'Transferencia') AND t.Estado = 'Exitosa' AND c.Estado <> 'Activa';

-- ---------------------------------------------------------
-- RESULTADO
-- ---------------------------------------------------------
SELECT Nro, Verificacion, Problemas, IF(Problemas = 0, 'OK', 'REVISAR') AS Resultado
FROM verificacion ORDER BY Nro;

-- Resumen de movimientos por cuenta (informativo)
SELECT c.NumeroCuenta, c.Estado, c.Saldo AS SaldoActual,
       COUNT(t.IdTransaccion) AS Movimientos,
       COALESCE(SUM(t.Estado = 'Exitosa'), 0) AS Exitosos,
       COALESCE(SUM(t.Estado = 'Rechazada'), 0) AS Rechazados
FROM Cuenta c
LEFT JOIN Transaccion t ON t.IdCuentaOrigen = c.IdCuenta
GROUP BY c.IdCuenta, c.NumeroCuenta, c.Estado, c.Saldo
ORDER BY c.NumeroCuenta;
