/* =============================================================
   02_sp_create_commerce.sql
   Inserta un registro del CSV en la tabla commerce.
   La API lo invoca una vez por cada fila del archivo.
   ============================================================= */

USE CommerceDB;
GO

CREATE OR ALTER PROCEDURE dbo.sp_create_commerce
    @pc_codcomercio NVARCHAR(20),
    @pc_nomcomred   NVARCHAR(150),
    @pc_razonsocial NVARCHAR(200),
    @pc_tipdoc      NVARCHAR(10),
    @pc_numdoc      NVARCHAR(30),
    @pc_direccion   NVARCHAR(250),
    @pc_telefono    NVARCHAR(30),
    @pc_email       NVARCHAR(150),
    @pc_processdate DATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.commerce
        (pc_codcomercio, pc_nomcomred, pc_razonsocial, pc_tipdoc,
         pc_numdoc, pc_direccion, pc_telefono, pc_email, pc_processdate)
    VALUES
        (@pc_codcomercio, @pc_nomcomred, @pc_razonsocial, @pc_tipdoc,
         @pc_numdoc, @pc_direccion, @pc_telefono, @pc_email, @pc_processdate);
END
GO
