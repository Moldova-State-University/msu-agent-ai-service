using System.Text.Json;

namespace USMAgent.Infrastructure.AI.Tools;

public static class ToolError
{
    public static string NotFound(string message)
    {
        return JsonSerializer.Serialize(new ErrorResponse(
            Status: "error",
            Code: "NOT_FOUND",
            Message: message));
    }

    public static string AmbiguousName(string message)
    {
        return JsonSerializer.Serialize(new ErrorResponse(
            Status: "error",
            Code: "AMBIGUOUS_NAME",
            Message: message));
    }

    public static string SourceUnavailable(string message)
    {
        return JsonSerializer.Serialize(new ErrorResponse(
            Status: "error",
            Code: "SOURCE_UNAVAILABLE",
            Message: message));
    }

    public static string InvalidArguments(string message)
    {
        return JsonSerializer.Serialize(new ErrorResponse(
            Status: "error",
            Code: "INVALID_ARGUMENTS",
            Message: message));
    }
    public static string NoSchedule(string message)
    {
        return JsonSerializer.Serialize(new ErrorResponse(
            Status: "error",
            Code: "NO_SCHEDULE",
            Message: message));
    }
}


public sealed record ErrorResponse(string Status, string Code, string Message);