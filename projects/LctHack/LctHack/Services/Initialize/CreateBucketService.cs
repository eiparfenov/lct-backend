using Amazon.S3;
using LctHack.Configuration;
using Microsoft.Extensions.Options;

namespace LctHack.Services.Initialize;

public class CreateBucketService(IServiceScopeFactory scopeFactory, IOptions<S3Options> s3Options): IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var amazonS3Client = scope.ServiceProvider.GetRequiredService<AmazonS3Client>();
        var buckets = await amazonS3Client.ListBucketsAsync(cancellationToken: cancellationToken);
        if (buckets.Buckets.Any(b => b.BucketName == s3Options.Value.BucketName))
        {
            return;
        }

        await amazonS3Client.PutBucketAsync(s3Options.Value.BucketName, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}