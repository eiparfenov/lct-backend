using LctHack.Models;
using LctHack.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace LctHack.Jobs;

public class RefreshVideoStateJob(ApplicationDbContext db, IMlService mlService, IUrlGenerator urlGenerator): IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var videosToCheckForDownloading = await db.Videos
            .Where(v => v.VideoState == VideoState.Downloading)
            .ToListAsync();
        foreach (var video in videosToCheckForDownloading)
        {
            var ready = await mlService.IsDownloaded(video.MlId);
            if (ready)
            {
                video.VideoState = VideoState.Downloaded;
            }
        }

        var videosToCheckForVerifying = await db.Videos
            .Where(v => v.VideoState == VideoState.Processing)
            .ToListAsync();
        foreach (var video in videosToCheckForVerifying)
        {
            var response = await mlService.GetVideoInfoById(video.MlId);
            if (response == null) continue;
            if (response.Result.Count == 0)
            {
                video.VideoState = VideoState.Downloading;
                var url = await urlGenerator.CreateUrl(video.FormTitle);
                var mlId = await mlService.SendVideoForDownload(url, video.FormTitle);
                video.MlId = mlId;
            }
            else
            {
                video.VideoState = VideoState.Processed;
                var matches = response.Result
                    .Select(m => new Match()
                    {
                        VideoId = video.Id,
                        EndTime = m.End,
                        EndTimeMatch = m.EndMatch,
                        StartTime = m.Start,
                        StartTimeMatch = m.StartMatch,
                        MatchFromTitle = m.Title
                    });
                await db.Matches.AddRangeAsync(matches);
            }
        }

        await db.SaveChangesAsync();
    }
}