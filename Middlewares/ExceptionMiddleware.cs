using System.Net;
using System.Text.Json;

namespace Pasteleria.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context) //InvokeAsync captura la excepción y loguea
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error interno en el servidor.");
                // Aquí llamamos al método
                await HandleExceptionAsync(context, ex);
            }
        } // <-- VERIFICA ESTO: InvokeAsync debe cerrarse aquí

        // El método debe existir en este nivel, dentro de la clase
        private static Task HandleExceptionAsync(HttpContext context, Exception exception) //HandleExceptionAsync: construir y enviar la respuesta HTTP de error al cliente
        {
            // Indicamos al cliente que el cuerpo de la respuesta será JSON.
            // Sin esto, el navegador podría interpretarlo como texto plano o HTML.
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; //Usamos el enum HttpStatusCode para mayor legibilidad que poner 500 a mano.

            // Construimos un objeto anónimo con la "forma" de nuestra respuesta de error.
            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Ha ocurrido un error inesperado al procesar la solicitud.", //Mensaje genérico para el usuario
                Detalle = exception.Message                                            //Sin revelar detalles
            };

            //Serializamos el objeto anónimo a un string JSON.
            //JsonSerializer usa System.Text.Json, el serializador nativo de .NET.
            var json = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(json);
        }
    } // Cierre de la clase
} // Cierre del namespace