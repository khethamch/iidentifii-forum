using Forum.Domain.Exceptions;

namespace Forum.Domain.Rules;

public static class ForumRules
{
    public static void EnsureCanLike(string authorId, string userId)
    {
        if (authorId == userId) throw new ForumException("You cannot like your own post.", ForumError.Conflict);
    }
    public static string RequiredText(string? value, int maximum)
    {
        var text = value?.Trim() ?? "";
        if (text.Length == 0 || text.Length > maximum)
            throw new ForumException($"Content must contain between 1 and {maximum} characters.");
        return text;
    }
    public static void ValidatePage(int page, int size)
    {
        if (page < 1 || page > 100000 || size < 1 || size > 50)
            throw new ForumException("Page must be 1–100000 and pageSize must be 1–50.");
    }
}
