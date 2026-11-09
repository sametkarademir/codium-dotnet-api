namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.MultiTenancy;

public interface IMultiTenant
{
    Guid TenantId { get; set; }
}