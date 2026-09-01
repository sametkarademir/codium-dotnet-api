namespace Codium.Template.Domain.Shared.Exceptions.Abstractions;

public interface IHasCorrelationId
{
    string? CorrelationId { get; }
}