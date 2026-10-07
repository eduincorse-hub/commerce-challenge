/* =============================================================
   03_sp_process_commerce.sql
   Valida los registros de commerce para una fecha de proceso y
   mueve los inválidos a commerce_quarantine con su motivo.

   Reglas:
     - pc_nomcomred no debe estar vacío.
     - pc_numdoc no debe estar vacío ni contener letras o
       caracteres especiales (solo dígitos 0-9).

   Devuelve la cantidad de registros insertados en cuarentena.
   Todo se ejecuta en una transacción: si algo falla, se revierte.
   ============================================================= */

USE CommerceDB;
GO

CREATE OR ALTER PROCEDURE dbo.sp_process_commerce
    @processdate DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @count INT = 0;

    BEGIN TRY
        BEGIN TRANSACTION;

        /* Registros inválidos de la fecha, con el motivo (puede haber más de uno). */
        SELECT
            c.id,
            RTRIM(CONCAT(
                CASE WHEN ISNULL(LTRIM(RTRIM(c.pc_nomcomred)), '') = ''
                     THEN N'El nombre del comercio (nomcomred) se encuentra vacío. ' END,
                CASE WHEN ISNULL(LTRIM(RTRIM(c.pc_numdoc)), '') = ''
                     THEN N'El número (numdoc) se encuentra vacío. '
                     WHEN LTRIM(RTRIM(c.pc_numdoc)) LIKE '%[^0-9]%'
                     THEN N'El número (numdoc) contiene letras o caracteres especiales. ' END
            )) AS motivo
        INTO #invalidos
        FROM dbo.commerce c
        WHERE c.pc_processdate = @processdate
          AND (
                ISNULL(LTRIM(RTRIM(c.pc_nomcomred)), '') = ''
             OR ISNULL(LTRIM(RTRIM(c.pc_numdoc)), '') = ''
             OR LTRIM(RTRIM(c.pc_numdoc)) LIKE '%[^0-9]%'
          );

        INSERT INTO dbo.commerce_quarantine
            (pc_codcomercio, pc_nomcomred, pc_razonsocial, pc_tipdoc,
             pc_numdoc, pc_direccion, pc_telefono, pc_email, pc_processdate, motivo)
        SELECT c.pc_codcomercio, c.pc_nomcomred, c.pc_razonsocial, c.pc_tipdoc,
               c.pc_numdoc, c.pc_direccion, c.pc_telefono, c.pc_email,
               c.pc_processdate, i.motivo
        FROM dbo.commerce c
        INNER JOIN #invalidos i ON i.id = c.id;

        SET @count = @@ROWCOUNT;

        DELETE c
        FROM dbo.commerce c
        INNER JOIN #invalidos i ON i.id = c.id;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH

    SELECT @count AS registros_cuarentena;
END
GO
