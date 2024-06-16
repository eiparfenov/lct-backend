using LctHack.Models;
using LctHack.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LctHack.Api;

public static class StatusTracking
{
    public static RouteGroupBuilder MapStatusTracking(this RouteGroupBuilder status)
    {
        status.MapPost("currentStatus", CurrentStatus);
        status.MapPost("getNotifications", GetNotifications);
        return status;
    }

    private static async Task<IResult> GetNotifications([FromQuery] string fileId, [FromServices] IWebSocketService webSocketService, HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using var socket = await context.WebSockets.AcceptWebSocketAsync();
            await webSocketService.MainLoop(fileId, socket);
        }

        return TypedResults.Ok();
    }

    private static async Task<IResult> CurrentStatus([FromQuery] string fileId, [FromServices] IVideoService videoService)
    {
        var result = await videoService.GetCurrentState(fileId);
        if (result == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(new StatusResponse()
        {
            VideoState = result.Value.state,
            Matches = result.Value.Item2.Select(m => new MatchDto()
            {
                MatchTitle = m.title,
                EndTime = m.end,
                StartTime = m.start,
                Url = m.url,
                StartTimeMatch = m.startMatch,
                EndTimeMatch = m.endMatch
            }).ToList()
        });
    }

    public class StatusResponse
    {
        public VideoState VideoState { get; set; }
        public ICollection<MatchDto> Matches { get; set; } = [];
    }

    public class MatchDto
    {
        public required string MatchTitle { get; set; }
        public required string Url { get; set; }
        public required string EndTime { get; set; }
        public required string StartTime { get; set; }
        public required string EndTimeMatch { get; set; }
        public required string StartTimeMatch { get; set; }
    }
}