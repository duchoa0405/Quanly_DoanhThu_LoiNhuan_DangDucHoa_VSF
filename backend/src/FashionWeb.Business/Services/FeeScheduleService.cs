using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;

namespace FashionWeb.Business.Services;

public class FeeScheduleService : IFeeScheduleService
{
    private readonly IFeeScheduleRepository _feeScheduleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FeeScheduleService(IFeeScheduleRepository feeScheduleRepository, IUnitOfWork unitOfWork)
    {
        _feeScheduleRepository = feeScheduleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<FeeSchedule>> GetActiveSchedulesAsync(ChannelType? channel = null, PaymentMethod? paymentMethod = null, CancellationToken ct = default)
    {
        return await _feeScheduleRepository.GetAllActiveSchedulesAsync(channel, paymentMethod, ct);
    }

    public async Task<FeeSchedule> CreateScheduleVersionAsync(CreateFeeScheduleCommand cmd, CancellationToken ct = default)
    {
        if (cmd.CommissionRate < 0m || cmd.PaymentFeeRate < 0m || cmd.ServiceFeeRate < 0m || cmd.FixedFeePerOrder < 0m)
            throw new ArgumentException("Fee rates and fixed fees cannot be negative.");

        if (cmd.ServiceFeeCap.HasValue && cmd.ServiceFeeCap.Value < 0m)
            throw new ArgumentException("Service fee cap cannot be negative.");

        FeeSchedule newSchedule = null!;

        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var priorSchedule = await _feeScheduleRepository.GetActiveScheduleAsync(cmd.Channel, cmd.PaymentMethod, ct);
            if (priorSchedule != null)
            {
                priorSchedule.Deactivate(cmd.EffectiveFrom);
            }

            newSchedule = new FeeSchedule
            {
                Id = Guid.NewGuid(),
                Channel = cmd.Channel,
                PaymentMethod = cmd.PaymentMethod,
                CommissionRate = cmd.CommissionRate,
                PaymentFeeRate = cmd.PaymentFeeRate,
                ServiceFeeRate = cmd.ServiceFeeRate,
                ServiceFeeCap = cmd.ServiceFeeCap,
                FixedFeePerOrder = cmd.FixedFeePerOrder,
                EffectiveFrom = cmd.EffectiveFrom,
                EffectiveTo = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _feeScheduleRepository.InsertScheduleVersionAsync(newSchedule, priorSchedule, ct);
        }, ct);

        return newSchedule;
    }
}
