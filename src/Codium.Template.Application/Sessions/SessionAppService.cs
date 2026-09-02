using AutoMapper;
using Codium.Template.Application.Contracts.Common.Results;
using Codium.Template.Application.Contracts.Sessions;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.Domain.Shared.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Codium.Template.Application.Sessions;

public class SessionAppService : ISessionAppService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<ApplicationResource> _localizer;
    private readonly ILogger<SessionAppService> _logger;

    public SessionAppService(
        ISessionRepository sessionRepository, 
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork, 
        ICurrentUser currentUser, 
        IMapper mapper, 
        IStringLocalizer<ApplicationResource> localizer,
        ILogger<SessionAppService> logger)
    {
        _sessionRepository = sessionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _localizer = localizer;
        _logger = logger;

        if (!_currentUser.IsAuthenticated)
        {
            throw new AppUnauthorizedException();
        }
    }

    public async Task<Result<PagedResult<SessionResponseDto>>> GetPageableAndFilterAsync(GetListSessionsRequestDto request, CancellationToken cancellationToken = default)
    {
        var queryable = _sessionRepository.AsQueryable();

        queryable = queryable.Where(s => s.UserId == _currentUser.Id);
        queryable = queryable.Where(s => s.IsRevoked == false);
        queryable = queryable.WhereIf(request.IsDesktop != null, s => s.IsDesktop == request.IsDesktop);
        queryable = queryable.WhereIf(request.IsMobile != null, s => s.IsMobile == request.IsMobile);
        queryable = queryable.WhereIf(request.IsTablet != null, s => s.IsTablet == request.IsTablet);

        queryable = queryable.AsNoTracking();
        queryable = queryable.ApplySort(request.Field, request.Order, cancellationToken);
        var pagedSessions = await queryable.ToPageableAsync(request.Page, request.PerPage, cancellationToken);
        
        var mappedSessions = _mapper.Map<List<SessionResponseDto>>(pagedSessions.Data);

        return Result<PagedResult<SessionResponseDto>>.Ok(new PagedResult<SessionResponseDto>(mappedSessions, pagedSessions.TotalCount, pagedSessions.Page, pagedSessions.PerPage)); 
    }

    public async Task InvalidateSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var matchedSession = await _sessionRepository.RevokeSessionByUserAsync(sessionId, _currentUser.Id!.Value, cancellationToken);
            if (matchedSession == null) 
            {
                throw new AppEntityNotFoundException(_localizer["SessionAppService:InvalidateSessionAsync:SessionNotFound"]);
            }
        
            await _refreshTokenRepository.RevokeRefreshTokensBySessionAsync(matchedSession.Id, _currentUser.Id!.Value, cancellationToken);
        
            await transaction.CommitAsync(cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger
                .WithProperties()
                .Add("Services", nameof(SessionAppService))
                .Add("Method", nameof(InvalidateSessionAsync))
                .Add("SessionId", sessionId)
                .Add("UserId", _currentUser.Id!.Value)
                .LogError($"An error occurred while invalidating session. Transaction is being rolled back. Exception Message: {e.Message}", e);
            
            await transaction.RollbackAsync(cancellationToken);
            
            throw;
        }
    }
}