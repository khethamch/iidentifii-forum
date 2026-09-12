using Forum.Domain.Rules;
using Forum.Domain.Exceptions;
namespace Forum.UnitTests;
public class ForumRulesTests
{
    [Fact]
    public void GivenAuthor_WhenLikingOwnPost_ThenReject() =>
        Assert.Throws<ForumException>(() => ForumRules.EnsureCanLike("alice", "alice"));
    [Fact]
    public void GivenOtherAuthor_WhenLiking_ThenAllow() =>
        ForumRules.EnsureCanLike("alice", "bob");
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GivenEmptyContent_WhenValidated_ThenReject(string? value) =>
        Assert.Throws<ForumException>(() => ForumRules.RequiredText(value, 10));
    [Fact]
    public void GivenLongContent_WhenValidated_ThenReject() =>
        Assert.Throws<ForumException>(() => ForumRules.RequiredText("123456", 5));
    [Fact]
    public void GivenValidContent_WhenValidated_ThenTrim() =>
        Assert.Equal("hello", ForumRules.RequiredText(" hello ", 10));
}
