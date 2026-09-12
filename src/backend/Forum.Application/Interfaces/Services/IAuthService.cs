using Forum.Application.Dtos.Auth;

namespace Forum.Application.Interfaces.Services;

public interface IAuthService
{
    Task<RegisterResponseDto> Register(RegisterRequestDto request);
    Task<AuthResponseDto> Login(LoginRequestDto request);
}