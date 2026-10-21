-- =========================================================
-- PROYECTO: Cajero Automático (3 capas - .NET Forms)
-- Script: Usuario de MySQL para la aplicación
-- Ejecutar con un usuario administrador (root), DESPUÉS de 01 y 02.
--
-- La aplicación solo necesita EJECUTAR los stored procedures: nunca
-- lee ni modifica tablas directamente. Con este usuario, aunque
-- alguien obtenga la contraseña del App.config, no puede hacer un
-- SELECT de las tarjetas ni un UPDATE de los saldos.
--
-- Después de correrlo, en App.config:
--   User ID=cajero_app;Password=<la que pongas abajo>;
-- =========================================================

SET NAMES utf8mb4;

DROP USER IF EXISTS 'cajero_app'@'localhost';

-- Cambiar la contraseña antes de ejecutar.
CREATE USER 'cajero_app'@'localhost' IDENTIFIED BY 'CambiarEsta.2026';

-- Solo permiso de ejecución sobre los procedures de la base del cajero.
GRANT EXECUTE ON cajero_atm_db.* TO 'cajero_app'@'localhost';

FLUSH PRIVILEGES;

-- Verificación: debería mostrar únicamente GRANT EXECUTE.
SHOW GRANTS FOR 'cajero_app'@'localhost';
