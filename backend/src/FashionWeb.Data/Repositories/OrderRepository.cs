using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FashionWeb.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id) =>
        await _context.Orders.Include(o => o.Items).Include(o => o.FeeSnapshot).FirstOrDefaultAsync(o => o.Id == id);

    public async Task<Order?> GetByChannelOrderCodeAsync(string channelOrderCode) =>
        await _context.Orders.Include(o => o.Items).Include(o => o.FeeSnapshot).FirstOrDefaultAsync(o => o.ChannelOrderCode == channelOrderCode);

    public async Task<IEnumerable<Order>> GetAllAsync(string? channel, string? status, int page, int pageSize)
    {
        var query = _context.Orders.Include(o => o.Items).Include(o => o.FeeSnapshot).AsQueryable();
        return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task AddAsync(Order order) => await _context.Orders.AddAsync(order);
    public Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
