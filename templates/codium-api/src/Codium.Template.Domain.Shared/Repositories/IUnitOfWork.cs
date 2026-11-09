using Microsoft.EntityFrameworkCore.Storage;

namespace Codium.Template.Domain.Shared.Repositories;

public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Begins a database transaction
    /// </summary>
    /// <returns>The database transaction</returns>
    IDbContextTransaction BeginTransaction();

    /// <summary>
    /// Begins a database transaction asynchronously
    /// </summary>
    /// <returns>The database transaction</returns>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current transaction
    /// </summary>
    IDbContextTransaction? CurrentTransaction { get; }

    /// <summary>
    /// Indicates whether there is an active transaction
    /// </summary>
    bool HasActiveTransaction { get; }

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}