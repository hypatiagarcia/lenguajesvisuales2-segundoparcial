using System.IO.Compression;
using ClientesAPI.Models;
using ClientesAPI.Data;

namespace ClientesAPI.Services
{
    public interface IFileService
    {
        Task<List<ArchivoCliente>> ProcessZipFileAsync(string ciCliente, IFormFile zipFile);
        Task<byte[]?> ConvertToByteArrayAsync(IFormFile? file);
    }

    public class FileService : IFileService
    {
        private readonly string _uploadPath;
        private readonly ILogger<FileService> _logger;

        public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
        {
            _uploadPath = Path.Combine(env.ContentRootPath, "UploadedFiles");
            _logger = logger;

            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<byte[]?> ConvertToByteArrayAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        public async Task<List<ArchivoCliente>> ProcessZipFileAsync(string ciCliente, IFormFile zipFile)
        {
            var archivos = new List<ArchivoCliente>();

            // Crear directorio temporal para extracción
            var tempPath = Path.Combine(_uploadPath, "temp_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            try
            {
                // Guardar el archivo ZIP temporalmente
                var zipPath = Path.Combine(tempPath, zipFile.FileName);
                using (var stream = new FileStream(zipPath, FileMode.Create))
                {
                    await zipFile.CopyToAsync(stream);
                }

                // Extraer archivos del ZIP
                ZipFile.ExtractToDirectory(zipPath, tempPath);

                // Crear directorio para el cliente
                var clientePath = Path.Combine(_uploadPath, ciCliente);
                if (!Directory.Exists(clientePath))
                {
                    Directory.CreateDirectory(clientePath);
                }

                // Procesar cada archivo extraído
                var extractedFiles = Directory.GetFiles(tempPath, "*.*", SearchOption.AllDirectories)
                    .Where(f => !f.EndsWith(".zip"));

                foreach (var filePath in extractedFiles)
                {
                    var fileName = Path.GetFileName(filePath);
                    var extension = Path.GetExtension(filePath);
                    var fileInfo = new FileInfo(filePath);

                    // Copiar archivo al directorio del cliente
                    var destinationPath = Path.Combine(clientePath, fileName);
                    
                    // Si existe, agregar timestamp al nombre
                    if (File.Exists(destinationPath))
                    {
                        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                        fileName = $"{fileNameWithoutExt}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                        destinationPath = Path.Combine(clientePath, fileName);
                    }

                    File.Copy(filePath, destinationPath, true);

                    // Crear registro del archivo
                    var archivo = new ArchivoCliente
                    {
                        CICliente = ciCliente,
                        NombreArchivo = fileName,
                        UrlArchivo = $"/UploadedFiles/{ciCliente}/{fileName}",
                        Extension = extension,
                        TamanoBytes = fileInfo.Length,
                        FechaSubida = DateTime.Now
                    };

                    archivos.Add(archivo);

                    _logger.LogInformation($"Archivo procesado: {fileName} - {fileInfo.Length} bytes");
                }

                return archivos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar archivo ZIP");
                throw;
            }
            finally
            {
                // Limpiar directorio temporal
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }
            }
        }
    }
}
