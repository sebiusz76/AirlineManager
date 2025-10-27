namespace AirlineManager.Models.Domain
{
    public class AppConfiguration
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string? Description { get; set; }
        public string Category { get; set; } // np. "SMTP", "General", "Security"
        public bool IsEncrypted { get; set; } // czy wartoœæ jest zaszyfrowana (np. has³a)
        public DateTime LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}