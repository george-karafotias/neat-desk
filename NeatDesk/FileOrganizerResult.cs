namespace NeatDesk
{
    public class FileOrganizerResult
    {
        public bool Success { get; set; }
        public string Description { get; set; }

        public FileOrganizerResult(bool success)
        {
            Success = success;
            Description = string.Empty;
        }

        public FileOrganizerResult(bool success, string description) : this(success)
        {
            Description = description;
        }
    }
}
