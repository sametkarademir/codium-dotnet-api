using Codium.Template.Application.Contracts.Common;
using Codium.Template.Application.Contracts.Permissions;
using Codium.Template.Domain.Shared.Permissions;
using Codium.Template.Domain.Shared.Result;
using Codium.Template.HttpApi.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Codium.Template.HttpApi.Controllers.v1;

[ApiController]
[Route("api/v1/permissions")]
[Authorize]
[EnableRateLimiting("api")]
public class PermissionController : ControllerBase
{
    private readonly IPermissionAppService _permissionAppService;

    public PermissionController(IPermissionAppService permissionAppService)
    {
        _permissionAppService = permissionAppService;
    }

    [HttpGet("paged")]
    [ProducesResponseType(typeof(Result<PagedResult<PermissionResponseDto>>), StatusCodes.Status200OK)]
    [PermissionAuthorize(PermissionConsts.Permission.View)]
    public async Task<IActionResult> GetPageableAndFilterAsync([FromQuery] GetListPermissionsRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = await _permissionAppService.GetPageableAndFilterAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("options")]
    [ProducesResponseType(typeof(Result<ListResultDto<OptionResponseDto<Guid>>>), StatusCodes.Status200OK)]
    [PermissionAuthorize(PermissionConsts.Permission.View)]
    public async Task<IActionResult> GetAllAsOptionsAsync([FromQuery] GetOptionsRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = await _permissionAppService.GetAllAsOptionsAsync(request, cancellationToken);
        return Ok(response);
    }
}