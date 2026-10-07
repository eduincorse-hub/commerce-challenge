# Commerce Challenge

Aplicación web para cargar archivos CSV de comercios, validarlos y revisar los registros con errores.

- **Front End:** Angular 22
- **Back End:** API REST en .NET 8 (ASP.NET Core)
- **Base de datos:** SQL Server (stored procedures)

## Estructura del repositorio

```
commerce-challenge/
├── backend/CommerceSolution/    API REST (.NET 8)
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

Los scripts se pueden ejecutar más de una vez sin perder datos.

### 2. Backend

1. Abre `backend/CommerceSolution/CommerceSolution.sln` en Visual Studio.
2. Revisa la cadena de conexión en `CommerceApi/appsettings.json` (clave `ConnectionStrings:CommerceDb`) y ajústala si tu instancia es distinta.
3. Ejecuta con **F5**. La API queda en `https://localhost:7030` y Swagger en `https://localhost:7030/swagger`.

### 3. Frontend

```bash
cd frontend/commerce-web
npm install
ng serve
```

Abre `http://localhost:4200`. Si la API usa otro puerto, cámbialo en `src/app/services/commerce.service.ts`.

## Funcionalidades

**Pantallas**

| Pantalla | Qué hace |
|---|---|
| Cargar archivo | Selecciona `commerce_DDMMYYYY.csv`, lo previsualiza y lo envía al servidor |
| Procesar | Valida los registros de una fecha (`pc_processdate`) y muestra cuántos fueron a cuarentena |
| Errores | Lista los registros en cuarentena con su motivo, con filtro por fecha |

**Endpoints**

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/Commerce/upload` | Recibe el CSV y lo registra con `sp_create_commerce`. `400` si el archivo está vacío o el nombre es incorrecto, `409` si ya fue cargado |
| POST | `/api/Commerce/process?processDate=YYYY-MM-DD` | Ejecuta `sp_process_commerce` y devuelve la cantidad de registros enviados a cuarentena |
| GET | `/api/Commerce/quarantine?processDate=YYYY-MM-DD` | Lista `commerce_quarantine` (la fecha es opcional) |

**Reglas de validación** (`sp_process_commerce`)

- `pc_nomcomred` no puede estar vacío.
- `pc_numdoc` no puede estar vacío ni contener letras o caracteres especiales (solo dígitos).
- Los registros inválidos se eliminan de `commerce` y se guardan en `commerce_quarantine` con la columna `motivo`.

## Decisiones de diseño

- **Capas en el backend:** Controller (HTTP) → Service (validaciones y lectura del CSV) → Repository (SQL Server con Dapper y stored procedures).
- **Transacciones:** la carga de un archivo se registra completa o no se registra. El proceso de validación también es transaccional.
- **Sin duplicados:** la tabla `commerce_file_log` tiene una restricción `UNIQUE` sobre el nombre del archivo, de modo que un mismo CSV no se puede cargar dos veces. Procesar la misma fecha varias veces es seguro.
- **Estado en el frontend:** *signals* y componentes standalone, con el acceso a la API aislado en `CommerceService`.

## Datos de prueba

`samples/commerce_07102026.csv` contiene 7 registros: 3 válidos y 4 con errores. Resultado esperado: carga de 7, proceso de `2026-10-07` envía 4 registros a cuarentena.

## Mejoras posibles

- Inserción por lotes (por ejemplo `SqlBulkCopy` o un parámetro de tabla) para archivos muy grandes.
- Pantalla de inicio de sesión con JWT.
- Pruebas unitarias del servicio y del frontend.
