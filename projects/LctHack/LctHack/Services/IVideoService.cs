using Amazon.S3;
using Amazon.S3.Model;
using LctHack.Configuration;
using LctHack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LctHack.Services;

public interface IVideoService
{
    Task<string> DownloadVideo(string fileTitle, Stream fileStream);
    Task<string> VerifyVideo(string fileTitle, Stream fileStream);
    Task<(VideoState state, ICollection<(string title, string url, string start, string end, string startMatch, string endMatch)>)?> GetCurrentState(string formTitle);
    Task<(string url, string title)?> GetVideoUrlAndTitle(string fileId);
}

public class VideoService(ApplicationDbContext db, IMlService mlService, AmazonS3Client s3Client, IOptions<S3Options> s3Options, IUrlGenerator urlGenerator) : IVideoService
{
    public async Task<string> DownloadVideo(string fileTitle, Stream fileStream)
    {
        var videoId = Guid.NewGuid();
        var initialState = VideoState.Processing;
        var video = await AddVideo(fileTitle, fileStream, videoId, initialState);
        
        var url = await urlGenerator.CreateUrl(video.FormTitle);
        
        video.MlId = await mlService.SendVideoForDownload(url, video.FormTitle);
        
        await db.Videos.AddAsync(video);
        await db.SaveChangesAsync();
        return video.FormTitle;
    }
    public async Task<string> VerifyVideo(string fileTitle, Stream fileStream)
    {
        var videoId = Guid.NewGuid();
        var initialState = VideoState.WaitingInOrderForProcessing;
        var video = await AddVideo(fileTitle, fileStream, videoId, initialState);
        
        var url = await urlGenerator.CreateUrl(video.FormTitle);

        video.MlId = await mlService.SendVideoForValidation(url, video.FormTitle);
        await db.AddAsync(video);
        await db.SaveChangesAsync();
        return video.FormTitle;
    }

    public async Task<(VideoState state, ICollection<(string title, string url, string start, string end, string startMatch, string endMatch)>)?> GetCurrentState(string formTitle)
    {
        var video = await db.Videos.SingleOrDefaultAsync(v => v.FormTitle == formTitle);
        if (video == null)
        {
            return null;
        }

        var status = video.VideoState;
        if (status != VideoState.Processed)
        {
            return (status, []);
        }

        var matches = new List<(string title, string url, string start, string end, string startMatch, string endMatch)>();
        var matched = await db.Matches
            .Where(m => m.VideoId == video.Id)
            .ToListAsync();
        foreach (var match in matched)
        {
            var vd = await db.Videos.SingleOrDefaultAsync(v => v.FormTitle == match.MatchFromTitle);
            var matchTitle = vd?.Title ?? match.MatchFromTitle;
            var url = await urlGenerator.CreateUrl(match.MatchFromTitle);
            matches.Add((matchTitle, url, match.StartTime, match.EndTime, match.MatchFromTitle, match.EndTimeMatch));
        }

        return (status, matches);
    }

    public async Task<(string url, string title)?> GetVideoUrlAndTitle(string fileId)
    {
        var video = await db.Videos.SingleOrDefaultAsync(v => v.FormTitle == fileId);
        if (video == null)
        {
            return null;
        }

        var url = await urlGenerator.CreateUrl(fileId);
        
        return (url, video.Title);
    }

    private async Task<Video> AddVideo(string fileTitle, Stream fileStream, Guid videoId, VideoState initialState)
    {
        var video = new Video()
        {
            Id = videoId,
            Title = fileTitle,
            FormTitle = videoId.ToString(),
            VideoState = initialState,
            MlId = ""
        };
        await s3Client.PutObjectAsync(new PutObjectRequest()
        {
            BucketName = s3Options.Value.BucketName,
            Key = $"videos/{video.FormTitle}.mp4",
            InputStream = fileStream,
            CannedACL = S3CannedACL.PublicRead
        });
        return video;
    }
}
