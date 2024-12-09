namespace SimpleMimo.Models;

public class ErrorResponse
{
    public required string Message { get; init; }

    public Dictionary<string, string> Errors { get; init; } = new();
}