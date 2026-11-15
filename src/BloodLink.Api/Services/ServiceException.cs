namespace BloodLink.Api.Services;

public class ServiceException : Exception
{
    public ServiceException(int statusCode, string message, IReadOnlyList<string>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors ?? Array.Empty<string>();
    }

    public int StatusCode { get; }
    public IReadOnlyList<string> Errors { get; }

    public static ServiceException NotFound(string message) => new(StatusCodes.Status404NotFound, message);
    public static ServiceException Conflict(string message) => new(StatusCodes.Status409Conflict, message);
    public static ServiceException Unauthorized(string message) => new(StatusCodes.Status401Unauthorized, message);

    public static ServiceException Validation(IReadOnlyList<string> errors) =>
        new(StatusCodes.Status400BadRequest, string.Join(" ", errors), errors);
}
