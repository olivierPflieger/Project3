namespace Project3.DTO
{
    public class FileMetaDataResponse
    {
        public string OriginalFileName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string FileSize { get; set; } = string.Empty;        
        public DateTime CreatedDate { get; set; }
        public int Expiration { get; set; } = 0;
        public string[] Tags {  get; set; } = new string[0];

    }
}