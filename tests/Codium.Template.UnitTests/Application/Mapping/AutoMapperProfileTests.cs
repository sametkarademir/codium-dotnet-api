using AutoMapper;
using Codium.Template.Application;

namespace Codium.Template.UnitTests.Application.Mapping;

public class AutoMapperProfileTests
{
    // Destination members the profile does not map on purpose: the app services fill them explicitly
    // (roles, permissions) or the request DTO carries data the entity never holds (passwords, option attributes).
    // The list is exact: a NEW unmapped member fails this test, and a member that becomes mapped must be removed here.
    private static readonly string[] KnownUnmappedMembers =
    [
        "Permission -> OptionResponseDto`1.Attributes",
        "Role -> RoleResponseDto.Permissions",
        "User -> CreateUserRequestDto.ConfirmPassword",
        "User -> CreateUserRequestDto.Password",
        "User -> UserResponseDto.Roles",
    ];

    [Fact]
    public void Configuration_HasOnlyTheKnownUnmappedMembers()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<ApplicationAutoMapperProfiles>());

        var exception = Record.Exception(configuration.AssertConfigurationIsValid);

        var actual = exception is AutoMapperConfigurationException mapperException
            ? mapperException.Errors
                .SelectMany(error => error.UnmappedPropertyNames.Select(property =>
                    $"{error.TypeMap!.SourceType.Name} -> {error.TypeMap.DestinationType.Name}.{property}"))
                .Order()
                .ToArray()
            : [];

        Assert.Equal(KnownUnmappedMembers.Order().ToArray(), actual);
    }

    [Fact]
    public void Configuration_CanBeBuilt_AndCreatesAMapper()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<ApplicationAutoMapperProfiles>());

        Assert.NotNull(configuration.CreateMapper());
    }
}
