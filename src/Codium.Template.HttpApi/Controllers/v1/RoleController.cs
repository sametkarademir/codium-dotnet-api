using Codium.Template.Application.Contracts.Common;
using Codium.Template.Application.Contracts.Roles;
using Codium.Template.Domain.Shared.Permissions;
using Codium.Template.Domain.Shared.Result;
using Codium.Template.HttpApi.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Codium.Template.HttpApi.Controllers.v1;

[ApiController]
[Route("api/v1/roles")]
[Authorize]
[EnableRateLimiting("api")]
public class RoleController : ControllerBase
{
    private readonly IRoleAppService _roleAppService;

    public RoleController(IRoleAppService roleAppService)
    {
        _roleAppService = roleAppService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Result<RoleResponseDto>), StatusCodes.Status200OK)]
    [PermissionAuthorize(PermissionConsts.Role.Detail)]
    public async Task<IActionResult> GetByIdAsync([FromRoute(Name = "id")] Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _roleAppService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("options")]
    [ProducesResponseType(typeof(Result<ListResultDto<OptionResponseDto<Guid>>>), StatusCodes.Status200OK)]
    [PermissionAuthorize(PermissionConsts.Role.View)]
    public async Task<IActionResult> GetAllAsOptionsAsync([FromQuery] GetOptionsRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = await _roleAppService.GetAllAsOptionsAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("paged")]
    [ProducesResponseType(typeof(Result<PagedResult<RoleResponseDto>>), StatusCodes.Status200OK)]
    [PermissionAuthorize(PermissionConsts.Role.View)]
    public async Task<IActionResult> GetPageableAndFilterAsync([FromQuery] GetListRolesRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = await _roleAppService.GetPageableAndFilterAsync(request, cancellationToken);
        return Ok(response);
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [PermissionAuthorize(PermissionConsts.Role.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateRoleRequestDto request, CancellationToken cancellationToken = default)
    {
        await _roleAppService.CreateAsync(request, cancellationToken);
        return NoContent();
    }
    
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [PermissionAuthorize(PermissionConsts.Role.Update)]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute(Name = "id")] Guid id,
        [FromBody] UpdateRoleRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await _roleAppService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }
    
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [PermissionAuthorize(PermissionConsts.Role.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute(Name = "id")] Guid id, CancellationToken cancellationToken = default)
    {
        await _roleAppService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
    
    [HttpPatch("{id:guid}/sync-permissions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [PermissionAuthorize(PermissionConsts.Role.AssignPermission)]
    public async Task<IActionResult> SyncPermissionsAsync(
        [FromRoute(Name = "id")] Guid id,
        [FromBody] SyncRolePermissionsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await _roleAppService.SyncPermissionsAsync(id, request, cancellationToken);
        return NoContent();
    }
}