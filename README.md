# Commerce Challenge

Aplicación web para cargar archivos CSV de comercios, validarlos y revisar los registros con errores. Incluye inicio de sesión con JWT.

- **Front End:** Angular 22
- **Back End:** API REST en .NET 8 (ASP.NET Core)
- **Base de datos:** SQL Server (stored procedures)

## Estructura del repositorio

```
commerce-challenge/
├── backend/CommerceSolution/    API REST (.NET 8) y pruebas unitarias (CommerceApi.Tests)
├── frontend/commerce-web/       Aplicación Angular
├── database/                    Scripts SQL Server (ejecutar en orden)
└── samples/                     CSV de prueba
```

## Requisitos

- Visual Studio 2022 (17.14 o superior) con la carga de trabajo "ASP.NET y desarrollo web"
- .NET SDK 8
- SQL Server (Express es suficiente) y SQL Server Management Studio
- Node.js LTS y Angular CLI (`npm install -g @angular/cli`)

## Puesta en marcha

### 1. Base de datos

En SSMS, conectado a `localhost\SQLEXPRESS`, ejecuta en este orden:

1. `database/01_create_database_and_tables.sql`
2. `database/02_sp_create_commerce.sql`
3. `database/03_sp_process_commerce.sql`
4. `database/04_create_app_user.sql`

Los scripts se pueden ejecutar más de una vez sin perder datos.

### 2. Backend

1. Abre `backend/CommerceSolution/CommerceSolution.sln` en Visual Studio.
2. Revisa la cadena de conexión en `CommerceApi/appsettings.json` (clave `ConnectionStrings:CommerceDb`) y ajústala si tu instancia es distinta.
3. Ejecuta con **F5**. La API queda en `https://localhost:7030` y Swagger en `https://localhost:7030/swagger`.
   Desde terminal: `dotnet run --launch-profile https` dentro de `CommerceApi`. Si el navegador no confía en el certificado local, ejecuta una vez `dotnet dev-certs https --trust`.

Al arrancar, la API crea el usuario inicial si la tabla `app_user` está vacía.

### 3. Frontend

```bash
cd frontend/commerce-web
npm install
ng serve
```

Abre `http://localhost:4200`. Si la API usa otro puerto, cámbialo en `src/app/services/commerce.service.ts` y `src/app/services/auth.service.ts`.

## Inicio de sesión

Usuario de prueba creado automáticamente:

| Usuario | Contraseña |
|---|---|
| `admin` | `Admin123*` |

Estos valores salen de la sección `SeedUser` de `appsettings.json` y son solo para desarrollo.

- La contraseña se guarda con hash BCrypt.
- Al iniciar sesión la API devuelve un token JWT (60 minutos). El frontend lo guarda en `sessionStorage`, lo envía en cada petición y cierra la sesión si la API responde `401`.
- Las rutas de Angular están protegidas con un *guard*, y los endpoints de `Commerce` con `[Authorize]`.
- En Swagger: ejecuta `POST /api/Auth/login`, pulsa **Authorize** y pega el token (sin la palabra "Bearer").

## Funcionalidades

**Pantallas**

| Pantalla | Qué hace |
|---|---|
| Iniciar sesión | Autentica al usuario y guarda el token |
| Cargar archivo | Selecciona `commerce_DDMMYYYY.csv`, lo previsualiza y lo envía al servidor |
| Procesar | Valida los registros de una fecha (`pc_processdate`) y muestra cuántos fueron a cuarentena |
| Errores | Lista los registros en cuarentena con su motivo, con filtro por fecha |

**Endpoints**

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/Auth/login` | Valida credenciales y devuelve un JWT. `401` si son incorrectas |
| POST | `/api/Commerce/upload` | Recibe el CSV y lo registra con `sp_create_commerce`. `400` si el archivo está vacío o el nombre es incorrecto, `409` si ya fue cargado |
| POST | `/api/Commerce/process?processDate=YYYY-MM-DD` | Ejecuta `sp_process_commerce` y devuelve la cantidad de registros enviados a cuarentena |
| GET | `/api/Commerce/quarantine?processDate=YYYY-MM-DD` | Lista `commerce_quarantine` (la fecha es opcional) |

Los endpoints de `Commerce` requieren el token JWT.

**Reglas de validación** (`sp_process_commerce`)

- `pc_nomcomred` no puede estar vacío.
- `pc_numdoc` no puede estar vacío ni contener letras o caracteres especiales (solo dígitos).
- Los registros inválidos se eliminan de `commerce` y se guardan en `commerce_quarantine` con la columna `motivo`.

## Decisiones de diseño

- **Capas en el backend:** Controller (HTTP) → Service (validaciones y lectura del CSV) → Repository (SQL Server con Dapper y stored procedures).
- **Carga y validación separadas:** la carga guarda todas las filas tal como llegan; las reglas se aplican después en `sp_process_commerce`, como pide el enunciado. Así ningún registro se pierde sin quedar registrado con su motivo.
- **Transacciones:** la carga de un archivo se registra completa o no se registra. El proceso de validación también es transaccional.
- **Sin duplicados:** la tabla `commerce_file_log` tiene una restricción `UNIQUE` sobre el nombre del archivo, de modo que un mismo CSV no se puede cargar dos veces. Procesar la misma fecha varias veces es seguro.
- **Estado en el frontend:** *signals* y componentes standalone, con el acceso a la API aislado en servicios (`CommerceService`, `AuthService`), un interceptor HTTP y un guard de rutas.

## Pruebas unitarias

`CommerceApi.Tests` prueba `CommerceService` con **xUnit** y **Moq**. El repositorio se reemplaza por un mock, así que no necesitan SQL Server. Cubren el archivo vacío, el nombre incorrecto, el CSV sin filas y la carga correcta.

```bash
cd backend/CommerceSolution
dotnet test
```

## Datos de prueba

`samples/commerce_07102026.csv` contiene 7 registros: 3 válidos y 4 con errores. Resultado esperado: carga de 7, proceso de `2026-10-07` envía 4 registros a cuarentena.

## Mejoras posibles

- Mover la clave JWT y el usuario inicial a variables de entorno o *User Secrets* (en este reto están en `appsettings.json` por simplicidad).
- Inserción por lotes (por ejemplo `SqlBulkCopy` o un parámetro de tabla) para archivos muy grandes.
- Gestión de usuarios y roles.
- Pruebas de integración del repositorio contra SQL Server y pruebas unitarias del frontend.
