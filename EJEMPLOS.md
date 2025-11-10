# 📚 Ejemplos de Uso - Clientes API

Esta guía contiene ejemplos prácticos de cómo usar cada endpoint de la API.

## 🔧 Configuración Inicial

### Variables de Entorno
```bash
export API_URL="https://localhost:5001"
export API_URL_PROD="https://tu-dominio.com"
```

---

## 👤 1. Gestión de Clientes

### 1.1 Registrar Cliente Simple (sin fotos)

**cURL:**
```bash
curl -X POST "$API_URL/api/clientes/registrar" \
  -H "Content-Type: multipart/form-data" \
  -F "CI=1234567890" \
  -F "Nombres=Juan Pérez Gómez" \
  -F "Direccion=Av. Principal #123, Santa Cruz, Bolivia" \
  -F "Telefono=77777777"
```

**PowerShell:**
```powershell
$form = @{
    CI = "1234567890"
    Nombres = "Juan Pérez Gómez"
    Direccion = "Av. Principal #123, Santa Cruz, Bolivia"
    Telefono = "77777777"
}

Invoke-RestMethod -Uri "https://localhost:5001/api/clientes/registrar" `
    -Method Post `
    -Form $form
```

**JavaScript (Fetch):**
```javascript
const formData = new FormData();
formData.append('CI', '1234567890');
formData.append('Nombres', 'Juan Pérez Gómez');
formData.append('Direccion', 'Av. Principal #123, Santa Cruz, Bolivia');
formData.append('Telefono', '77777777');

fetch('https://localhost:5001/api/clientes/registrar', {
  method: 'POST',
  body: formData
})
.then(response => response.json())
.then(data => console.log(data))
.catch(error => console.error('Error:', error));
```

**Python:**
```python
import requests

url = "https://localhost:5001/api/clientes/registrar"
data = {
    'CI': '1234567890',
    'Nombres': 'Juan Pérez Gómez',
    'Direccion': 'Av. Principal #123, Santa Cruz, Bolivia',
    'Telefono': '77777777'
}

response = requests.post(url, data=data, verify=False)
print(response.json())
```

---

### 1.2 Registrar Cliente con Fotografías

**cURL:**
```bash
curl -X POST "$API_URL/api/clientes/registrar" \
  -H "Content-Type: multipart/form-data" \
  -F "CI=9876543210" \
  -F "Nombres=María González López" \
  -F "Direccion=Calle Secundaria #456, La Paz" \
  -F "Telefono=76543210" \
  -F "FotoCasa1=@/ruta/a/foto1.jpg" \
  -F "FotoCasa2=@/ruta/a/foto2.jpg" \
  -F "FotoCasa3=@/ruta/a/foto3.png"
```

**PowerShell:**
```powershell
$form = @{
    CI = "9876543210"
    Nombres = "María González López"
    Direccion = "Calle Secundaria #456, La Paz"
    Telefono = "76543210"
    FotoCasa1 = Get-Item -Path "C:\fotos\foto1.jpg"
    FotoCasa2 = Get-Item -Path "C:\fotos\foto2.jpg"
    FotoCasa3 = Get-Item -Path "C:\fotos\foto3.png"
}

Invoke-RestMethod -Uri "https://localhost:5001/api/clientes/registrar" `
    -Method Post `
    -Form $form
```

**JavaScript (con FileReader):**
```javascript
async function registrarClienteConFotos() {
  const formData = new FormData();
  formData.append('CI', '9876543210');
  formData.append('Nombres', 'María González López');
  formData.append('Direccion', 'Calle Secundaria #456, La Paz');
  formData.append('Telefono', '76543210');
  
  // Obtener archivos desde input file
  const foto1 = document.getElementById('foto1').files[0];
  const foto2 = document.getElementById('foto2').files[0];
  const foto3 = document.getElementById('foto3').files[0];
  
  formData.append('FotoCasa1', foto1);
  formData.append('FotoCasa2', foto2);
  formData.append('FotoCasa3', foto3);

  try {
    const response = await fetch('https://localhost:5001/api/clientes/registrar', {
      method: 'POST',
      body: formData
    });
    const data = await response.json();
    console.log('Cliente registrado:', data);
  } catch (error) {
    console.error('Error:', error);
  }
}
```

**Python con archivos:**
```python
import requests

url = "https://localhost:5001/api/clientes/registrar"

data = {
    'CI': '9876543210',
    'Nombres': 'María González López',
    'Direccion': 'Calle Secundaria #456, La Paz',
    'Telefono': '76543210'
}

files = {
    'FotoCasa1': open('/ruta/foto1.jpg', 'rb'),
    'FotoCasa2': open('/ruta/foto2.jpg', 'rb'),
    'FotoCasa3': open('/ruta/foto3.png', 'rb')
}

response = requests.post(url, data=data, files=files, verify=False)
print(response.json())

# Cerrar archivos
for file in files.values():
    file.close()
```

---

### 1.3 Obtener Lista de Clientes

**cURL:**
```bash
curl -X GET "$API_URL/api/clientes" \
  -H "Accept: application/json"
```

**PowerShell:**
```powershell
$response = Invoke-RestMethod -Uri "https://localhost:5001/api/clientes" `
    -Method Get `
    -ContentType "application/json"

$response.data | Format-Table CI, Nombres, Telefono
```

**JavaScript:**
```javascript
fetch('https://localhost:5001/api/clientes')
  .then(response => response.json())
  .then(data => {
    console.log(`Total de clientes: ${data.data.length}`);
    data.data.forEach(cliente => {
      console.log(`${cliente.ci} - ${cliente.nombres}`);
    });
  });
```

**Python:**
```python
response = requests.get('https://localhost:5001/api/clientes', verify=False)
clientes = response.json()

print(f"Total de clientes: {len(clientes['data'])}")
for cliente in clientes['data']:
    print(f"{cliente['ci']} - {cliente['nombres']}")
```

---

### 1.4 Obtener Cliente Específico

**cURL:**
```bash
curl -X GET "$API_URL/api/clientes/1234567890" \
  -H "Accept: application/json"
```

**JavaScript:**
```javascript
async function obtenerCliente(ci) {
  const response = await fetch(`https://localhost:5001/api/clientes/${ci}`);
  const data = await response.json();
  
  if (data.success) {
    console.log('Cliente encontrado:', data.data);
    console.log(`Tiene fotos: 
      Foto 1: ${data.data.tieneFotoCasa1}
      Foto 2: ${data.data.tieneFotoCasa2}
      Foto 3: ${data.data.tieneFotoCasa3}
    `);
  }
}

obtenerCliente('1234567890');
```

---

### 1.5 Descargar Fotografía de Cliente

**cURL (guardar en archivo):**
```bash
curl -X GET "$API_URL/api/clientes/1234567890/foto/1" \
  -o foto_descargada.jpg
```

**PowerShell:**
```powershell
$ci = "1234567890"
$numeroFoto = 1
$outputPath = "C:\descargas\foto_cliente_${ci}_${numeroFoto}.jpg"

Invoke-WebRequest `
    -Uri "https://localhost:5001/api/clientes/$ci/foto/$numeroFoto" `
    -OutFile $outputPath

Write-Host "Foto descargada en: $outputPath"
```

**JavaScript (mostrar en imagen):**
```javascript
function mostrarFoto(ci, numeroFoto) {
  const url = `https://localhost:5001/api/clientes/${ci}/foto/${numeroFoto}`;
  
  // Crear elemento img
  const img = document.createElement('img');
  img.src = url;
  img.alt = `Foto ${numeroFoto} de cliente ${ci}`;
  img.style.maxWidth = '500px';
  
  document.body.appendChild(img);
}

mostrarFoto('1234567890', 1);
```

**Python (descargar):**
```python
def descargar_foto(ci, numero_foto, ruta_salida):
    url = f"https://localhost:5001/api/clientes/{ci}/foto/{numero_foto}"
    response = requests.get(url, verify=False)
    
    if response.status_code == 200:
        with open(ruta_salida, 'wb') as f:
            f.write(response.content)
        print(f"Foto descargada: {ruta_salida}")
    else:
        print("Error al descargar foto")

descargar_foto('1234567890', 1, '/descargas/foto1.jpg')
```

---

## 📁 2. Gestión de Archivos

### 2.1 Subir Archivos ZIP

**Preparación del ZIP:**
```bash
# Crear archivo ZIP con varios archivos
zip -r archivos_cliente.zip documentos/ fotos/ videos/
```

**cURL:**
```bash
curl -X POST "$API_URL/api/archivos/subir" \
  -H "Content-Type: multipart/form-data" \
  -F "CICliente=1234567890" \
  -F "ArchivoZip=@/ruta/archivos_cliente.zip"
```

**PowerShell:**
```powershell
$form = @{
    CICliente = "1234567890"
    ArchivoZip = Get-Item -Path "C:\archivos\archivos_cliente.zip"
}

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/archivos/subir" `
    -Method Post `
    -Form $form

Write-Host "Archivos subidos: $($response.data.Count)"
$response.data | ForEach-Object {
    Write-Host "$($_.nombreArchivo) - $($_.tamanoBytes) bytes"
}
```

**JavaScript:**
```javascript
async function subirArchivosZip(ciCliente, archivoZip) {
  const formData = new FormData();
  formData.append('CICliente', ciCliente);
  formData.append('ArchivoZip', archivoZip);

  try {
    const response = await fetch('https://localhost:5001/api/archivos/subir', {
      method: 'POST',
      body: formData
    });
    
    const data = await response.json();
    
    if (data.success) {
      console.log(`${data.message}`);
      console.log('Archivos procesados:');
      data.data.forEach(archivo => {
        console.log(`- ${archivo.nombreArchivo} (${archivo.tamanoBytes} bytes)`);
      });
    }
  } catch (error) {
    console.error('Error:', error);
  }
}

// Uso con input file
const inputFile = document.getElementById('archivoZip');
inputFile.addEventListener('change', (e) => {
  const archivo = e.target.files[0];
  subirArchivosZip('1234567890', archivo);
});
```

**Python:**
```python
def subir_archivos_zip(ci_cliente, ruta_zip):
    url = "https://localhost:5001/api/archivos/subir"
    
    data = {'CICliente': ci_cliente}
    files = {'ArchivoZip': open(ruta_zip, 'rb')}
    
    response = requests.post(url, data=data, files=files, verify=False)
    resultado = response.json()
    
    if resultado['success']:
        print(f"{resultado['message']}")
        print(f"Archivos procesados: {len(resultado['data'])}")
        for archivo in resultado['data']:
            print(f"  - {archivo['nombreArchivo']}: {archivo['tamanoBytes']} bytes")
    else:
        print(f"Error: {resultado['message']}")
    
    files['ArchivoZip'].close()

subir_archivos_zip('1234567890', '/ruta/archivos_cliente.zip')
```

---

### 2.2 Consultar Archivos de un Cliente

**cURL:**
```bash
curl -X GET "$API_URL/api/archivos/cliente/1234567890"
```

**JavaScript:**
```javascript
async function listarArchivosCliente(ci) {
  const response = await fetch(`https://localhost:5001/api/archivos/cliente/${ci}`);
  const data = await response.json();
  
  console.log(`Cliente ${ci} tiene ${data.data.length} archivos:`);
  
  // Agrupar por extensión
  const porExtension = data.data.reduce((acc, archivo) => {
    const ext = archivo.extension || 'sin extensión';
    acc[ext] = (acc[ext] || 0) + 1;
    return acc;
  }, {});
  
  console.log('Archivos por tipo:');
  Object.entries(porExtension).forEach(([ext, count]) => {
    console.log(`  ${ext}: ${count} archivo(s)`);
  });
  
  return data.data;
}

listarArchivosCliente('1234567890');
```

**Python con análisis:**
```python
def analizar_archivos_cliente(ci):
    url = f"https://localhost:5001/api/archivos/cliente/{ci}"
    response = requests.get(url, verify=False)
    data = response.json()
    
    archivos = data['data']
    total_archivos = len(archivos)
    tamano_total = sum(a['tamanoBytes'] for a in archivos)
    
    print(f"\n=== Resumen de archivos del cliente {ci} ===")
    print(f"Total de archivos: {total_archivos}")
    print(f"Tamaño total: {tamano_total / (1024*1024):.2f} MB")
    
    # Agrupar por extensión
    extensiones = {}
    for archivo in archivos:
        ext = archivo['extension'] or 'sin extensión'
        extensiones[ext] = extensiones.get(ext, 0) + 1
    
    print("\nArchivos por tipo:")
    for ext, count in sorted(extensiones.items()):
        print(f"  {ext}: {count}")
    
    # Listar archivos
    print("\nLista de archivos:")
    for archivo in archivos:
        fecha = archivo['fechaSubida'].split('T')[0]
        tamano_kb = archivo['tamanoBytes'] / 1024
        print(f"  [{fecha}] {archivo['nombreArchivo']} ({tamano_kb:.2f} KB)")

analizar_archivos_cliente('1234567890')
```

---

## 📊 3. Consulta de Logs

### 3.1 Ver Últimos Logs

**cURL:**
```bash
curl -X GET "$API_URL/api/logs?limite=50"
```

**JavaScript con filtros:**
```javascript
async function obtenerLogs(filtros = {}) {
  const params = new URLSearchParams();
  
  if (filtros.limite) params.append('limite', filtros.limite);
  if (filtros.tipoLog) params.append('tipoLog', filtros.tipoLog);
  
  const response = await fetch(`https://localhost:5001/api/logs?${params}`);
  const data = await response.json();
  
  console.log(`${data.message}`);
  console.log(`\nÚltimos ${data.data.length} logs:\n`);
  
  data.data.forEach(log => {
    const fecha = new Date(log.dateTime).toLocaleString();
    console.log(`[${log.tipoLog}] ${fecha} - ${log.metodoHttp} ${log.urlEndpoint}`);
    console.log(`  Status: ${log.statusCode} | Duración: ${log.duracionMs.toFixed(2)}ms`);
    console.log(`  IP: ${log.direccionIp}`);
    console.log('');
  });
}

// Obtener últimos 20 logs
obtenerLogs({ limite: 20 });

// Obtener solo errores
obtenerLogs({ tipoLog: 'ERROR', limite: 10 });
```

---

### 3.2 Estadísticas de la API

**cURL:**
```bash
curl -X GET "$API_URL/api/logs/estadisticas"
```

**JavaScript (visualización):**
```javascript
async function mostrarEstadisticas() {
  const response = await fetch('https://localhost:5001/api/logs/estadisticas');
  const data = await response.json();
  const stats = data.data;
  
  console.log('╔══════════════════════════════════════╗');
  console.log('║   ESTADÍSTICAS DE LA API             ║');
  console.log('╠══════════════════════════════════════╣');
  console.log(`║ Total de logs: ${stats.totalLogs.toString().padEnd(22)}║`);
  console.log(`║ Duración promedio: ${stats.duracionPromedioMs.toFixed(2)}ms`.padEnd(39) + '║');
  console.log('╠══════════════════════════════════════╣');
  console.log('║ LOGS POR TIPO:                       ║');
  console.log(`║   INFO:    ${stats.logsPorTipo.INFO.toString().padEnd(28)}║`);
  console.log(`║   ERROR:   ${stats.logsPorTipo.ERROR.toString().padEnd(28)}║`);
  console.log(`║   WARNING: ${stats.logsPorTipo.WARNING.toString().padEnd(28)}║`);
  console.log('╠══════════════════════════════════════╣');
  console.log('║ ENDPOINTS MÁS USADOS:                ║');
  
  stats.endpointsMasUsados.slice(0, 5).forEach((ep, i) => {
    const linea = `║ ${i+1}. [${ep.metodo}] ${ep.endpoint}`.substring(0, 37);
    console.log(linea.padEnd(39) + '║');
    console.log(`║    Peticiones: ${ep.cantidad.toString().padEnd(23)}║`);
  });
  
  console.log('╚══════════════════════════════════════╝');
}

mostrarEstadisticas();
```

**Python (reporte detallado):**
```python
def generar_reporte_estadisticas():
    url = "https://localhost:5001/api/logs/estadisticas"
    response = requests.get(url, verify=False)
    data = response.json()['data']
    
    print("\n" + "="*60)
    print(" "*15 + "REPORTE DE ESTADÍSTICAS")
    print("="*60)
    
    print(f"\nTotal de registros: {data['totalLogs']}")
    print(f"Duración promedio: {data['duracionPromedioMs']:.2f} ms")
    
    print("\n--- Distribución de logs por tipo ---")
    for tipo, cantidad in data['logsPorTipo'].items():
        porcentaje = (cantidad / data['totalLogs']) * 100
        print(f"  {tipo:10s}: {cantidad:5d} ({porcentaje:5.2f}%)")
    
    print("\n--- Top 10 Endpoints más utilizados ---")
    for i, endpoint in enumerate(data['endpointsMasUsados'], 1):
        print(f"\n{i}. [{endpoint['metodo']}] {endpoint['endpoint']}")
        print(f"   Peticiones: {endpoint['cantidad']}")
    
    print("\n" + "="*60)
    print(f"Última actualización: {data['ultimaActualizacion']}")
    print("="*60 + "\n")

generar_reporte_estadisticas()
```

---

### 3.3 Logs por Rango de Fechas

**cURL:**
```bash
curl -X GET "$API_URL/api/logs/por-fecha?fechaInicio=2024-11-01&fechaFin=2024-11-10"
```

**JavaScript:**
```javascript
async function logsEntreFechas(fechaInicio, fechaFin) {
  const params = new URLSearchParams({
    fechaInicio: fechaInicio,
    fechaFin: fechaFin
  });
  
  const response = await fetch(`https://localhost:5001/api/logs/por-fecha?${params}`);
  const data = await response.json();
  
  console.log(data.message);
  
  // Analizar por día
  const porDia = {};
  data.data.forEach(log => {
    const dia = log.dateTime.split('T')[0];
    porDia[dia] = (porDia[dia] || 0) + 1;
  });
  
  console.log('\nActividad por día:');
  Object.entries(porDia).sort().forEach(([dia, count]) => {
    console.log(`  ${dia}: ${count} peticiones`);
  });
}

logsEntreFechas('2024-11-01', '2024-11-10');
```

---

## 🏥 4. Health Check

**cURL:**
```bash
curl -X GET "$API_URL/health"
```

**JavaScript (monitoreo continuo):**
```javascript
async function monitorearAPI(intervalo = 30000) {
  console.log('Iniciando monitoreo de la API...\n');
  
  const verificar = async () => {
    try {
      const response = await fetch('https://localhost:5001/health');
      const data = await response.json();
      
      const timestamp = new Date().toLocaleString();
      console.log(`[${timestamp}] Estado: ${data.status} | Versión: ${data.version}`);
      
      if (data.status !== 'healthy') {
        console.error('⚠️ ALERTA: La API no está saludable!');
      }
    } catch (error) {
      console.error(`[${new Date().toLocaleString()}] ❌ ERROR: No se pudo conectar a la API`);
    }
  };
  
  // Verificar inmediatamente
  await verificar();
  
  // Verificar periódicamente
  setInterval(verificar, intervalo);
}

// Monitorear cada 30 segundos
monitorearAPI(30000);
```

---

## 💡 Casos de Uso Completos

### Caso de Uso 1: Registro Completo de Cliente

```javascript
async function registroCompleto() {
  // Paso 1: Registrar cliente con fotos
  console.log('1. Registrando cliente...');
  const formDataCliente = new FormData();
  formDataCliente.append('CI', '5555555555');
  formDataCliente.append('Nombres', 'Carlos Rodríguez Méndez');
  formDataCliente.append('Direccion', 'Zona Sur #789, Cochabamba');
  formDataCliente.append('Telefono', '72222222');
  formDataCliente.append('FotoCasa1', foto1File);
  formDataCliente.append('FotoCasa2', foto2File);
  formDataCliente.append('FotoCasa3', foto3File);
  
  const clienteResponse = await fetch('https://localhost:5001/api/clientes/registrar', {
    method: 'POST',
    body: formDataCliente
  });
  const clienteData = await clienteResponse.json();
  console.log('✓ Cliente registrado:', clienteData.data.nombres);
  
  // Paso 2: Subir documentos
  console.log('2. Subiendo documentos...');
  const formDataArchivos = new FormData();
  formDataArchivos.append('CICliente', '5555555555');
  formDataArchivos.append('ArchivoZip', archivoZipFile);
  
  const archivosResponse = await fetch('https://localhost:5001/api/archivos/subir', {
    method: 'POST',
    body: formDataArchivos
  });
  const archivosData = await archivosResponse.json();
  console.log(`✓ ${archivosData.data.length} archivos subidos`);
  
  // Paso 3: Verificar registro completo
  console.log('3. Verificando registro...');
  const verificarResponse = await fetch('https://localhost:5001/api/clientes/5555555555');
  const verificarData = await verificarResponse.json();
  
  console.log('\n=== REGISTRO COMPLETADO ===');
  console.log(`Cliente: ${verificarData.data.nombres}`);
  console.log(`CI: ${verificarData.data.ci}`);
  console.log(`Fotos: ${[verificarData.data.tieneFotoCasa1, verificarData.data.tieneFotoCasa2, verificarData.data.tieneFotoCasa3].filter(Boolean).length}/3`);
  console.log(`Documentos: ${archivosData.data.length}`);
}
```

---

**Nota:** Reemplazar `$API_URL` con la URL real de tu API en todos los ejemplos.

**Para más información, consultar:**
- README.md - Documentación completa
- PRUEBAS.md - Casos de prueba detallados
- DESPLIEGUE.md - Guía de despliegue en producción
