namespace Assignment.Models
{
    public class BannedModel
    {
        public long UserId { get; set; }
        public string Reason { get; set; }
        public bool IsLifeTime { get; set; }
        public string? EndTime { get; set; }
    }
}
