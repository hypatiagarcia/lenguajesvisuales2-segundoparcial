# Clientes API - Sistema de Gestión de Clientes y Archivos

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?style=flat&logo=microsoft-sql-server)](https://www.microsoft.com/sql-server)
[![Swagger](https://img.shields.io/badge/Swagger-API%20Docs-85EA2D?style=flat&logo=swagger)](https://swagger.io/)

API REST desarrollada con ASP.NET Core Web API (.NET 8.0) y SQL Server bajo el enfoque Code First de Entity Framework Core para la gestión integral de clientes y sus archivos asociados.

## 📋 Descripción

Sistema completo que permite:
- ✅ Registro de clientes con información personal y fotografías
- 📦 Carga masiva de archivos mediante archivos ZIP
- 📊 Sistema automático de logging y auditoría
- 🔍 Consulta y seguimiento de operaciones
- 📈 Estadísticas de uso de la API

## 🚀 Características Principales

### ✨ Requerimiento 1: Registro de Clientes
- **Endpoint:** `POST /api/clientes/registrar`
- Registro de información completa del cliente (CI, Nombres, Dirección, Teléfono)
- Almacenamiento de hasta 3 fotografías en formato binario en la base de datos
- Validaciones de datos obligatorios
- Respuestas estructuradas con información detallada

### 📁 Requerimiento 2: Carga de Archivos
- **Endpoint:** `POST /api/archivos/subir`
- Recepción de archivos comprimidos en formato ZIP
- Descompresión automática del archivo
- Almacenamiento organizado en carpetas por cliente
- Registro de metadatos en base de datos (nombre, URL, extensión, tamaño)
- Soporte para múltiples tipos de archivos (imágenes, documentos, videos)

### 📝 Requerimiento 3: Sistema de Logging
- **Endpoint:** `GET /api/logs`
- Middleware global que registra todas las peticiones
- Captura automática de errores y excepciones
- Información registrada:
  - Fecha y hora de la petición
  - Tipo de log (INFO, ERROR, WARNING)
  - Request y Response bodies
  - URL del endpoint
  - Método HTTP
  - Dirección IP del cliente
  - Código de estado HTTP
  - Duración de la petición en milisegundos
- Endpoints adicionales:
  - `GET /api/logs/{id}` - Consultar log específico
  - `GET /api/logs/estadisticas` - Estadísticas de uso
  - `GET /api/logs/por-fecha` - Filtrar por rango de fechas

## 🛠️ Tecnologías Utilizadas

- **Framework:** ASP.NET Core Web API 8.0
- **ORM:** Entity Framework Core 9.0
- **Base de Datos:** SQL Server
- **Documentación:** Swagger/OpenAPI
- **Logging:** Middleware personalizado + ILogger
- **Manejo de Archivos:** System.IO.Compression

## 📦 Dependencias

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.10" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.10" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.10" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
```

## 🏗️ Estructura del Proyecto

```
ClientesAPI/
├── Controllers/           # Controladores de la API
│   ├── ClientesController.cs
│   ├── ArchivosController.cs
│   └── LogsController.cs
├── Models/               # Entidades del dominio
│   ├── Cliente.cs
│   ├── ArchivoCliente.cs
│   └── LogApi.cs
├── Data/                 # Contexto de base de datos
│   └── ApplicationDbContext.cs
├── DTOs/                 # Data Transfer Objects
│   └── DTOs.cs
├── Services/             # Servicios de negocio
│   └── FileService.cs
├── Middleware/           # Middleware personalizado
│   └── ApiLoggingMiddleware.cs
├── UploadedFiles/        # Archivos subidos por clientes
├── Program.cs            # Configuración de la aplicación
├── appsettings.json      # Configuración general
└── DatabaseScript.sql    # Script de creación de BD
```

## 🗄️ Modelo de Base de Datos

### Tabla: Clientes
```sql
CREATE TABLE Clientes (
    CI NVARCHAR(20) PRIMARY KEY,
    Nombres NVARCHAR(200) NOT NULL,
    Direccion NVARCHAR(500) NOT NULL,
    Telefono NVARCHAR(50) NOT NULL,
    FotoCasa1 VARBINARY(MAX),
    FotoCasa2 VARBINARY(MAX),
    FotoCasa3 VARBINARY(MAX),
    FechaRegistro DATETIME DEFAULT GETDATE()
)
```

### Tabla: ArchivosCliente
```sql
CREATE TABLE ArchivosCliente (
    IdArchivo INT IDENTITY(1,1) PRIMARY KEY,
    CICliente NVARCHAR(20) NOT NULL,
    NombreArchivo NVARCHAR(255) NOT NULL,
    UrlArchivo NVARCHAR(500) NOT NULL,
    Extension NVARCHAR(10),
    TamanoBytes BIGINT,
    FechaSubida DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CICliente) REFERENCES Clientes(CI)
)
```

### Tabla: LogsApi
```sql
CREATE TABLE LogsApi (
    IdLog INT IDENTITY(1,1) PRIMARY KEY,
    DateTime DATETIME DEFAULT GETDATE(),
    TipoLog NVARCHAR(50) NOT NULL,
    RequestBody NVARCHAR(MAX),
    ResponseBody NVARCHAR(MAX),
    UrlEndpoint NVARCHAR(500),
    MetodoHttp NVARCHAR(10),
    DireccionIp NVARCHAR(50),
    Detalle NVARCHAR(MAX),
    StatusCode INT,
    DuracionMs FLOAT
)
```

## ⚙️ Instalación y Configuración

### Prerrequisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server 2019+](https://www.microsoft.com/sql-server) o SQL Server Express
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o VS Code
- [Postman](https://www.postman.com/) o similar para pruebas

### Pasos de Instalación

1. **Clonar el repositorio**
```bash
git clone https://github.com/tu-usuario/lenguajesvisuales2-segundoparcial.git
cd lenguajesvisuales2-segundoparcial/ClientesAPI
```

2. **Restaurar dependencias**
```bash
dotnet restore
```

3. **Configurar la cadena de conexión**

Editar `appsettings.json` con tu servidor SQL Server:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=ClientesDB;User Id=tu_usuario;Password=tu_password;TrustServerCertificate=true"
  }
}
```

Para SQL Server LocalDB (desarrollo local):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ClientesDB;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

4. **Crear la base de datos**

Opción A - Usando EF Core:
```bash
dotnet ef database update
```

Opción B - Usando el script SQL:
```bash
# Ejecutar DatabaseScript.sql en SQL Server Management Studio
```

5. **Ejecutar la aplicación**
```bash
dotnet run
```

La API estará disponible en:
- **HTTP:** http://localhost:5000
- **HTTPS:** https://localhost:5001
- **Swagger UI:** https://localhost:5001 (página principal)

## 📡 Endpoints de la API

### 👤 Clientes

#### Registrar Cliente
```http
POST /api/clientes/registrar
Content-Type: multipart/form-data

CI: string (requerido)
Nombres: string (requerido)
Direccion: string (requerido)
Telefono: string (requerido)
FotoCasa1: file (opcional)
FotoCasa2: file (opcional)
FotoCasa3: file (opcional)
```

**Respuesta exitosa (201):**
```json
{
  "success": true,
  "message": "Cliente registrado exitosamente",
  "data": {
    "ci": "1234567890",
    "nombres": "Juan Pérez",
    "direccion": "Av. Principal #123",
    "telefono": "77777777",
    "fechaRegistro": "2024-11-10T10:30:00",
    "tieneFotoCasa1": true,
    "tieneFotoCasa2": true,
    "tieneFotoCasa3": false
  }
}
```

#### Obtener Cliente
```http
GET /api/clientes/{ci}
```

#### Listar Todos los Clientes
```http
GET /api/clientes
```

#### Obtener Fotografía
```http
GET /api/clientes/{ci}/foto/{numeroFoto}
```

### 📁 Archivos

#### Subir Archivos ZIP
```http
POST /api/archivos/subir
Content-Type: multipart/form-data

CICliente: string (requerido)
ArchivoZip: file (requerido, .zip)
```

**Respuesta exitosa (201):**
```json
{
  "success": true,
  "message": "Se subieron 5 archivos exitosamente",
  "data": [
    {
      "idArchivo": 1,
      "ciCliente": "1234567890",
      "nombreArchivo": "documento1.pdf",
      "urlArchivo": "/UploadedFiles/1234567890/documento1.pdf",
      "extension": ".pdf",
      "tamanoBytes": 245678,
      "fechaSubida": "2024-11-10T10:35:00"
    }
  ]
}
```

#### Obtener Archivos por Cliente
```http
GET /api/archivos/cliente/{ciCliente}
```

#### Obtener Archivo Específico
```http
GET /api/archivos/{idArchivo}
```

#### Listar Todos los Archivos
```http
GET /api/archivos
```

### 📊 Logs

#### Obtener Logs
```http
GET /api/logs?limite=100&tipoLog=ERROR
```

#### Obtener Log Específico
```http
GET /api/logs/{idLog}
```

#### Estadísticas
```http
GET /api/logs/estadisticas
```

**Respuesta:**
```json
{
  "success": true,
  "message": "Estadísticas obtenidas exitosamente",
  "data": {
    "totalLogs": 1250,
    "logsPorTipo": {
      "INFO": 1180,
      "ERROR": 65,
      "WARNING": 5
    },
    "duracionPromedioMs": 125.45,
    "endpointsMasUsados": [
      {
        "endpoint": "/api/clientes",
        "metodo": "GET",
        "cantidad": 450
      }
    ],
    "ultimaActualizacion": "2024-11-10T15:30:00"
  }
}
```

#### Logs por Fecha
```http
GET /api/logs/por-fecha?fechaInicio=2024-11-01&fechaFin=2024-11-10
```

### 🏥 Health Check
```http
GET /health
```

## 🧪 Ejemplos de Prueba con cURL

### Registrar un Cliente
```bash
curl -X POST "https://localhost:5001/api/clientes/registrar" \
  -H "Content-Type: multipart/form-data" \
  -F "CI=1234567890" \
  -F "Nombres=Juan Pérez Gómez" \
  -F "Direccion=Av. Principal #123, Santa Cruz" \
  -F "Telefono=77777777" \
  -F "FotoCasa1=@/ruta/foto1.jpg" \
  -F "FotoCasa2=@/ruta/foto2.jpg"
```

### Subir Archivos ZIP
```bash
curl -X POST "https://localhost:5001/api/archivos/subir" \
  -H "Content-Type: multipart/form-data" \
  -F "CICliente=1234567890" \
  -F "ArchivoZip=@/ruta/archivos.zip"
```

### Obtener Logs
```bash
curl -X GET "https://localhost:5001/api/logs?limite=50&tipoLog=ERROR"
```

## 🔐 Validaciones Implementadas

- ✅ Campos obligatorios en registro de clientes
- ✅ Validación de formato de archivo ZIP
- ✅ Verificación de existencia de cliente antes de subir archivos
- ✅ Validación de duplicados (CI único)
- ✅ Manejo global de excepciones
- ✅ Validación de tipos de archivo
- ✅ Control de tamaño de archivos

## 📈 Características Avanzadas

- **Retry Policy:** Reintentos automáticos en caso de fallo de conexión a BD
- **CORS:** Configurado para permitir peticiones desde cualquier origen
- **Static Files:** Servicio de archivos estáticos para acceso a uploads
- **Middleware de Logging:** Captura automática de todas las peticiones
- **Swagger UI:** Documentación interactiva en la raíz de la aplicación
- **Health Check:** Endpoint para verificar estado de la API

## 🌐 Publicación en Hosting

### Preparación para Producción

1. **Compilar en modo Release**
```bash
dotnet publish -c Release -o ./publish
```

2. **Configurar cadena de conexión de producción**

Editar `appsettings.json` en la carpeta publish:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=servidor-produccion.database.windows.net;Database=ClientesDB;User Id=admin;Password=password;TrustServerCertificate=true"
  }
}
```

3. **Ejecutar script SQL en servidor remoto**
- Conectarse al servidor SQL de producción
- Ejecutar `DatabaseScript.sql`

4. **Subir archivos al hosting**
- Subir carpeta `publish` al servidor
- Configurar IIS o servidor web correspondiente

### Servicios de Hosting Recomendados

- **Azure App Service** (recomendado para .NET)
- **AWS Elastic Beanstalk**
- **Google Cloud Run**
- **DigitalOcean App Platform**
- **Heroku** (con contenedor Docker)

### Ejemplo con Azure

```bash
# Instalar Azure CLI
az login

# Crear App Service
az webapp create --name clientesapi --resource-group miGrupo --plan miPlan

# Publicar
dotnet publish -c Release
cd bin/Release/net8.0/publish
zip -r publish.zip *
az webapp deployment source config-zip --resource-group miGrupo --name clientesapi --src publish.zip
```

## 📊 Diagrama de Arquitectura

```
┌─────────────┐
│   Cliente   │
│  (Postman/  │
│   Swagger)  │
└──────┬──────┘
       │
       ▼
┌─────────────────────────────────────┐
│      ASP.NET Core Web API           │
│  ┌──────────────────────────────┐   │
│  │  Controllers Layer           │   │
│  │  - ClientesController        │   │
│  │  - ArchivosController        │   │
│  │  - LogsController            │   │
│  └────────┬─────────────────────┘   │
│           ▼                          │
│  ┌──────────────────────────────┐   │
│  │  Middleware                  │   │
│  │  - ApiLoggingMiddleware      │   │
│  └────────┬─────────────────────┘   │
│           ▼                          │
│  ┌──────────────────────────────┐   │
│  │  Services Layer              │   │
│  │  - FileService               │   │
│  └────────┬─────────────────────┘   │
│           ▼                          │
│  ┌──────────────────────────────┐   │
│  │  Data Layer                  │   │
│  │  - ApplicationDbContext      │   │
│  │  - EF Core                   │   │
│  └────────┬─────────────────────┘   │
└───────────┼─────────────────────────┘
            ▼
    ┌───────────────┐       ┌──────────────┐
    │  SQL Server   │       │  File System │
    │   Database    │       │  (Uploads)   │
    └───────────────┘       └──────────────┘
```

## 🐛 Solución de Problemas

### Error: "Cannot connect to SQL Server"
**Solución:** Verificar que SQL Server esté ejecutándose y la cadena de conexión sea correcta.

### Error: "The entity type requires a primary key"
**Solución:** Ejecutar `dotnet ef database update` o el script SQL.

### Error: "Access denied" al subir archivos
**Solución:** Verificar permisos de escritura en la carpeta `UploadedFiles`.

### Error: "Package restore failed"
**Solución:** 
```bash
dotnet nuget locals all --clear
dotnet restore
```

## 📝 Notas Importantes

- Las fotografías se almacenan en formato **VARBINARY(MAX)** en la base de datos
- Los archivos descomprimidos se guardan en **UploadedFiles/{CI_Cliente}/**
- El sistema registra **automáticamente** todas las peticiones en la tabla LogsApi
- La documentación Swagger está disponible en la **raíz** de la aplicación (/)

## 👨‍💻 Autor

**Proyecto:** Segundo Parcial - Lenguajes Visuales II  
**Universidad:** UNINORTE  
**Fecha:** Noviembre 2024

## 📄 Licencia

Este proyecto fue desarrollado con fines educativos para el curso de Lenguajes Visuales II.

## 🙏 Agradecimientos

- UNINORTE - Universidad del Norte
- Curso: Lenguajes Visuales II
- Docente: [Nombre del Docente]

---

**⚡ Para más información, consultar la documentación Swagger en https://localhost:5001**
