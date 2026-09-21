using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Interfaces.Repositories;
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

    public async Task<FeeSchedule?> GetActiveScheduleAsync(ChannelType channel, PaymentMethod paymentMethod, CancellationToken ct = default)
    {
        return await _context.FeeSchedules
            .FirstOrDefaultAsync(f => f.Channel == channel && f.PaymentMethod == paymentMethod && f.IsActive, ct);
    }

    public async Task<IEnumerable<FeeSchedule>> GetAllActiveSchedulesAsync(ChannelType? channel = null, PaymentMethod? paymentMethod = null, CancellationToken ct = default)
    {
        var query = _context.FeeSchedules
            .Where(f => f.IsActive)
            .AsNoTracking();

        if (channel.HasValue)
            query = query.Where(f => f.Channel == channel.Value);

        if (paymentMethod.HasValue)
            query = query.Where(f => f.PaymentMethod == paymentMethod.Value);

        return await query
            .OrderBy(f => f.Channel)
            .ThenBy(f => f.PaymentMethod)
            .ToListAsync(ct);
    }

    public async Task InsertScheduleVersionAsync(FeeSchedule newSchedule, FeeSchedule? priorSchedule, CancellationToken ct = default)
    {
        if (priorSchedule != null)
        {
            _context.FeeSchedules.Update(priorSchedule);
        }

        await _context.FeeSchedules.AddAsync(newSchedule, ct);
    }

    public async Task AddAsync(FeeSchedule schedule, CancellationToken ct = default)
    {
        await _context.FeeSchedules.AddAsync(schedule, ct);
    }

    public Task UpdateAsync(FeeSchedule schedule, CancellationToken ct = default)
    {
        _context.FeeSchedules.Update(schedule);
        return Task.CompletedTask;
    }
}
