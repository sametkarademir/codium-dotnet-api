using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Codium.Template.Application.Contracts.AuthTokens;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Codium.Template.Application.AuthTokens;

public class JwtTokenAppService: IJwtTokenAppService
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenAppService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public GenerateJwtTokenResponseDto GenerateJwt(GenerateJwtTokenRequestDto request)
    {
        var now = DateTime.UtcNow;
        var accessTokenExpiryTime = now.AddHours(_jwtOptions.AccessTokenLifeHours);

        var claimIdentity = new ClaimsIdentity();
        claimIdentity.AddUserId(request.Id);
        claimIdentity.AddUserEmail(request.Email);
        claimIdentity.AddSessionId(request.SessionId);
        claimIdentity.AddRoles(request.Roles);
        claimIdentity.AddPermissions(request.Permissions);

        var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var credentials = new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha512Signature);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimIdentity,
            Expires = accessTokenExpiryTime,
            SigningCredentials = credentials,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(jwt);

        var refreshToken = CreateRefreshToken();

        return new GenerateJwtTokenResponseDto
        {
            AccessToken = accessToken,
            AccessTokenExpiryTime = (int)(accessTokenExpiryTime - now).TotalSeconds,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = now.AddHours(_jwtOptions.RefreshTokenLifeHours)
        };
    }

    private string CreateRefreshToken()
    {
        var token = new byte[128];
        using var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(token);

        var tokenString = Convert.ToBase64String(token)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "");

        var alphanumericToken = new StringBuilder();
        var random = new Random();

        foreach (var c in tokenString)
        {
            if (char.IsLetterOrDigit(c))
            {
                alphanumericToken.Append(c);
            }
            else
            {
                alphanumericToken.Append((char)random.Next(48, 58));
            }
        }

        return alphanumericToken.ToString();
    }
}