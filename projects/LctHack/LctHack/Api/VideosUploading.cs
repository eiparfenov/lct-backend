using Amazon.Runtime.Internal.Auth;
using LctHack.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LctHack.Api;

public static class VideosUploading
{
    public static RouteGroupBuilder MapVideosUploading(this RouteGroupBuilder videos)
    {
        videos.MapPost("download", DownloadVideo).DisableAntiforgery();
        videos.MapPost("verify", VerifyVideo).DisableAntiforgery();
        videos.MapPost("getVideoInfo", GetVideoData);
        return videos;
    }

    private static async Task<IResult> VerifyVideo([FromQuery] string title, [FromForm] IFormFile video, [FromServices] IVideoService videoService)
    {
        using var stream = new MemoryStream();
        await video.CopyToAsync(stream);
        var id = await videoService.VerifyVideo(title, stream);
        return Results.Ok(new { Id = id });
    }
    private static async Task<IResult> DownloadVideo([FromQuery] string title, [FromForm] IFormFile video, [FromServices] IVideoService videoService)
    {
        Console.WriteLine("Got it");
        using var stream = new MemoryStream();
        await video.CopyToAsync(stream);
        var id = await videoService.DownloadVideo(title, stream);
        return Results.Ok(new { Id = id });
    }

    private static async Task<IResult> GetVideoData([FromQuery] string fileId, [FromServices] IVideoService videoService)
    {
        var result = await videoService.GetVideoUrlAndTitle(fileId);
        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new VideoInfoResponse()
        {
            Url = result.Value.url,
            Title = result.Value.title
        });
    }

    private class VideoInfoResponse
    {
        public required string Url { get; set; }
        public required string Title { get; set; }
    }
}