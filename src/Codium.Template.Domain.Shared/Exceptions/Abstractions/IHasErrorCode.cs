namespace Codium.Template.Domain.Shared.Exceptions.Abstractions;

public interface IHasErrorCode
{
    string? ErrorCode { get; }
}