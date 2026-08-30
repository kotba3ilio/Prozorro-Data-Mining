using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProzorroDataMining.Application.Import;

namespace ProzorroDataMining.Infrastructure.Import;

public sealed class TenderImportBackgroundJobQueue(IServiceScopeFactory serviceScopeFactory) : ITenderImportJobQueue
{
    private readonly Channel<TenderImportJob> _queue = Channel.CreateUnbounded<TenderImportJob>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    private readonly ConcurrentDictionary<Guid, TenderImportJob> _jobs = new();

    public async ValueTask<TenderImportJob> EnqueueAsync(
        ImportTendersRequest request,
        CancellationToken cancellationToken = default)
    {
        var job = new TenderImportJob(request, DateTimeOffset.UtcNow);

        _jobs[job.Id] = job;

        await SaveJobAsync(job, cancellationToken);
        await _queue.Writer.WriteAsync(job, cancellationToken);

        return job;
    }

    public async ValueTask UpdateAsync(
        TenderImportJob job,
        CancellationToken cancellationToken = default)
    {
        _jobs[job.Id] = job;

        await SaveJobAsync(job, cancellationToken);
    }

    public async Task<int> RequeueUnfinishedJobsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var unfinishedJobRecords = await dbContext.TenderImportJobs
            .Where(job => job.Status == TenderImportJobStatus.Queued ||
                          job.Status == TenderImportJobStatus.Running)
            .OrderBy(job => job.CreatedAt)
            .ToListAsync(cancellationToken);

        foreach (var record in unfinishedJobRecords)
        {
            var job = record.ToJob();
            job.MarkQueued("Application restarted before the import job completed. The job was returned to the queue.");
            record.UpdateFromJob(job);
            _jobs[job.Id] = job;
            await _queue.Writer.WriteAsync(job, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return unfinishedJobRecords.Count;
    }

    public TenderImportJob? GetById(Guid jobId)
    {
        if (_jobs.TryGetValue(jobId, out var inMemoryJob))
        {
            return inMemoryJob;
        }

        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return dbContext.TenderImportJobs
            .AsNoTracking()
            .SingleOrDefault(job => job.Id == jobId)
            ?.ToJob();
    }

    public IReadOnlyList<TenderImportJob> GetJobs()
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return dbContext.TenderImportJobs
            .AsNoTracking()
            .OrderByDescending(job => job.CreatedAt)
            .ToList()
            .Select(job => _jobs.TryGetValue(job.Id, out var inMemoryJob) ? inMemoryJob : job.ToJob())
            .ToList();
    }

    public IReadOnlyList<TenderImportJob> GetRecentJobs(int limit)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return dbContext.TenderImportJobs
            .AsNoTracking()
            .OrderByDescending(job => job.CreatedAt)
            .Take(Math.Max(1, limit))
            .ToList()
            .Select(job => _jobs.TryGetValue(job.Id, out var inMemoryJob) ? inMemoryJob : job.ToJob())
            .ToList();
    }

    public IAsyncEnumerable<TenderImportJob> ReadAllAsync(CancellationToken cancellationToken)
    {
        return _queue.Reader.ReadAllAsync(cancellationToken);
    }

    private async Task SaveJobAsync(TenderImportJob job, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var record = await dbContext.TenderImportJobs
            .SingleOrDefaultAsync(existingJob => existingJob.Id == job.Id, cancellationToken);

        if (record is null)
        {
            await dbContext.TenderImportJobs.AddAsync(TenderImportJobRecord.Create(job), cancellationToken);
        }
        else
        {
            record.UpdateFromJob(job);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
