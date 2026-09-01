using Microsoft.EntityFrameworkCore.Storage;

namespace Codium.Template.EntityFrameworkCore.Repositories.Common;

public class NoOpTransaction(IDbContextTransaction innerTransaction) : IDbContextTransaction
{
    public Guid TransactionId => innerTransaction.TransactionId;
    public bool SupportsSavepoints => innerTransaction.SupportsSavepoints;

    public void Commit() 
    { 
    }

    public Task CommitAsync(CancellationToken cancellationToken = default) 
    {
        return Task.CompletedTask;
    }

    public void Rollback() 
    { 
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default) 
    {
        return Task.CompletedTask;
    }

    public void Dispose() 
    { 
    }

    public ValueTask DisposeAsync() 
    {
        return ValueTask.CompletedTask;
    }

    public Task CreateSavepointAsync(string name, CancellationToken cancellationToken = default)
    {
        return innerTransaction.CreateSavepointAsync(name, cancellationToken);
    }

    public Task ReleaseSavepointAsync(string name, CancellationToken cancellationToken = default)
    {
        return innerTransaction.ReleaseSavepointAsync(name, cancellationToken);
    }

    public Task RollbackToSavepointAsync(string name, CancellationToken cancellationToken = default)
    {
        return innerTransaction.RollbackToSavepointAsync(name, cancellationToken);
    }
}