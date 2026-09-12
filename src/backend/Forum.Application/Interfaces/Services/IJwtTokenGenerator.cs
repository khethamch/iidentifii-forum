using Forum.Application.Dtos.Auth;

namespace Forum.Application.Interfaces.Services;

public interface IJwtTokenGenerator
{
    AuthResponseDto Generate(CurrentUserDto user, IEnumerable<string> roles);
}