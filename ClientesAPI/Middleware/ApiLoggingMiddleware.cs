using ClientesAPI.Data;
using ClientesAPI.Models;
using System.Diagnostics;
using System.Text;

namespace ClientesAPI.Middleware
{
    public class ApiLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiLoggingMiddleware> _logger;

        public ApiLoggingMiddleware(RequestDelegate next, ILogger<ApiLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestBody = await GetRequestBodyAsync(context.Request);
            var originalResponseBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            var logEntry = new LogApi
            {
                DateTime = DateTime.Now,
                UrlEndpoint = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}",
                MetodoHttp = context.Request.Method,
                DireccionIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                RequestBody = requestBody
            };

            try
            {
                await _next(context);

                stopwatch.Stop();
                logEntry.StatusCode = context.Response.StatusCode;
                logEntry.DuracionMs = stopwatch.Elapsed.TotalMilliseconds;
                logEntry.TipoLog = context.Response.StatusCode >= 400 ? "ERROR" : "INFO";
                logEntry.ResponseBody = await GetResponseBodyAsync(responseBody);
                logEntry.Detalle = $"Request procesado exitosamente en {logEntry.DuracionMs:F2}ms";

                _logger.LogInformation($"{logEntry.MetodoHttp} {logEntry.UrlEndpoint} - {logEntry.StatusCode} - {logEntry.DuracionMs:F2}ms");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logEntry.StatusCode = 500;
                logEntry.DuracionMs = stopwatch.Elapsed.TotalMilliseconds;
                logEntry.TipoLog = "ERROR";
                logEntry.Detalle = $"Error: {ex.Message}";
                logEntry.ResponseBody = ex.ToString();

                _logger.LogError(ex, $"Error en {logEntry.MetodoHttp} {logEntry.UrlEndpoint}");

                // Reescribir la respuesta de error
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                var errorResponse = System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
                
                var errorBytes = Encoding.UTF8.GetBytes(errorResponse);
                await context.Response.Body.WriteAsync(errorBytes, 0, errorBytes.Length);
            }
            finally
            {
                // Guardar log en base de datos
                try
                {
                    dbContext.LogsApi.Add(logEntry);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception logEx)
                {
                    _logger.LogError(logEx, "Error al guardar log en base de datos");
                }

                // Copiar respuesta al stream original
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalResponseBodyStream);
            }
        }

        private async Task<string> GetRequestBodyAsync(HttpRequest request)
        {
            try
            {
                request.EnableBuffering();
                var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                await request.Body.ReadAsync(buffer, 0, buffer.Length);
                var bodyText = Encoding.UTF8.GetString(buffer);
                request.Body.Position = 0;

                // Limitar tamaño del log
                return bodyText.Length > 4000 ? bodyText.Substring(0, 4000) + "..." : bodyText;
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<string> GetResponseBodyAsync(MemoryStream responseBody)
        {
            try
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                var text = await new StreamReader(responseBody).ReadToEndAsync();
                responseBody.Seek(0, SeekOrigin.Begin);

                // Limitar tamaño del log
                return text.Length > 4000 ? text.Substring(0, 4000) + "..." : text;
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    public static class ApiLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiLoggingMiddleware>();
        }
    }
}
