namespace Forum.Domain.Exceptions;

public enum ForumError { Validation, NotFound, Conflict, Unauthorized }
public sealed class ForumException(string message, ForumError kind = ForumError.Validation) : Exception(message)
{
    public ForumError Kind { get; } = kind;
}