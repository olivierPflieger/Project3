namespace Project3.ViewModels
{
    public class FileMetaDataViewModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string GeneratedFileName { get; set; } = string.Empty;
        public string FileSize { get; set; } = string.Empty;
    }
}