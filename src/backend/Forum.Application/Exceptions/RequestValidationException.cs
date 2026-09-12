namespace Forum.Application.Exceptions;

public sealed class RequestValidationException(Dictionary<string, string[]> errors) : Exception("Validation failed.")
{
    public Dictionary<string, string[]> Errors { get; } = errors;
}