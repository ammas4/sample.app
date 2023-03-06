using AppSample.CoreTools.Jobs;
using Microsoft.Extensions.Logging;

namespace AppSample.Domain.Jobs;

public class ReloadHttpContentJob : BaseJob
{
    public ReloadHttpContentJob(ILogger<ReloadHttpContentJob> logger):base(logger)
    {
    }

    public async Task Load(CancellationToken cancellationToken)
    {
        await ExecuteAsync(cancellationToken, "ReloadHttpContentJob", async () =>
        {
             
        });
    }
}