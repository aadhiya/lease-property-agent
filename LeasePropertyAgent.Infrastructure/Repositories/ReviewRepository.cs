using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Domain.Entities;
using LeasePropertyAgent.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeasePropertyAgent.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly LeasePropertyDbContext _dbContext;

    public ReviewRepository(LeasePropertyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LeaseField?> GetLeaseFieldByIdAsync(
        Guid fieldId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.LeaseFields
            .FirstOrDefaultAsync(
                field => field.Id == fieldId,
                cancellationToken);
    }

    public async Task<LeaseFlag?> GetLeaseFlagByIdAsync(
        Guid flagId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.LeaseFlags
            .FirstOrDefaultAsync(
                flag => flag.Id == flagId,
                cancellationToken);
    }

    public async Task<WorkOrder?> GetWorkOrderByIdAsync(
        Guid workOrderId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.WorkOrders
            .FirstOrDefaultAsync(
                workOrder => workOrder.Id == workOrderId,
                cancellationToken);
    }

    public async Task AddReviewActionAsync(
        ReviewAction reviewAction,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.ReviewActions.AddAsync(
            reviewAction,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}