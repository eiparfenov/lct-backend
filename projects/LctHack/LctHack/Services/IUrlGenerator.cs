using System.Net.Http.Headers;
using Amazon.S3;
using Amazon.S3.Model;
using LctHack.Configuration;
using Microsoft.Extensions.Options;

namespace LctHack.Services;

public interface IUrlGenerator
{
    Task<string> CreateUrl(string fileId);
}

public class UrlGenerator(AmazonS3Client s3Client, IOptions<UrlGeneratorOptions> urlOptions) : IUrlGenerator
{
    public Task<string> CreateUrl(string fileId)
    {
        var url = string.Format(urlOptions.Value.Template, fileId);
        return Task.FromResult(url);
    }
}