
namespace Models.Common
{
    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ApiResponse() { }

        public ApiResponse(int statusCode, bool success, string? message = null, T? data = default)
        {
            StatusCode = statusCode;
            Success = success;
            Message = message;
            Data = data;
        }

        // Static factory methods for convenience
        public static ApiResponse<T> Ok(T data, string? message = null)
            => new(200, true, message, data);

        public static ApiResponse<T> Created(T data, string? message = null)
            => new(201, true, message, data);

        public static ApiResponse<T> Fail(int statusCode, string message)
            => new(statusCode, false, message, default);
    }
}
