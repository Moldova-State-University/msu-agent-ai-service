using USMAgent.Domain.Enums;

namespace USMAgent.Domain;

public sealed record Response(
    bool Success,
    object? Data = null,
    ErrorCode? Code = null,
    string? Message = null)
{
    public static Response Ok(object data) => new(true, data);

    public static Response Fail(ErrorCode code, string message) =>
        new(false, Code: code, Message: message);
}

public sealed record Response<T>(
    bool Success,
    T? Data = default,
    ErrorCode? Code = null,
    string? Message = null)
{
    public static Response<T> Ok(T data) => new(true, data);

    public static Response<T> Fail(ErrorCode code, string message) =>
        new(false, Code: code, Message: message);
}
