namespace Service.Services.Common;

public sealed class ServiceResult<T>
{
    public bool Success { get; }
    public int StatusCode { get; }
    public string? Message { get; }
    public T? Data { get; }

    private ServiceResult(bool success, int statusCode, string? message, T? data)
    {
        Success = success;
        StatusCode = statusCode;
        Message = message;
        Data = data;
    }

    public static ServiceResult<T> SuccessResult(T data, int statusCode = 200)
        => new(true, statusCode, null, data);

    public static ServiceResult<T> Fail(int statusCode, string message)
        => new(false, statusCode, message, default);
}
