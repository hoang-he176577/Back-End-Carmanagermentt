using Models.Common;
using Service.Exceptions;

namespace API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Unhandled Exception]");

                var response = new ApiResponse<object>
                {
                    StatusCode = 500,
                    Success = false,
                    Message = "An unexpected error occurred.",
                    Data = null
                };

                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
