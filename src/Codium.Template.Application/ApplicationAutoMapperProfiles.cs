using AutoMapper;
using Codium.Template.Application.Contracts.Common;
using Codium.Template.Application.Contracts.Permissions;
using Codium.Template.Application.Contracts.Roles;
using Codium.Template.Application.Contracts.Sessions;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.Domain.Permissions;
using Codium.Template.Domain.Roles;
using Codium.Template.Domain.Sessions;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Users;

namespace Codium.Template.Application;

public class ApplicationAutoMapperProfiles : Profile
{
    public ApplicationAutoMapperProfiles()
    {
        CreateMap<Permission, PermissionResponseDto>();
        CreateMap<Permission, OptionResponseDto<Guid>>()
            .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Name));

        CreateMap<Role, RoleResponseDto>();
        CreateMap<Role, CreateRoleRequestDto>();
        CreateMap<Role, UpdateRoleRequestDto>();
        CreateMap<Role, OptionResponseDto<Guid>>()
            .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => new Dictionary<string, object?>
            {
                { nameof(Role.Description), src.Description },
            }.CamelizeKeys()));

        CreateMap<Session, SessionResponseDto>();

        CreateMap<User, UserResponseDto>();
        CreateMap<User, CreateUserRequestDto>();
        CreateMap<User, UpdateUserRequestDto>();
        CreateMap<User, OptionResponseDto<Guid>>()
            .ForMember(dest => dest.Label, opt => opt.MapFrom(src =>
                !string.IsNullOrWhiteSpace(src.FirstName) || !string.IsNullOrWhiteSpace(src.LastName)
                    ? $"{src.FirstName} {src.LastName}".Trim()
                    : src.Email))
            .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => new Dictionary<string, object?>
            {
                { nameof(User.IsActive), src.IsActive },
            }.CamelizeKeys()));
    }
}