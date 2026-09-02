namespace Codium.Template.Application.Contracts.AuthTokens;

public class GenerateJwtTokenResponseDto
{
    public string AccessToken { get; set; } = null!;
    public long AccessTokenExpiryTime { get; set; }
    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiryTime { get; set; }
}