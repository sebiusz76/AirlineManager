namespace AirlineManager.Models.Domain
{
    public class ApplicationLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string? Exception { get; set; }
        public string? LogEvent { get; set; }
    }
}