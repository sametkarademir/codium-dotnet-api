using Codium.Template.Application.Contracts.BaseEntities;

namespace Codium.Template.Application.Contracts.Common;

public class OptionResponseDto<TKey> : EntityDto<TKey>
{
    public string Label { get; set; } = null!;
    public Dictionary<string, object?> Attributes { get; set; } = new();
}