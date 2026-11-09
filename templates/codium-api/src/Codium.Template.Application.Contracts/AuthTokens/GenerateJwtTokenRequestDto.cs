namespace Codium.Template.Application.Contracts.AuthTokens;

public class GenerateJwtTokenRequestDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public List<string> Roles { get; set; } = [];
    public List<string> Permissions { get; set; } = [];
    public Guid SessionId { get; set; }
}