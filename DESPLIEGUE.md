# 🚀 Guía de Despliegue - Clientes API

Esta guía proporciona instrucciones detalladas para desplegar la API en diferentes entornos de hosting.

## 📋 Tabla de Contenidos

1. [Azure App Service](#azure-app-service)
2. [IIS (Internet Information Services)](#iis-internet-information-services)
3. [Docker](#docker)
4. [Linux con Nginx](#linux-con-nginx)
5. [Verificación Post-Despliegue](#verificación-post-despliegue)

---

## 🔵 Azure App Service

### Requisitos Previos
- Cuenta de Azure activa
- Azure CLI instalado
- SQL Server en Azure o Azure SQL Database

### Paso 1: Crear Recursos en Azure

```bash
# Login en Azure
az login

# Crear grupo de recursos
az group create --name ClientesAPI-RG --location eastus

# Crear plan de App Service
az appservice plan create --name ClientesAPI-Plan --resource-group ClientesAPI-RG --sku B1 --is-linux

# Crear Web App
az webapp create --name clientesapi-uninorte --resource-group ClientesAPI-RG --plan ClientesAPI-Plan --runtime "DOTNETCORE:8.0"
```

### Paso 2: Crear Base de Datos Azure SQL

```bash
# Crear SQL Server
az sql server create --name clientesapi-sqlserver --resource-group ClientesAPI-RG --location eastus --admin-user sqladmin --admin-password TuPassword123!

# Crear Base de Datos
az sql db create --resource-group ClientesAPI-RG --server clientesapi-sqlserver --name ClientesDB --service-objective S0

# Permitir acceso de servicios Azure
az sql server firewall-rule create --resource-group ClientesAPI-RG --server clientesapi-sqlserver --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
```

### Paso 3: Configurar Connection String

```bash
# Cadena de conexión
CONNECTION_STRING="Server=tcp:clientesapi-sqlserver.database.windows.net,1433;Initial Catalog=ClientesDB;Persist Security Info=False;User ID=sqladmin;Password=TuPassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Configurar en App Service
az webapp config connection-string set --resource-group ClientesAPI-RG --name clientesapi-uninorte --settings DefaultConnection="$CONNECTION_STRING" --connection-string-type SQLAzure
```

### Paso 4: Ejecutar Script de Base de Datos

Conectarse a Azure SQL Database usando SQL Server Management Studio o Azure Data Studio y ejecutar `DatabaseScript.sql`.

### Paso 5: Publicar la API

```bash
# Compilar y publicar
cd ClientesAPI
dotnet publish -c Release -o ./publish

# Crear archivo ZIP
cd publish
zip -r ../publish.zip *
cd ..

# Desplegar
az webapp deployment source config-zip --resource-group ClientesAPI-RG --name clientesapi-uninorte --src publish.zip
```

### Paso 6: Verificar Despliegue

```bash
# Abrir en navegador
az webapp browse --name clientesapi-uninorte --resource-group ClientesAPI-RG
```

URL: `https://clientesapi-uninorte.azurewebsites.net`

---

## 🖥️ IIS (Internet Information Services)

### Requisitos Previos
- Windows Server 2019 o superior
- IIS instalado con módulo ASP.NET Core
- .NET 8.0 Hosting Bundle instalado
- SQL Server instalado localmente o accesible

### Paso 1: Instalar .NET Hosting Bundle

Descargar e instalar desde: https://dotnet.microsoft.com/download/dotnet/8.0

```powershell
# Verificar instalación
dotnet --info
```

### Paso 2: Compilar la Aplicación

```powershell
cd ClientesAPI
dotnet publish -c Release -o C:\inetpub\ClientesAPI
```

### Paso 3: Configurar IIS

1. Abrir **IIS Manager**
2. Click derecho en **Sites** → **Add Website**
3. Configurar:
   - **Site name:** ClientesAPI
   - **Physical path:** C:\inetpub\ClientesAPI
   - **Port:** 80 (o 443 para HTTPS)
   - **Host name:** clientesapi.midominio.com

### Paso 4: Configurar Application Pool

1. En IIS Manager, ir a **Application Pools**
2. Seleccionar el pool de ClientesAPI
3. Click derecho → **Basic Settings**
4. Configurar:
   - **.NET CLR version:** No Managed Code
   - **Managed pipeline mode:** Integrated

### Paso 5: Configurar Connection String

Editar `C:\inetpub\ClientesAPI\appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ClientesDB;User Id=sa;Password=TuPassword;TrustServerCertificate=true"
  }
}
```

### Paso 6: Configurar Permisos

```powershell
# Dar permisos a la carpeta UploadedFiles
icacls "C:\inetpub\ClientesAPI\UploadedFiles" /grant "IIS AppPool\ClientesAPI:(OI)(CI)F" /T
```

### Paso 7: Crear Base de Datos

Ejecutar `DatabaseScript.sql` en SQL Server Management Studio.

### Paso 8: Reiniciar IIS

```powershell
iisreset
```

Acceder a: `http://localhost` o `http://clientesapi.midominio.com`

---

## 🐳 Docker

### Paso 1: Crear Dockerfile

Crear archivo `Dockerfile` en la raíz del proyecto:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ClientesAPI/ClientesAPI.csproj", "ClientesAPI/"]
RUN dotnet restore "ClientesAPI/ClientesAPI.csproj"
COPY . .
WORKDIR "/src/ClientesAPI"
RUN dotnet build "ClientesAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ClientesAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClientesAPI.dll"]
```

### Paso 2: Crear docker-compose.yml

```yaml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=ClientesDB;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true
    depends_on:
      - sqlserver
    volumes:
      - ./UploadedFiles:/app/UploadedFiles

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql

volumes:
  sqldata:
```

### Paso 3: Construir y Ejecutar

```bash
# Construir imagen
docker-compose build

# Ejecutar contenedores
docker-compose up -d

# Ver logs
docker-compose logs -f api

# Ejecutar script de BD
docker exec -it <container_id> /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourPassword123!' -i /app/DatabaseScript.sql
```

Acceder a: `http://localhost:5000`

### Paso 4: Detener Contenedores

```bash
docker-compose down
```

---

## 🐧 Linux con Nginx

### Requisitos Previos
- Ubuntu 22.04 LTS o superior
- .NET 8.0 SDK instalado
- Nginx instalado
- SQL Server para Linux o conexión a SQL Server remoto

### Paso 1: Instalar .NET 8.0

```bash
# Descargar script de instalación
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh

# Instalar .NET 8.0
./dotnet-install.sh --channel 8.0

# Agregar al PATH
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc

# Verificar
dotnet --version
```

### Paso 2: Publicar la Aplicación

```bash
cd ClientesAPI
dotnet publish -c Release -o /var/www/clientesapi
```

### Paso 3: Configurar Servicio systemd

Crear archivo `/etc/systemd/system/clientesapi.service`:

```ini
[Unit]
Description=Clientes API .NET Web API
After=network.target

[Service]
WorkingDirectory=/var/www/clientesapi
ExecStart=/home/usuario/.dotnet/dotnet /var/www/clientesapi/ClientesAPI.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=clientesapi
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

### Paso 4: Habilitar y Iniciar Servicio

```bash
# Recargar systemd
sudo systemctl daemon-reload

# Habilitar servicio
sudo systemctl enable clientesapi

# Iniciar servicio
sudo systemctl start clientesapi

# Ver estado
sudo systemctl status clientesapi

# Ver logs
sudo journalctl -fu clientesapi
```

### Paso 5: Configurar Nginx

Crear archivo `/etc/nginx/sites-available/clientesapi`:

```nginx
server {
    listen 80;
    server_name clientesapi.midominio.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # Configurar límites para archivos grandes
        client_max_body_size 100M;
        proxy_read_timeout 300s;
        proxy_connect_timeout 75s;
    }
}
```

### Paso 6: Habilitar Sitio

```bash
# Crear enlace simbólico
sudo ln -s /etc/nginx/sites-available/clientesapi /etc/nginx/sites-enabled/

# Probar configuración
sudo nginx -t

# Reiniciar Nginx
sudo systemctl restart nginx
```

### Paso 7: Configurar Firewall

```bash
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable
```

### Paso 8: Configurar SSL con Let's Encrypt (Opcional)

```bash
# Instalar Certbot
sudo apt install certbot python3-certbot-nginx

# Obtener certificado
sudo certbot --nginx -d clientesapi.midominio.com

# Renovación automática (ya configurado por Certbot)
sudo certbot renew --dry-run
```

Acceder a: `http://clientesapi.midominio.com`

---

## ✅ Verificación Post-Despliegue

### Checklist de Verificación

- [ ] API responde en URL de producción
- [ ] Swagger UI accesible
- [ ] Endpoint /health retorna 200 OK
- [ ] Conexión a base de datos exitosa
- [ ] Registro de clientes funciona
- [ ] Carga de archivos ZIP funciona
- [ ] Carpeta UploadedFiles tiene permisos correctos
- [ ] Logs se registran en base de datos
- [ ] Consultas GET funcionan correctamente
- [ ] Fotografías se recuperan correctamente

### Scripts de Verificación

#### Test de Health Check
```bash
curl -X GET https://tu-dominio.com/health
```

Respuesta esperada:
```json
{
  "status": "healthy",
  "timestamp": "2024-11-10T12:00:00",
  "version": "1.0.0"
}
```

#### Test de Registro de Cliente
```bash
curl -X POST "https://tu-dominio.com/api/clientes/registrar" \
  -H "Content-Type: multipart/form-data" \
  -F "CI=1111111111" \
  -F "Nombres=Cliente Prueba" \
  -F "Direccion=Direccion Test" \
  -F "Telefono=70000000"
```

#### Test de Consulta
```bash
curl -X GET "https://tu-dominio.com/api/clientes/1111111111"
```

#### Test de Logs
```bash
curl -X GET "https://tu-dominio.com/api/logs?limite=10"
```

### Monitoreo

#### Ver Logs en Tiempo Real (Linux)
```bash
sudo journalctl -fu clientesapi
```

#### Ver Logs (Azure)
```bash
az webapp log tail --name clientesapi-uninorte --resource-group ClientesAPI-RG
```

#### Ver Logs (IIS)
```powershell
Get-Content "C:\inetpub\ClientesAPI\logs\*.log" -Tail 50 -Wait
```

---

## 🔧 Troubleshooting

### Error: "Connection to database failed"
**Solución:** Verificar cadena de conexión y que SQL Server acepta conexiones remotas.

### Error: "Cannot write to UploadedFiles"
**Solución:** Verificar permisos de escritura en la carpeta.

### Error: "502 Bad Gateway" (Nginx)
**Solución:** Verificar que el servicio de la API está corriendo:
```bash
sudo systemctl status clientesapi
```

### Error: "HTTP Error 500.31" (IIS)
**Solución:** Verificar que .NET Hosting Bundle está instalado correctamente.

---

## 📊 Recomendaciones de Producción

1. **Seguridad:**
   - Usar HTTPS con certificados SSL válidos
   - Implementar autenticación y autorización
   - No exponer cadenas de conexión en appsettings.json (usar Azure Key Vault o variables de entorno)

2. **Performance:**
   - Habilitar compresión de respuestas
   - Implementar cache donde sea apropiado
   - Configurar límites de tamaño de archivos

3. **Monitoreo:**
   - Implementar Application Insights (Azure)
   - Configurar alertas para errores 500
   - Monitorear uso de CPU y memoria

4. **Backup:**
   - Configurar backups automáticos de SQL Server
   - Backup de carpeta UploadedFiles
   - Plan de recuperación ante desastres

5. **Escalabilidad:**
   - Considerar Azure App Service con escalado automático
   - Usar Azure Blob Storage para archivos grandes
   - Implementar CDN para contenido estático

---

**Fecha:** Noviembre 2024  
**Versión:** 1.0.0  
**Autor:** Lenguajes Visuales II - UNINORTE
