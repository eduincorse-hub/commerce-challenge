/* =============================================================
   01_create_database_and_tables.sql
   Crea la base de datos CommerceDB y sus tablas.
   Se puede ejecutar varias veces: no borra datos existentes.
   ============================================================= */

IF DB_ID(N'CommerceDB') IS NULL
    CREATE DATABASE CommerceDB;
GO

USE CommerceDB;
GO

/* Tabla principal: misma estructura del CSV (más un id autoincremental). */
IF OBJECT_ID(N'dbo.commerce', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.commerce (
        id              INT IDENTITY(1,1) PRIMARY KEY,
        pc_codcomercio  NVARCHAR(20)  NULL,
        pc_nomcomred    NVARCHAR(150) NULL,
        pc_razonsocial  NVARCHAR(200) NULL,
        pc_tipdoc       NVARCHAR(10)  NULL,
        pc_numdoc       NVARCHAR(30)  NULL,
        pc_direccion    NVARCHAR(250) NULL,
        pc_telefono     NVARCHAR(30)  NULL,
        pc_email        NVARCHAR(150) NULL,
        pc_processdate  DATE          NOT NULL
    );
END
GO

/* Registros que no pasaron las validaciones, con el motivo del rechazo. */
IF OBJECT_ID(N'dbo.commerce_quarantine', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.commerce_quarantine (
        id              INT IDENTITY(1,1) PRIMARY KEY,
        pc_codcomercio  NVARCHAR(20)  NULL,
        pc_nomcomred    NVARCHAR(150) NULL,
        pc_razonsocial  NVARCHAR(200) NULL,
        pc_tipdoc       NVARCHAR(10)  NULL,
        pc_numdoc       NVARCHAR(30)  NULL,
        pc_direccion    NVARCHAR(250) NULL,
        pc_telefono     NVARCHAR(30)  NULL,
        pc_email        NVARCHAR(150) NULL,
        pc_processdate  DATE          NOT NULL,
        motivo          NVARCHAR(500) NOT NULL
    );
END
GO

/* Registro de archivos cargados: evita cargar dos veces el mismo CSV. */
IF OBJECT_ID(N'dbo.commerce_file_log', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.commerce_file_log (
        id          INT IDENTITY(1,1) PRIMARY KEY,
        file_name   NVARCHAR(100) NOT NULL,
        total_rows  INT           NOT NULL,
        loaded_at   DATETIME2     NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT UQ_commerce_file_log_file_name UNIQUE (file_name)
    );
END
GO
