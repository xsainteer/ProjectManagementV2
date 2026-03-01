using Domain.Common;

namespace Application.Interfaces;

public interface IRepository<T> where T : class, IEntity
{
    Task<T?> GetByIdAsync(int id, bool asNoTracking = true, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(bool asNoTracking = true, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
