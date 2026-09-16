using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Domain.Repositories;

public interface IFeeScheduleRepository
{
    Task<FeeSchedule?> GetActiveByChannelAsync(ChannelType channel);
    Task<IEnumerable<FeeSchedule>> GetAllAsync();
    Task AddAsync(FeeSchedule schedule);
    Task UpdateAsync(FeeSchedule schedule);
}
