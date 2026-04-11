namespace DataShare_API.Models
{
    public class FileUploadSettings
    {
        public long MaxFileSize { get; set; } = 0;
        public string AwsBucketName { get; set; } = string.Empty;
    }
}