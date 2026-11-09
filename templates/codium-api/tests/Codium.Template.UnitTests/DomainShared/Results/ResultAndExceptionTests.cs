using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;
using Codium.Template.Domain.Shared.Exceptions;
using Codium.Template.Domain.Shared.Exceptions.Abstractions;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Codium.Template.Domain.Shared.Result;

namespace Codium.Template.UnitTests.DomainShared.Results;

public class ResultAndExceptionTests
{
    private sealed class SampleDto : IEntityDto;

    [Fact]
    public void Ok_CarriesTheDataAndNoError()
    {
        var data = new SampleDto();

        var result = Result<SampleDto>.Ok(data);

        Assert.True(result.Success);
        Assert.Same(data, result.Data);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Fail_CarriesTheErrorAndNoData()
    {
        var result = Result<SampleDto>.Fail(404, "missing", "APP:X", details: new[] { "a" }, correlationId: "c-1");

        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error!.StatusCode);
        Assert.Equal("missing", result.Error.Message);
        Assert.Equal("APP:X", result.Error.ErrorCode);
        Assert.Equal("c-1", result.Error.CorrelationId);
    }

    public static TheoryData<AppException, int, string> ExceptionContracts => new()
    {
        { new AppValidationException("m"), 400, "APP:VALIDATION" },
        { new AppUnauthorizedException(), 401, "APP:UNAUTHORIZED" },
        { new AppForbiddenException("m"), 403, "APP:FORBIDDEN" },
        { new AppEntityNotFoundException("m"), 404, "APP:ENTITY:NOT_FOUND" },
        { new AppConflictException("m"), 409, "APP:CONFLICT" },
        { new AppBusinessException("m"), 422, "APP:BUSINESS" },
        { new AppInternalServerErrorException("m"), 500, "APP:INTERNAL_SERVER_ERROR" },
    };

    [Theory]
    [MemberData(nameof(ExceptionContracts))]
    public void EachExceptionType_MapsToItsStatusCodeAndErrorCode(AppException exception, int status, string code)
    {
        Assert.Equal(status, exception.StatusCode);
        Assert.Equal(code, exception.ErrorCode);
    }

    [Fact]
    public void ValidationException_WithModels_BuildsTheMessageAndDetails()
    {
        var models = new List<ValidationExceptionModel>
        {
            new() { Property = "Email", Errors = ["Email is invalid"] },
            new() { Property = "Password", Errors = ["too short", "no digit"] }
        };

        var exception = new AppValidationException(models);

        Assert.Same(models, exception.Details);
        Assert.Contains(" -- Email: Email is invalid", exception.Message);
        Assert.Contains(" -- Password: too short", exception.Message);
        Assert.Contains("no digit", exception.Message);
    }

    [Fact]
    public void NotFoundException_WithTypeAndId_KeepsBoth()
    {
        var id = Guid.NewGuid();

        var exception = new AppEntityNotFoundException(typeof(SampleDto), id);

        Assert.Equal(typeof(SampleDto), exception.EntityType);
        Assert.Equal(id, exception.Id);
        Assert.Equal(404, exception.StatusCode);
    }
}
