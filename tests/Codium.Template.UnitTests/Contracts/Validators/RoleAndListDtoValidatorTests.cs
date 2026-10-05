using Codium.Template.Application.Contracts.Roles;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.Domain.Shared.Roles;
using Codium.Template.UnitTests.Infrastructure;
using FluentValidation.TestHelper;

namespace Codium.Template.UnitTests.Contracts.Validators;

public class RoleAndListDtoValidatorTests
{
    [Fact]
    public void CreateRole_ValidRequest_HasNoErrors()
    {
        var validator = new CreateRoleRequestDtoValidator(new KeyLocalizer());

        validator.TestValidate(new CreateRoleRequestDto { Name = "Editors", Description = "can edit" })
            .ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateRole_MissingName_IsReported(string? name)
    {
        var validator = new CreateRoleRequestDtoValidator(new KeyLocalizer());

        validator.TestValidate(new CreateRoleRequestDto { Name = name! })
            .ShouldHaveValidationErrorFor(item => item.Name)
            .WithErrorMessage("CreateRoleRequestDto:Name:NotEmpty");
    }

    [Fact]
    public void CreateRole_TooLongNameAndDescription_AreReported()
    {
        var validator = new CreateRoleRequestDtoValidator(new KeyLocalizer());

        var result = validator.TestValidate(new CreateRoleRequestDto
        {
            Name = new string('n', RoleConsts.NameMaxLength + 1),
            Description = new string('d', RoleConsts.DescriptionMaxLength + 1)
        });

        result.ShouldHaveValidationErrorFor(item => item.Name);
        result.ShouldHaveValidationErrorFor(item => item.Description);
    }

    [Fact]
    public void CreateRole_NameAtTheLimit_IsAccepted()
    {
        var validator = new CreateRoleRequestDtoValidator(new KeyLocalizer());

        validator.TestValidate(new CreateRoleRequestDto { Name = new string('n', RoleConsts.NameMaxLength) })
            .ShouldNotHaveValidationErrorFor(item => item.Name);
    }

    [Fact]
    public void UpdateRole_MissingName_IsReported()
    {
        var validator = new UpdateRoleRequestDtoValidator(new KeyLocalizer());

        validator.TestValidate(new UpdateRoleRequestDto { Name = "" })
            .ShouldHaveValidationErrorFor(item => item.Name)
            .WithErrorMessage("UpdateRoleRequestDto:Name:NotEmpty");
    }

    // The list validator is shared by every GetList*RequestDto through Include(...); users stand in for all of them.

    [Theory]
    [InlineData(1, 10)]
    [InlineData(1, 1)]
    [InlineData(5, 100)]
    public void GetList_ValidPaging_HasNoErrors(int page, int perPage)
    {
        var validator = new GetListUsersRequestDtoValidator(new KeyLocalizer());

        validator.TestValidate(new GetListUsersRequestDto { Page = page, PerPage = perPage })
            .ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0, 10, "Page")]
    [InlineData(-1, 10, "Page")]
    [InlineData(1, 0, "PerPage")]
    [InlineData(1, 101, "PerPage")]
    public void GetList_InvalidPaging_IsReported(int page, int perPage, string property)
    {
        var validator = new GetListUsersRequestDtoValidator(new KeyLocalizer());

        var result = validator.TestValidate(new GetListUsersRequestDto { Page = page, PerPage = perPage });

        result.ShouldHaveValidationErrorFor(property);
    }

    [Fact]
    public void GetList_SearchLongerThan32Characters_IsReported()
    {
        var validator = new GetListUsersRequestDtoValidator(new KeyLocalizer());

        var result = validator.TestValidate(new GetListUsersRequestDto { Search = new string('s', 33) });

        result.ShouldHaveValidationErrorFor(item => item.Search);
    }

    [Fact]
    public void GetList_SearchOf32Characters_IsAccepted()
    {
        var validator = new GetListUsersRequestDtoValidator(new KeyLocalizer());

        validator.TestValidate(new GetListUsersRequestDto { Search = new string('s', 32) })
            .ShouldNotHaveValidationErrorFor(item => item.Search);
    }
}
