using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.EntityFrameworkCore.Contexts;
using Microsoft.EntityFrameworkCore.Storage;

namespace Codium.Template.EntityFrameworkCore.Repositories.Common;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private bool _disposed;

    public IDbContextTransaction? CurrentTransaction => context.Database.CurrentTransaction;
    
    public bool HasActiveTransaction => CurrentTransaction != null;

    public IDbContextTransaction BeginTransaction()
    {
        if (HasActiveTransaction)
        {
            return new NoOpTransaction(CurrentTransaction!);
        }
        
        return context.Database.BeginTransaction();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (HasActiveTransaction)
        {
            return new NoOpTransaction(CurrentTransaction!);
        }
        
        return await context.Database.BeginTransactionAsync(cancellationToken);
    }

    public int SaveChanges()
    {
        return context.SaveChanges();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                context.Dispose();
            }
            
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}