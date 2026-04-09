namespace Project3.DTO
{
    public class FileMetaDataResponse
    {
        public string OriginalFileName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string FileSize { get; set; } = string.Empty;        
        public bool IsExpired { get; set; } = false;        
        public int ExpirationDays { get; set; } = 0;
        public DateTime ExpirationDate { get; set; } = DateTime.MinValue;
        public string[] Tags {  get; set; } = new string[0];

    }
}