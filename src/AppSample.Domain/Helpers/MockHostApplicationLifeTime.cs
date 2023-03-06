using Microsoft.Extensions.Hosting;

namespace AppSample.Domain.Helpers;

public class MockHostApplicationLifeTime : IHostApplicationLifetime
{
    public void StopApplication()
    {
        //DO NOTHING
    }

    public CancellationToken ApplicationStarted { get; }
    public CancellationToken ApplicationStopped { get; }
    public CancellationToken ApplicationStopping { get; }
}