namespace Application.Authentication.DTO;

public record TokenResponseDto(string Token, string TokenType = "Bearer", int ExpiresIn = 3600);
