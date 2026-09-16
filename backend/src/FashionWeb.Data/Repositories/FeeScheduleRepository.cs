using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Domain.Repositories;
using FashionWeb.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FashionWeb.Data.Repositories;

public class FeeScheduleRepository : IFeeScheduleRepository
{
    private readonly AppDbContext _context;

    public FeeScheduleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FeeSchedule?> GetActiveByChannelAsync(ChannelType channel) =>
        await _context.FeeSchedules.FirstOrDefaultAsync(f => f.Channel == channel && f.IsActive);

    public async Task<IEnumerable<FeeSchedule>> GetAllAsync() =>
        await _context.FeeSchedules.ToListAsync();

    public async Task AddAsync(FeeSchedule schedule) => await _context.FeeSchedules.AddAsync(schedule);
    public Task UpdateAsync(FeeSchedule schedule)
    {
        _context.FeeSchedules.Update(schedule);
        return Task.CompletedTask;
    }
}
