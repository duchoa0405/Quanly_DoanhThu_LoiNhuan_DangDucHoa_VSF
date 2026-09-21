using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Interfaces.Services;

public interface IFeeScheduleService
{
    Task<IEnumerable<FeeSchedule>> GetActiveSchedulesAsync(ChannelType? channel = null, PaymentMethod? paymentMethod = null, CancellationToken ct = default);
    Task<FeeSchedule> CreateScheduleVersionAsync(CreateFeeScheduleCommand cmd, CancellationToken ct = default);
}
