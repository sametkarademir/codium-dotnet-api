using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Querying;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Common;

public abstract class GetListRequestDto
{
    public int Page { get; set; } = 1;
    public int PerPage { get; set; } = 10;
    public string? Search { get; set; }
    public string? Field { get; set; }
    public SortOrderTypes Order { get; set; } = SortOrderTypes.Desc;
    
    public SortRequest GetSortRequest(string? defaultField = null)
    {
        return new SortRequest()
        {
            Field = this.Field ?? defaultField,
            Order = this.Order
        };
    }
}

public class GetListRequestDtoValidator : AbstractValidator<GetListRequestDto>
{
    public GetListRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        RuleFor(item => item.Page)
            .GreaterThan(0).WithMessage(localizer["GetList:Page:GreaterThanZero"]);

        RuleFor(item => item.PerPage)
            .GreaterThan(0).WithMessage(localizer["GetList:PerPage:GreaterThanZero"])
            .LessThanOrEqualTo(100).WithMessage(localizer["GetList:PerPage:LessThanOrEqualTo", 100]);
        
        RuleFor(item => item.Search)
            .MaximumLength(32).WithMessage(localizer["GetList:Search:MaxLength", 32]);
        
        RuleFor(item => item.Field)
            .MaximumLength(256).WithMessage(localizer["GetList:Field:MaxLength", 256]);

        RuleFor(item => item.Order)
            .IsInEnum().WithMessage(localizer["GetList:Order:IsInEnum"]);
    }
}