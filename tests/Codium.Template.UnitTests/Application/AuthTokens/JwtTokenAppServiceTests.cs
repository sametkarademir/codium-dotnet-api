using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Codium.Template.Application.AuthTokens;
using Codium.Template.Application.Contracts.AuthTokens;
using Codium.Template.Domain.Shared.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Codium.Template.UnitTests.Application.AuthTokens;

public class JwtTokenAppServiceTests
{
    private static readonly JwtOptions Options = new()
    {
        SigningKey = new string('k', 128),
        Issuer = "https://issuer.test",
        Audience = "https://audience.test",
        AccessTokenLifeHours = 2,
        RefreshTokenLifeHours = 48
    };

    private static JwtTokenAppService CreateService(JwtOptions? options = null) =>
        new(Microsoft.Extensions.Options.Options.Create(options ?? Options));

    private static GenerateJwtTokenRequestDto Request() => new()
    {
        Id = Guid.NewGuid(),
        Email = "user@example.com",
        Roles = ["Admin", "Editor"],
        Permissions = ["User.View", "Role.Create"],
        SessionId = Guid.NewGuid()
    };

    private static JwtSecurityToken Read(string token) => new JwtSecurityTokenHandler().ReadJwtToken(token);

    [Fact]
    public void GenerateJwt_CarriesTheIdentityRolesPermissionsAndSession()
    {
        var request = Request();

        var response = CreateService().GenerateJwt(request);
        var jwt = Read(response.AccessToken);

        Assert.Equal(request.Id.ToString(), jwt.Claims.Single(c => c.Type == "nameid").Value);
        Assert.Equal(request.Email, jwt.Claims.Single(c => c.Type == "email").Value);
        Assert.Equal(request.SessionId.ToString(), jwt.Claims.Single(c => c.Type == "session_id").Value);
        Assert.Equal(request.Roles, jwt.Claims.Where(c => c.Type == "role").Select(c => c.Value));
        Assert.Equal(request.Permissions, jwt.Claims.Where(c => c.Type == "permission").Select(c => c.Value));
    }

    [Fact]
    public void GenerateJwt_UsesTheConfiguredIssuerAndAudience()
    {
        var jwt = Read(CreateService().GenerateJwt(Request()).AccessToken);

        Assert.Equal(Options.Issuer, jwt.Issuer);
        Assert.Equal(Options.Audience, Assert.Single(jwt.Audiences));
    }

    [Fact]
    public void GenerateJwt_ExpiresAfterTheConfiguredAccessTokenLife()
    {
        var before = DateTime.UtcNow;

        var response = CreateService().GenerateJwt(Request());
        var jwt = Read(response.AccessToken);

        Assert.Equal(2 * 3600, response.AccessTokenExpiryTime);
        Assert.InRange(jwt.ValidTo, before.AddHours(2).AddSeconds(-2), DateTime.UtcNow.AddHours(2).AddSeconds(2));
    }

    [Fact]
    public void GenerateJwt_RefreshTokenExpiresAfterTheConfiguredLife()
    {
        var before = DateTime.UtcNow;

        var response = CreateService().GenerateJwt(Request());

        Assert.InRange(response.RefreshTokenExpiryTime, before.AddHours(48), DateTime.UtcNow.AddHours(48));
    }

    [Fact]
    public void GenerateJwt_TokenIsSignedWithTheConfiguredKey()
    {
        var token = CreateService().GenerateJwt(Request()).AccessToken;

        var parameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Options.SigningKey)),
            ValidIssuer = Options.Issuer,
            ValidAudience = Options.Audience,
            ClockSkew = TimeSpan.Zero
        };

        new JwtSecurityTokenHandler().ValidateToken(token, parameters, out var validated);
        Assert.NotNull(validated);

        parameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(new string('x', 128)));
        Assert.ThrowsAny<SecurityTokenException>(() => new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _));
    }

    [Fact]
    public void GenerateJwt_WithoutRolesOrPermissions_HasNoSuchClaims()
    {
        var request = Request();
        request.Roles = [];
        request.Permissions = [];

        var jwt = Read(CreateService().GenerateJwt(request).AccessToken);

        Assert.DoesNotContain(jwt.Claims, c => c.Type is "role" or "permission");
    }

    [Fact]
    public void RefreshToken_IsAlphanumeric_LongEnough_AndUniquePerCall()
    {
        var service = CreateService();

        var tokens = Enumerable.Range(0, 20).Select(_ => service.GenerateJwt(Request()).RefreshToken).ToList();

        Assert.All(tokens, token =>
        {
            Assert.True(token.Length >= 100, $"token too short: {token.Length}");
            Assert.All(token, c => Assert.True(char.IsAsciiLetterOrDigit(c), $"unexpected character '{c}'"));
        });
        Assert.Equal(tokens.Count, tokens.Distinct().Count());
    }
}
