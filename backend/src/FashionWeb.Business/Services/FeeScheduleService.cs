using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Domain.Validators;
using FashionWeb.Business.Exceptions;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;

namespace FashionWeb.Business.Services;

public class FeeScheduleService : IFeeScheduleService
{
    private readonly IFeeScheduleRepository _feeScheduleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public FeeScheduleService(
        IFeeScheduleRepository feeScheduleRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _feeScheduleRepository = feeScheduleRepository ?? throw new ArgumentNullException(nameof(feeScheduleRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<IEnumerable<FeeSchedule>> GetActiveSchedulesAsync(ChannelType? channel = null, PaymentMethod? paymentMethod = null, CancellationToken ct = default)
    {
        return await _feeScheduleRepository.GetAllActiveSchedulesAsync(channel, paymentMethod, ct);
    }

    public async Task<FeeSchedule> CreateScheduleVersionAsync(CreateFeeScheduleCommand cmd, CancellationToken ct = default)
    {
        OrderValidationRules.ValidateChannelPaymentCompatibility(cmd.Channel, cmd.PaymentMethod);

        if (cmd.CommissionRate < 0m || cmd.CommissionRate > 1.0m)
            throw new ValidationException($"Commission rate must be between 0 and 1 (0% to 100%). Received: {cmd.CommissionRate}.");

        if (cmd.PaymentFeeRate < 0m || cmd.PaymentFeeRate > 1.0m)
            throw new ValidationException($"Payment fee rate must be between 0 and 1 (0% to 100%). Received: {cmd.PaymentFeeRate}.");

        if (cmd.ServiceFeeRate < 0m || cmd.ServiceFeeRate > 1.0m)
            throw new ValidationException($"Service fee rate must be between 0 and 1 (0% to 100%). Received: {cmd.ServiceFeeRate}.");

        if (cmd.FixedFeePerOrder < 0m)
            throw new ValidationException("Fixed fee per order cannot be negative.");

        if (cmd.ServiceFeeCap.HasValue && cmd.ServiceFeeCap.Value < 0m)
            throw new ValidationException("Service fee cap cannot be negative.");

        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        if (cmd.EffectiveFrom > today)
            throw new ValidationException($"Future effective dates are not supported in MVP. EffectiveFrom must be on or before {today:yyyy-MM-dd}.");

        FeeSchedule newSchedule = null!;
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var priorSchedule = await _feeScheduleRepository.GetActiveScheduleAsync(cmd.Channel, cmd.PaymentMethod, ct);
            if (priorSchedule != null)
            {
                priorSchedule.Deactivate(cmd.EffectiveFrom);
                await _feeScheduleRepository.UpdateAsync(priorSchedule, ct);
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
                CreatedAt = now
            };

            await _feeScheduleRepository.AddAsync(newSchedule, ct);
        }, ct);

        return newSchedule;
    }
}
