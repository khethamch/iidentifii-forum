namespace Forum.Application.Dtos.Auth;

public record AuthResponseDto(string Token, DateTime ExpiresAt, CurrentUserDto User);
