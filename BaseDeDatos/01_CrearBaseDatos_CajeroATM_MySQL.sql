-- =========================================================
-- PROYECTO: Cajero Automático (3 capas - .NET Forms)
-- Motor: MySQL 8.0+
-- Script: Creación de base de datos y tablas
--
-- CAMBIOS RESPECTO A LA VERSIÓN SQL SERVER:
-- - IDENTITY(1,1)         -> AUTO_INCREMENT
-- - NVARCHAR              -> VARCHAR (con charset utf8mb4 a nivel BD,
--                            así que los acentos/ñ funcionan igual)
-- - DATETIME2(0)          -> DATETIME (MySQL no tiene DATETIME2, y no
--                            necesitamos fracciones de segundo acá)
-- - Los índices se declaran DENTRO del CREATE TABLE (más compacto
--   que las sentencias CREATE INDEX separadas de SQL Server)
-- - No existen los índices FILTRADOS (WHERE) de SQL Server: el índice
--   de IdCuentaDestino queda como índice normal, sin filtro
-- =========================================================

-- El script está guardado en UTF-8: sin esto, un cliente con otro
-- juego de caracteres guardaría "Pérez" como "PÃ©rez".
SET NAMES utf8mb4;

CREATE DATABASE IF NOT EXISTS cajero_atm_db
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE cajero_atm_db;

-- =========================================================
-- 1. SUCURSAL
-- =========================================================
CREATE TABLE Sucursal (
    IdSucursal      INT AUTO_INCREMENT PRIMARY KEY,
    Nombre          VARCHAR(100)  NOT NULL,
    Direccion       VARCHAR(200)  NOT NULL,
    CodigoSucursal  VARCHAR(10)   NOT NULL,

    CONSTRAINT UQ_Sucursal_Codigo UNIQUE (CodigoSucursal)
) ENGINE=InnoDB;

-- =========================================================
-- 2. CAJERO
-- =========================================================
CREATE TABLE Cajero (
    IdCajero      INT AUTO_INCREMENT PRIMARY KEY,
    IdSucursal    INT           NOT NULL,
    NumeroSerie   VARCHAR(30)   NOT NULL,
    Ubicacion     VARCHAR(150)  NOT NULL,
    Estado        VARCHAR(20)   NOT NULL DEFAULT 'Activo',

    CONSTRAINT UQ_Cajero_NumeroSerie UNIQUE (NumeroSerie),
    CONSTRAINT CK_Cajero_Estado CHECK (Estado IN ('Activo','Inactivo','Mantenimiento')),
    CONSTRAINT FK_Cajero_Sucursal FOREIGN KEY (IdSucursal)
        REFERENCES Sucursal(IdSucursal),

    INDEX IX_Cajero_IdSucursal (IdSucursal)
) ENGINE=InnoDB;

-- =========================================================
-- 3. CLIENTE
-- =========================================================
CREATE TABLE Cliente (
    IdCliente   INT AUTO_INCREMENT PRIMARY KEY,
    Nombre      VARCHAR(80)   NOT NULL,
    Apellido    VARCHAR(80)   NOT NULL,
    DNI         VARCHAR(15)   NOT NULL,
    Email       VARCHAR(120)  NULL,
    Telefono    VARCHAR(20)   NULL,
    FechaAlta   DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT UQ_Cliente_DNI UNIQUE (DNI)
) ENGINE=InnoDB;

-- =========================================================
-- 4. CUENTA
-- =========================================================
CREATE TABLE Cuenta (
    IdCuenta      INT AUTO_INCREMENT PRIMARY KEY,
    IdCliente     INT             NOT NULL,
    NumeroCuenta  VARCHAR(20)     NOT NULL,
    Saldo         DECIMAL(14,2)   NOT NULL DEFAULT 0,
    TipoCuenta    VARCHAR(20)     NOT NULL DEFAULT 'Caja de Ahorro',
    Estado        VARCHAR(20)     NOT NULL DEFAULT 'Activa',
    FechaAlta     DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT UQ_Cuenta_Numero UNIQUE (NumeroCuenta),
    CONSTRAINT CK_Cuenta_Tipo CHECK (TipoCuenta IN ('Caja de Ahorro','Cuenta Corriente')),
    CONSTRAINT CK_Cuenta_Estado CHECK (Estado IN ('Activa','Bloqueada','Cerrada')),
    CONSTRAINT CK_Cuenta_SaldoNoNegativo CHECK (Saldo >= 0),
    CONSTRAINT FK_Cuenta_Cliente FOREIGN KEY (IdCliente)
        REFERENCES Cliente(IdCliente),

    INDEX IX_Cuenta_IdCliente (IdCliente)
) ENGINE=InnoDB;

-- =========================================================
-- 5. TARJETA
-- PIN protegido con salt + PBKDF2 (se calcula en C#, la BD
-- solo guarda el resultado ya hasheado y su salt).
-- =========================================================
CREATE TABLE Tarjeta (
    IdTarjeta         INT AUTO_INCREMENT PRIMARY KEY,
    IdCuenta          INT             NOT NULL,
    NumeroTarjeta     VARCHAR(20)     NOT NULL,
    PIN_Hash          VARBINARY(64)   NOT NULL,   -- salida de Rfc2898DeriveBytes
    Salt              VARBINARY(32)   NOT NULL,   -- salt aleatorio por tarjeta
    FechaVencimiento  DATE            NOT NULL,
    IntentosFallidos  TINYINT UNSIGNED NOT NULL DEFAULT 0,
    Estado            VARCHAR(20)     NOT NULL DEFAULT 'Activa',

    CONSTRAINT UQ_Tarjeta_Numero UNIQUE (NumeroTarjeta),
    CONSTRAINT CK_Tarjeta_Estado CHECK (Estado IN ('Activa','Bloqueada','Vencida','Anulada')),
    CONSTRAINT CK_Tarjeta_Intentos CHECK (IntentosFallidos BETWEEN 0 AND 3),
    CONSTRAINT FK_Tarjeta_Cuenta FOREIGN KEY (IdCuenta)
        REFERENCES Cuenta(IdCuenta),

    INDEX IX_Tarjeta_IdCuenta (IdCuenta)
) ENGINE=InnoDB;

-- =========================================================
-- 6. TIPO DE TRANSACCION
-- =========================================================
CREATE TABLE TipoTransaccion (
    IdTipoTransaccion  INT AUTO_INCREMENT PRIMARY KEY,
    Nombre             VARCHAR(30) NOT NULL,

    CONSTRAINT UQ_TipoTransaccion_Nombre UNIQUE (Nombre)
) ENGINE=InnoDB;

-- =========================================================
-- 7. TRANSACCION
-- =========================================================
CREATE TABLE Transaccion (
    IdTransaccion      INT AUTO_INCREMENT PRIMARY KEY,
    IdTarjeta          INT             NOT NULL,
    IdCajero           INT             NOT NULL,
    IdTipoTransaccion  INT             NOT NULL,
    IdCuentaOrigen     INT             NOT NULL,
    IdCuentaDestino    INT             NULL,        -- solo en transferencias
    Monto              DECIMAL(14,2)   NOT NULL DEFAULT 0,
    Fecha              DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    SaldoResultante    DECIMAL(14,2)   NOT NULL,
    Estado             VARCHAR(20)     NOT NULL DEFAULT 'Exitosa',

    CONSTRAINT CK_Transaccion_Estado CHECK (Estado IN ('Exitosa','Rechazada','Error')),
    CONSTRAINT CK_Transaccion_Monto CHECK (Monto >= 0),
    CONSTRAINT FK_Transaccion_Tarjeta FOREIGN KEY (IdTarjeta)
        REFERENCES Tarjeta(IdTarjeta),
    CONSTRAINT FK_Transaccion_Cajero FOREIGN KEY (IdCajero)
        REFERENCES Cajero(IdCajero),
    CONSTRAINT FK_Transaccion_Tipo FOREIGN KEY (IdTipoTransaccion)
        REFERENCES TipoTransaccion(IdTipoTransaccion),
    CONSTRAINT FK_Transaccion_CuentaOrigen FOREIGN KEY (IdCuentaOrigen)
        REFERENCES Cuenta(IdCuenta),
    CONSTRAINT FK_Transaccion_CuentaDestino FOREIGN KEY (IdCuentaDestino)
        REFERENCES Cuenta(IdCuenta),

    INDEX IX_Transaccion_IdCajero (IdCajero),
    INDEX IX_Transaccion_IdTipoTransaccion (IdTipoTransaccion),
    -- Historial de movimientos de una cuenta, más reciente primero
    INDEX IX_Transaccion_CuentaOrigen_Fecha (IdCuentaOrigen, Fecha DESC),
    INDEX IX_Transaccion_CuentaDestino_Fecha (IdCuentaDestino, Fecha DESC),
    -- Últimas operaciones hechas con una tarjeta (ticket/resumen)
    INDEX IX_Transaccion_Tarjeta_Fecha (IdTarjeta, Fecha DESC)
) ENGINE=InnoDB;

-- =========================================================
-- DATOS INICIALES (seed)
-- =========================================================

INSERT INTO TipoTransaccion (Nombre) VALUES
    ('Consulta de Saldo'),
    ('Extraccion'),
    ('Transferencia');

INSERT INTO Sucursal (Nombre, Direccion, CodigoSucursal)
VALUES ('Sucursal Centro', 'Av. Corrientes 1234, CABA', 'SUC001');

INSERT INTO Cajero (IdSucursal, NumeroSerie, Ubicacion, Estado)
VALUES (1, 'ATM-0001', 'Hall de entrada - Sucursal Centro', 'Activo');

INSERT INTO Cliente (Nombre, Apellido, DNI, Email, Telefono) VALUES
    ('Marcos', 'Pérez', '30123456', 'marcos@mail.com', '1122334455'),
    ('Laura',  'Gómez', '28987654', 'laura@mail.com',  '1155667788');

-- Cuenta 1: la del login de prueba. Cuenta 2: destino válido para
-- transferencias. Cuenta 3: bloqueada, para probar rechazos.
-- (Mismos datos que usa ServicioCuentaSimulado en la Presentación.)
INSERT INTO Cuenta (IdCliente, NumeroCuenta, Saldo, TipoCuenta, Estado) VALUES
    (1, '0001-0001-01', 50000.00, 'Caja de Ahorro',   'Activa'),
    (2, '0002-0002-02', 15000.00, 'Cuenta Corriente', 'Activa'),
    (2, '0003-0003-03',  8000.00, 'Caja de Ahorro',   'Bloqueada');

-- Tarjetas de prueba. El PIN_Hash es PBKDF2 exactamente como lo
-- calcula la capa de Negocio en C#:
--     Rfc2898DeriveBytes.Pbkdf2(PIN en UTF-8, Salt, 100000 iteraciones,
--                               HashAlgorithmName.SHA256, 32 bytes)
-- Los salt son fijos solo para que el seed sea reproducible; las
-- tarjetas reales reciben un salt aleatorio de 32 bytes.
--   4000111122223333 / PIN 1234 -> cuenta 0001-0001-01 (Marcos Pérez)
--   4000444455556666 / PIN 5678 -> cuenta 0002-0002-02 (Laura Gómez)
INSERT INTO Tarjeta (IdCuenta, NumeroTarjeta, PIN_Hash, Salt, FechaVencimiento, Estado) VALUES
    (1, '4000111122223333',
        UNHEX('1A1A1772D2B4B74BF3C01D538FC3F45BD8536DFA17BDF135D2FF50DE4F5E76BE'),
        UNHEX('4A1F9C2E7B3D58A06E21C4F7980B3D5A1C6E8F20B47D93A5E61C0F2B8D4A7E39'),
        '2029-12-31', 'Activa'),
    (2, '4000444455556666',
        UNHEX('C546408B2F79C155F83407BB3BBF9D8123652BE4200B7FD4EFE712F9FC33295E'),
        UNHEX('B3E07A1D94C25F86E0A3D71B2C9F4E58A1D6073B9E2C5F84A17D0E6B3C92F51D'),
        '2030-06-30', 'Activa');
