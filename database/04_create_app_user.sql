/* =============================================================
   04_create_app_user.sql
   Tabla de usuarios para el inicio de sesión.

   La contraseña se guarda con hash (BCrypt), nunca en texto plano.
   No se inserta ningún usuario aquí: al arrancar, la API crea el
   usuario inicial (clave SeedUser de appsettings.json) si la tabla
   está vacía, porque el hash lo genera la aplicación.
   ============================================================= */

USE CommerceDB;
GO

IF OBJECT_ID(N'dbo.app_user', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.app_user (
        id            INT IDENTITY(1,1) PRIMARY KEY,
        username      NVARCHAR(50)  NOT NULL,
        password_hash NVARCHAR(200) NOT NULL,
        created_at    DATETIME2     NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT UQ_app_user_username UNIQUE (username)
    );
END
GO
