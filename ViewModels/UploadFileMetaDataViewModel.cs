namespace Project3.ViewModels
{
    public class UploadFileMetaDataViewModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string FileSize { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int Expiration { get; set; } = 0;
        public string[] Tags { get; set; } = new string[0];
    }
}
