namespace Innova.Application.DTOs
{
    public class ApiResponse<T>
    {
        public int StatusCode { get; private set; }
        public string? Message { get; private set; }
        public T? Data { get; private set; }
        public List<string> Details { get; private set; } = new();

        public ApiResponse(int statusCode, ApiResponse<T> dto)
        {
            StatusCode = statusCode;
            Data = dto.Data;
            Message = dto.Message ?? GetDefaultMessage(statusCode);
            Details = dto.Details;
        }

        public ApiResponse(int statusCode, T? data = default, string? message = null, List<string>? details = null)
        {
            StatusCode = statusCode;
            Data = data;
            Message = message ?? GetDefaultMessage(statusCode);
            Details = details ?? new List<string>();
        }

        private static string? GetDefaultMessage(int statusCode)
        {
            return statusCode switch
            {
                200 => "Success",
                201 => "Created successfully",
                204 => "No content",
                400 => "A bad request, you have made.",
                401 => "Unauthorized, you are not.",
                404 => "Resource found, it was not.",
                500 => "Errors are the path to the dark side. Errors lead to anger. Anger leads to hate. Hate leads to career change",
                _ => null
            };
        }

        public static ApiResponse<T> Success(T data) => new(200, data);
        public static ApiResponse<T> Fail(int code, string message, List<string>? details = null) =>
            new(code, default, message, details);
    }
}