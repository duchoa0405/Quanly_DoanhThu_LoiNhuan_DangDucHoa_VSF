namespace FashionWeb.Business.Interfaces.Repositories;

public interface IUnitOfWork
{
    Task ExecuteTransactionAsync(Func<Task> action, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
