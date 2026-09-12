using Microsoft.AspNetCore.Identity;

namespace Forum.Infrastructure.Entities;

public class User : IdentityUser
{
    public string DisplayName { get; set; } = "";
}