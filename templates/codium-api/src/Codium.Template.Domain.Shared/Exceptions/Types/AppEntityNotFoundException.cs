using Codium.Template.Domain.Shared.Exceptions.Abstractions;

namespace Codium.Template.Domain.Shared.Exceptions.Types;

public class AppEntityNotFoundException : AppException
{
    public override int StatusCode { get; protected set; } = 404;
    public override string ErrorCode { get; protected set; } = "APP:ENTITY:NOT_FOUND";
    public Type? EntityType { get; set; }
    public object? Id { get; set; }

    public AppEntityNotFoundException()
    {
    }

    public AppEntityNotFoundException(string message) : base(message)
    {
    }

    public AppEntityNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public AppEntityNotFoundException(Type entityType, object? id) : this(entityType, id, null)
    {
    }

    public AppEntityNotFoundException(Type entityType, object? id = null, Exception? innerException = null)
        : base(
            id == null
                ? $"There is no such an entity given id. Entity type: {entityType.FullName}"
                : $"There is no such an entity. Entity type: {entityType.FullName}, id: {id}",
            innerException)
    {
        EntityType = entityType;
        Id = id;
    }
}