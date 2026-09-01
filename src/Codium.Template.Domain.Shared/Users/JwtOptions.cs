namespace Codium.Template.Domain.Shared.Users;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int AccessTokenLifeHours { get; set; }
    public int RefreshTokenLifeHours { get; set; }
}
