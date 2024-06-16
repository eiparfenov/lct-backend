namespace LctHack.Models;

public class Match
{
    public Guid Id { get; set; }
    public required string MatchFromTitle { get; set; }
    public required string StartTime { get; set; }
    public required string EndTime { get; set; }
    public required string StartTimeMatch { get; set; }
    public required string EndTimeMatch { get; set; }
    

    public Video? Video { get; set; }
    public Guid VideoId { get; set; }
}