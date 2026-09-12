namespace Forum.Domain.Rules;

public static class ForumRules
{
    public static void EnsureCanLike(string authorId, string userId) =>
        throw new NotImplementedException();

    public static string RequiredText(string? value, int maximum) =>
        throw new NotImplementedException();

    public static void ValidatePage(int page, int size) =>
        throw new NotImplementedException();
}
