using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Interfaces.Repositories;

public interface IFeeScheduleRepository
{
    Task<FeeSchedule?> GetActiveScheduleAsync(ChannelType channel, PaymentMethod paymentMethod, CancellationToken ct = default);
    Task<IEnumerable<FeeSchedule>> GetAllActiveSchedulesAsync(ChannelType? channel = null, PaymentMethod? paymentMethod = null, CancellationToken ct = default);
    Task InsertScheduleVersionAsync(FeeSchedule newSchedule, FeeSchedule? priorSchedule, CancellationToken ct = default);
    Task AddAsync(FeeSchedule schedule, CancellationToken ct = default);
    Task UpdateAsync(FeeSchedule schedule, CancellationToken ct = default);
}
