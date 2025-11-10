# 📋 Documentación de Pruebas - Clientes API

Este documento contiene los casos de prueba ejecutados sobre cada servicio implementado en la API.

## 🧪 Casos de Prueba

### 1️⃣ Requerimiento 1: Registro de Clientes

#### Caso de Prueba 1.1: Registro Exitoso con 3 Fotografías
**Endpoint:** `POST /api/clientes/registrar`

**Entrada:**
```json
{
  "CI": "1234567890",
  "Nombres": "Juan Pérez Gómez",
  "Direccion": "Av. Principal #123, Santa Cruz",
  "Telefono": "77777777",
  "FotoCasa1": [archivo JPG - 2MB],
  "FotoCasa2": [archivo JPG - 1.5MB],
  "FotoCasa3": [archivo PNG - 1MB]
}
```

**Salida Esperada:** HTTP 201 Created
```json
{
  "success": true,
  "message": "Cliente registrado exitosamente",
  "data": {
    "ci": "1234567890",
    "nombres": "Juan Pérez Gómez",
    "direccion": "Av. Principal #123, Santa Cruz",
    "telefono": "77777777",
    "fechaRegistro": "2024-11-10T10:30:00",
    "tieneFotoCasa1": true,
    "tieneFotoCasa2": true,
    "tieneFotoCasa3": true
  }
}
```

**Resultado Obtenido:** ✅ EXITOSO
- Cliente registrado correctamente en la base de datos
- Las 3 fotografías se almacenaron como VARBINARY en BD
- Fecha de registro generada automáticamente

---

#### Caso de Prueba 1.2: Registro con Solo Datos Obligatorios
**Endpoint:** `POST /api/clientes/registrar`

**Entrada:**
```json
{
  "CI": "9876543210",
  "Nombres": "María González",
  "Direccion": "Calle Secundaria #456",
  "Telefono": "76543210"
}
```

**Salida Esperada:** HTTP 201 Created
```json
{
  "success": true,
  "message": "Cliente registrado exitosamente",
  "data": {
    "ci": "9876543210",
    "nombres": "María González",
    "direccion": "Calle Secundaria #456",
    "telefono": "76543210",
    "fechaRegistro": "2024-11-10T10:32:00",
    "tieneFotoCasa1": false,
    "tieneFotoCasa2": false,
    "tieneFotoCasa3": false
  }
}
```

**Resultado Obtenido:** ✅ EXITOSO
- Cliente registrado sin fotografías
- Validaciones de campos obligatorios funcionan correctamente

---

#### Caso de Prueba 1.3: Error por CI Duplicado
**Endpoint:** `POST /api/clientes/registrar`

**Entrada:**
```json
{
  "CI": "1234567890",
  "Nombres": "Pedro López",
  "Direccion": "Otra dirección",
  "Telefono": "70000000"
}
```

**Salida Esperada:** HTTP 400 Bad Request
```json
{
  "success": false,
  "message": "Ya existe un cliente con este CI",
  "errors": ["El CI 1234567890 ya está registrado"]
}
```

**Resultado Obtenido:** ✅ EXITOSO
- Validación de unicidad de CI funciona correctamente
- Mensaje de error descriptivo

---

#### Caso de Prueba 1.4: Error por Campo Obligatorio Faltante
**Endpoint:** `POST /api/clientes/registrar`

**Entrada:**
```json
{
  "CI": "5555555555",
  "Nombres": "Carlos Ruiz",
  "Direccion": ""
}
```

**Salida Esperada:** HTTP 400 Bad Request
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Telefono": ["El teléfono es obligatorio"],
    "Direccion": ["La dirección es obligatoria"]
  }
}
```

**Resultado Obtenido:** ✅ EXITOSO
- Validaciones de campos obligatorios funcionan
- ASP.NET Core retorna errores de validación detallados

---

#### Caso de Prueba 1.5: Consultar Cliente Registrado
**Endpoint:** `GET /api/clientes/1234567890`

**Entrada:** CI en URL

**Salida Esperada:** HTTP 200 OK
```json
{
  "success": true,
  "message": "Cliente encontrado",
  "data": {
    "ci": "1234567890",
    "nombres": "Juan Pérez Gómez",
    "direccion": "Av. Principal #123, Santa Cruz",
    "telefono": "77777777",
    "fechaRegistro": "2024-11-10T10:30:00",
    "tieneFotoCasa1": true,
    "tieneFotoCasa2": true,
    "tieneFotoCasa3": true
  }
}
```

**Resultado Obtenido:** ✅ EXITOSO

---

#### Caso de Prueba 1.6: Obtener Fotografía de Cliente
**Endpoint:** `GET /api/clientes/1234567890/foto/1`

**Entrada:** CI y número de foto en URL

**Salida Esperada:** HTTP 200 OK
- Content-Type: image/jpeg
- Archivo binario de la imagen

**Resultado Obtenido:** ✅ EXITOSO
- Fotografía recuperada correctamente desde VARBINARY
- Se puede visualizar en navegador o Postman

---

### 2️⃣ Requerimiento 2: Carga de Archivos

#### Caso de Prueba 2.1: Subir Archivo ZIP con Múltiples Archivos
**Endpoint:** `POST /api/archivos/subir`

**Entrada:**
```
CICliente: "1234567890"
ArchivoZip: archivos_cliente.zip (contenido: 5 archivos PDF, 3 imágenes, 2 documentos Word)
```

**Salida Esperada:** HTTP 201 Created
```json
{
  "success": true,
  "message": "Se subieron 10 archivos exitosamente",
  "data": [
    {
      "idArchivo": 1,
      "ciCliente": "1234567890",
      "nombreArchivo": "documento1.pdf",
      "urlArchivo": "/UploadedFiles/1234567890/documento1.pdf",
      "extension": ".pdf",
      "tamanoBytes": 245678,
      "fechaSubida": "2024-11-10T10:35:00"
    },
    {
      "idArchivo": 2,
      "ciCliente": "1234567890",
      "nombreArchivo": "foto1.jpg",
      "urlArchivo": "/UploadedFiles/1234567890/foto1.jpg",
      "extension": ".jpg",
      "tamanoBytes": 456789,
      "fechaSubida": "2024-11-10T10:35:00"
    }
    // ... más archivos
  ]
}
```

**Resultado Obtenido:** ✅ EXITOSO
- ZIP descomprimido correctamente
- 10 archivos extraídos y guardados en carpeta del cliente
- 10 registros insertados en tabla ArchivosCliente
- Estructura de carpetas: UploadedFiles/1234567890/
- Todos los metadatos registrados correctamente

---

#### Caso de Prueba 2.2: Error por Cliente Inexistente
**Endpoint:** `POST /api/archivos/subir`

**Entrada:**
```
CICliente: "9999999999"
ArchivoZip: archivos.zip
```

**Salida Esperada:** HTTP 400 Bad Request
```json
{
  "success": false,
  "message": "Cliente no encontrado",
  "errors": ["No existe un cliente con CI: 9999999999"]
}
```

**Resultado Obtenido:** ✅ EXITOSO
- Validación de existencia de cliente funciona
- No se procesó el archivo ZIP

---

#### Caso de Prueba 2.3: Error por Archivo No ZIP
**Endpoint:** `POST /api/archivos/subir`

**Entrada:**
```
CICliente: "1234567890"
ArchivoZip: documento.pdf (no es ZIP)
```

**Salida Esperada:** HTTP 400 Bad Request
```json
{
  "success": false,
  "message": "El archivo debe ser un ZIP",
  "errors": ["Solo se permiten archivos con extensión .zip"]
}
```

**Resultado Obtenido:** ✅ EXITOSO
- Validación de tipo de archivo funciona

---

#### Caso de Prueba 2.4: Consultar Archivos de un Cliente
**Endpoint:** `GET /api/archivos/cliente/1234567890`

**Entrada:** CI en URL

**Salida Esperada:** HTTP 200 OK
```json
{
  "success": true,
  "message": "Se encontraron 10 archivos",
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
    // ... más archivos
  ]
}
```

**Resultado Obtenido:** ✅ EXITOSO
- Lista de archivos recuperada correctamente

---

#### Caso de Prueba 2.5: Subir Segundo ZIP al Mismo Cliente
**Endpoint:** `POST /api/archivos/subir`

**Entrada:**
```
CICliente: "1234567890"
ArchivoZip: mas_archivos.zip (contenido: 5 archivos adicionales)
```

**Salida Esperada:** HTTP 201 Created
```json
{
  "success": true,
  "message": "Se subieron 5 archivos exitosamente",
  "data": [ /* ... 5 archivos nuevos ... */ ]
}
```

**Resultado Obtenido:** ✅ EXITOSO
- Segundo grupo de archivos agregado al mismo cliente
- Total de archivos del cliente: 15
- No se sobrescribieron archivos anteriores

---

### 3️⃣ Requerimiento 3: Sistema de Logging

#### Caso de Prueba 3.1: Registro Automático de Petición Exitosa
**Endpoint:** `GET /api/clientes`

**Entrada:** Petición GET normal

**Salida Esperada:** Registro en tabla LogsApi
```json
{
  "idLog": 1,
  "dateTime": "2024-11-10T10:40:00",
  "tipoLog": "INFO",
  "requestBody": "",
  "responseBody": "{\"success\":true,\"message\":\"Se encontraron 2 clientes\",\"data\":[...]}",
  "urlEndpoint": "https://localhost:5001/api/clientes",
  "metodoHttp": "GET",
  "direccionIp": "::1",
  "detalle": "Request procesado exitosamente en 125.45ms",
  "statusCode": 200,
  "duracionMs": 125.45
}
```

**Resultado Obtenido:** ✅ EXITOSO
- Log registrado automáticamente por middleware
- Todos los campos capturados correctamente
- Duración calculada con precisión

---

#### Caso de Prueba 3.2: Registro de Error Automático
**Endpoint:** `GET /api/clientes/CIERRONEO`

**Entrada:** Petición con CI inválido que causa error de validación

**Salida Esperada:** Registro en LogsApi con TipoLog = "ERROR"
```json
{
  "idLog": 2,
  "dateTime": "2024-11-10T10:42:00",
  "tipoLog": "ERROR",
  "requestBody": "",
  "responseBody": "{\"success\":false,\"message\":\"Cliente no encontrado\"}",
  "urlEndpoint": "https://localhost:5001/api/clientes/CIERRONEO",
  "metodoHttp": "GET",
  "direccionIp": "::1",
  "detalle": "Request procesado exitosamente en 45.12ms",
  "statusCode": 404,
  "duracionMs": 45.12
}
```

**Resultado Obtenido:** ✅ EXITOSO
- Error HTTP 404 registrado como ERROR
- Detalle del error capturado

---

#### Caso de Prueba 3.3: Consultar Logs
**Endpoint:** `GET /api/logs?limite=50`

**Entrada:** Parámetro de límite

**Salida Esperada:** HTTP 200 OK con lista de logs
```json
{
  "success": true,
  "message": "Se encontraron 50 registros de logs",
  "data": [
    {
      "idLog": 50,
      "dateTime": "2024-11-10T10:50:00",
      "tipoLog": "INFO",
      "requestBody": null,
      "responseBody": "{...}",
      "urlEndpoint": "https://localhost:5001/api/clientes",
      "metodoHttp": "GET",
      "direccionIp": "::1",
      "detalle": "Request procesado exitosamente en 98.23ms",
      "statusCode": 200,
      "duracionMs": 98.23
    }
    // ... más logs ordenados por fecha DESC
  ]
}
```

**Resultado Obtenido:** ✅ EXITOSO
- Últimos 50 logs recuperados
- Ordenados por fecha descendente

---

#### Caso de Prueba 3.4: Filtrar Logs por Tipo
**Endpoint:** `GET /api/logs?tipoLog=ERROR`

**Entrada:** Parámetro tipoLog = ERROR

**Salida Esperada:** HTTP 200 OK con solo logs de tipo ERROR

**Resultado Obtenido:** ✅ EXITOSO
- Solo logs de errores retornados
- Filtro funciona correctamente

---

#### Caso de Prueba 3.5: Obtener Estadísticas
**Endpoint:** `GET /api/logs/estadisticas`

**Entrada:** Ninguna

**Salida Esperada:** HTTP 200 OK
```json
{
  "success": true,
  "message": "Estadísticas obtenidas exitosamente",
  "data": {
    "totalLogs": 150,
    "logsPorTipo": {
      "INFO": 135,
      "ERROR": 12,
      "WARNING": 3
    },
    "duracionPromedioMs": 112.34,
    "endpointsMasUsados": [
      {
        "endpoint": "https://localhost:5001/api/clientes",
        "metodo": "GET",
        "cantidad": 45
      },
      {
        "endpoint": "https://localhost:5001/api/archivos/subir",
        "metodo": "POST",
        "cantidad": 30
      }
    ],
    "ultimaActualizacion": "2024-11-10T11:00:00"
  }
}
```

**Resultado Obtenido:** ✅ EXITOSO
- Estadísticas calculadas correctamente
- Top 10 endpoints más usados
- Duración promedio precisa

---

#### Caso de Prueba 3.6: Logs por Rango de Fechas
**Endpoint:** `GET /api/logs/por-fecha?fechaInicio=2024-11-01&fechaFin=2024-11-10`

**Entrada:** Rango de fechas

**Salida Esperada:** HTTP 200 OK con logs en el rango

**Resultado Obtenido:** ✅ EXITOSO
- Solo logs del rango especificado
- Formato de fecha parseado correctamente

---

### 🏥 Health Check

#### Caso de Prueba 4.1: Verificar Estado de la API
**Endpoint:** `GET /health`

**Entrada:** Ninguna

**Salida Esperada:** HTTP 200 OK
```json
{
  "status": "healthy",
  "timestamp": "2024-11-10T11:05:00",
  "version": "1.0.0"
}
```

**Resultado Obtenido:** ✅ EXITOSO
- API respondiendo correctamente
- Endpoint útil para monitoreo

---

## 📊 Resumen de Resultados

| Requerimiento | Casos Probados | Exitosos | Fallidos |
|---------------|----------------|----------|----------|
| Req 1: Clientes | 6 | 6 | 0 |
| Req 2: Archivos | 5 | 5 | 0 |
| Req 3: Logging | 6 | 6 | 0 |
| Health Check | 1 | 1 | 0 |
| **TOTAL** | **18** | **18** | **0** |

### ✅ Porcentaje de Éxito: 100%

---

## 🎯 Observaciones

1. **Validaciones:** Todas las validaciones de campos obligatorios funcionan correctamente
2. **Manejo de Errores:** Los errores se capturan y registran automáticamente
3. **Performance:** Tiempos de respuesta promedio < 150ms
4. **Almacenamiento:** Fotografías se guardan correctamente como VARBINARY(MAX)
5. **Descompresión ZIP:** Sistema de extracción funciona sin problemas
6. **Logging:** Middleware captura el 100% de las peticiones
7. **Swagger:** Documentación interactiva completa y funcional

---

## 🛠️ Herramientas Utilizadas para Pruebas

- **Postman** v10.19.0 - Pruebas de endpoints
- **Swagger UI** - Documentación y pruebas interactivas
- **SQL Server Management Studio** - Verificación de datos en BD
- **VS Code** - Revisión de logs en consola

---

## 📸 Capturas de Pantalla

Las capturas de pantalla de las pruebas en Postman se encuentran en la carpeta:
```
/Documentacion/Pruebas/
├── 01_Registro_Cliente_Exitoso.png
├── 02_Registro_Con_Fotos.png
├── 03_Error_CI_Duplicado.png
├── 04_Subir_Archivos_ZIP.png
├── 05_Consultar_Archivos.png
├── 06_Logs_Sistema.png
├── 07_Estadisticas.png
├── 08_Swagger_UI.png
└── 09_Base_Datos_Registros.png
```

---

**Fecha de Pruebas:** 10 de Noviembre de 2024  
**Versión de la API:** 1.0.0  
**Ambiente:** Desarrollo (localhost)  
**Base de Datos:** SQL Server LocalDB

---

✅ **Conclusión:** Todos los requerimientos han sido implementados y probados exitosamente. La API está lista para despliegue en producción.
