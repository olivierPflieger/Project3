namespace Project3.DTO
{
    public class DownloadFileResponse
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int ErrorCode { get; set; }

        public Stream? FileStream { get; set; }
        public string? ContentType { get; set; }
        public string? FileName { get; set; }
    }
}
