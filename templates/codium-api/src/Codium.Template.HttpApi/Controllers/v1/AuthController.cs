using Codium.Template.Application.Contracts.Auth;
using Codium.Template.Domain.Shared.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Codium.Template.HttpApi.Controllers.v1;

[ApiController]
[Route("api/v1/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthAppService _authAppService;

    public AuthController(IAuthAppService authAppService)
    {
        _authAppService = authAppService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(Result<LoginResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = await _authAppService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(Result<LoginResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = await _authAppService.RefreshTokenAsync(request, cancellationToken);
        return Ok(response);
    }
    
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken = default)
    {
        await _authAppService.LogoutAsync(cancellationToken);
        return NoContent();
    }
}