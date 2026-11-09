using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.Result;

public class ListResultDto<T> : IEntityDto where T : IEntityDto
{
    public IReadOnlyList<T> Data { get; set; }

    public ListResultDto()
    {
    }

    public ListResultDto(List<T> data)
    {
        Data = data;
    }
}
