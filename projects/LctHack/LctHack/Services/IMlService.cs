using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace LctHack.Services;

public interface IMlService
{
    Task<string> SendVideoForDownload(string url, string title);
    Task<string> SendVideoForValidation(string url, string title);
    Task<ValidationResponse?> GetVideoInfoById(string videoMlId);
    Task<bool> IsDownloaded(string fileMlId);
}

public class MlService(HttpClient client) : IMlService
{
    public async Task<string> SendVideoForDownload(string url, string title)
    {
        var response = await client.PostAsJsonAsync("set_video_download", new
        {
            Url = url,
            Title = title,
            Purpose = "index"
        });
        var id = await response.Content.ReadFromJsonAsync<MlIdResponse>();
        return id!.TaskId;
    }

    public async Task<string> SendVideoForValidation(string url, string title)
    {
        var response = await client.PostAsJsonAsync("set_video_download", new
        {
            Url = url,
            Title = title,
            Purpose = "val"
        });
        var id = await response.Content.ReadFromJsonAsync<MlIdResponse>();
        return id!.TaskId;
    }

    public async Task<ValidationResponse?> GetVideoInfoById(string videoMlId)
    {
        var httpResponse = await client.PostAsJsonAsync("task_status", new
        {
            task_id = videoMlId
        });
        var response = await httpResponse.Content.ReadFromJsonAsync<MlStrangeResponse>();
        if (response?.Status != "ready") return null;
        
        var intervals = response?.Content?.Intervals;
        if (string.IsNullOrEmpty(intervals))
        {
            return new ValidationResponse();
        }
        var match = _intervalsRegex.Match(intervals);
        if (!match.Success)
        {
            return new ValidationResponse();
        }

        return new ValidationResponse()
        {
            Result =
            [
                new ValidationProbe()
                {
                    End = match.Groups["End"].Value,
                    Start = match.Groups["Start"].Value,
                    EndMatch = match.Groups["EndMatch"].Value,
                    StartMatch = match.Groups["StartMatch"].Value,
                    Title = response!.Content!.Filename!
                }
            ]
        };

    }

    public async Task<bool> IsDownloaded(string fileMlId)
    {
        var httpResponse = await client.PostAsJsonAsync("task_status", new
        {
            task_id = fileMlId
        });
        var response = await httpResponse.Content.ReadFromJsonAsync<MlStrangeResponse>();
        return response?.Status != "ready";
    }

    class MlIdResponse
    {
        [JsonPropertyName("task_id")]
        public required string TaskId { get; set; }
    }

    class MlStrangeResponse
    {
        public required string Status { get; set; }
        public MlStrangeResponseContent? Content { get; set; }
    }

    class MlStrangeResponseContent
    {
        public string? Intervals { get; set; }
        public string? Filename { get; set; }
    }

    private Regex _intervalsRegex = new Regex(@"(?<Start>\d*)-(?<End>\d*) (?<StartMatch>\d*)-(?<EndMatch>\d*)");
}

public class ValidationProbe
{
    public required string Title { get; set; }
    public required string Start { get; set; }
    public required string End { get; set; }
    public required string EndMatch { get; set; }
    public required string StartMatch { get; set; }
}
public class ValidationResponse
{
    public ICollection<ValidationProbe> Result { get; set; } = [];
}


public class MockMlService(ILogger<MockMlService> logger) : IMlService
{ 
    public async Task<string> SendVideoForDownload(string url, string title)
    {
        logger.LogInformation(url);
        await Task.Delay(TimeSpan.FromSeconds(3));
        return "";
    }

    public async Task<string> SendVideoForValidation(string url, string title)
    {
        logger.LogInformation(url);
        await Task.Delay(TimeSpan.FromSeconds(5));
        return "";
    }

    public async Task<ValidationResponse?> GetVideoInfoById(string videoMlId)
    {
        return new ValidationResponse();
    }

    public Task<bool> IsDownloaded(string fileMlId)
    {
        return Task.FromResult(true);
    }
}

