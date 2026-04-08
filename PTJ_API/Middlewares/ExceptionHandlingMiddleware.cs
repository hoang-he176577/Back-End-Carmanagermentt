using Models.Common;
using Service.Exceptions;
using Models.Exceptions;

namespace API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (VehicleBaseException ex)
            {
                _logger.LogWarning("[VehicleException] {Code} on field {Field}: {Message}", ex.ErrorCode, ex.Field, ex.Message);

                var errorResponse = new
                {
                    errorCode = ex.ErrorCode,
                    field = ex.Field,
                    message = ex.Message,
                    severity = ex.Severity
                };

                context.Response.StatusCode = 400; // Bad Request for validation errors
                await context.Response.WriteAsJsonAsync(errorResponse);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning("[BusinessException] {Code}: {Message}", ex.Code, ex.Message);

                var response = new ApiResponse<object>
                {
                    StatusCode = (int)ex.StatusCode,
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };

                context.Response.StatusCode = (int)ex.StatusCode;
                await context.Response.WriteAsJsonAsync(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("[ArgumentException] Invalid argument provided: {Message}", ex.Message);

                var response = new ApiResponse<object>
                {
                    StatusCode = 400, // Bad Request
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };

                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Unhandled Exception]");

                // expose details in development for easier debugging
                string message = "An unexpected error occurred.";
                object? data = null;
                if (_env.IsDevelopment())
                {
                    message = ex.Message;
                    data = ex.StackTrace;
                }

                var response = new ApiResponse<object>
                {
                    StatusCode = 500,
                    Success = false,
                    Message = message,
                    Data = data
                };

                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
