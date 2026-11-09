namespace Codium.Template.Application.Contracts.AuthTokens;

public interface IJwtTokenAppService
{
    GenerateJwtTokenResponseDto GenerateJwt(GenerateJwtTokenRequestDto request);
}