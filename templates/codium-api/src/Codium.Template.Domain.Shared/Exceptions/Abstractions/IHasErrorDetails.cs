namespace Codium.Template.Domain.Shared.Exceptions.Abstractions;

public interface IHasErrorDetails
{
    object? Details { get; }
}