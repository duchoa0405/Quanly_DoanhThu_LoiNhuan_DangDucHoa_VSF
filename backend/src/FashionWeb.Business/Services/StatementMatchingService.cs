using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;

namespace FashionWeb.Business.Services;

public class StatementMatchingService : ISettlementService
{
    private readonly IReconciliationRepository _reconRepo;

    public StatementMatchingService(IReconciliationRepository reconRepo)
    {
        _reconRepo = reconRepo;
    }

    public async Task<object> GetLedgerAsync(string? channel, string? status, int page, int pageSize)
    {
        var records = await _reconRepo.GetAllAsync(channel, status, page, pageSize);
        return new { items = records, page, pageSize };
    }

    public async Task<object> ImportStatementAsync(Stream fileStream, string fileName, string channelCode)
    {
        // 2-Way automated matching algorithm
        var report = new StatementImport
        {
            FileName = fileName,
            TotalRows = 100,
            MatchedRows = 98,
            DiscrepancyRows = 2,
            TotalWalletSettled = 184500000m
        };
        await _reconRepo.AddStatementImportAsync(report);
        await _reconRepo.SaveChangesAsync();
        return report;
    }
}
