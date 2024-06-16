namespace LctHack.Models;

public class Video
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string FormTitle { get; set; }
    public required string MlId { get; set; }
    public required VideoState VideoState { get; set; }
}

public enum VideoState
{
    Undefined,
    WaitingInOrderForProcessing,
    Processing,
    Processed,
    WaitingInOrderForDownloading,
    Downloading,
    Downloaded,
}