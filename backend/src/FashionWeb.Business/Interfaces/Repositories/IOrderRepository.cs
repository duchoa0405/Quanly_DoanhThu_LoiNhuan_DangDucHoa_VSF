using FashionWeb.Business.Domain.Entities;

namespace FashionWeb.Business.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order?> GetByChannelOrderCodeAsync(string channelOrderCode);
    Task<IEnumerable<Order>> GetAllAsync(string? channel, string? status, int page, int pageSize);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task SaveChangesAsync();
}
