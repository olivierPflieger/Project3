namespace DataShare_API.Utils
{
    public static class FileUtils
    {
        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "o", "Ko", "Mo", "Go", "To" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:F2} {sizes[order]}";
        }

        public static ExpirationDetails CalculateExpirationDetails(DateTime creationDate, int expirationDays)
        {
            var expirationDetails = new ExpirationDetails();

            expirationDetails.ExpirationDate = creationDate.AddDays(expirationDays);
            expirationDetails.RemainingDays = (int)(expirationDetails.ExpirationDate.Date - DateTime.Now.Date).TotalDays;

            if (expirationDetails.RemainingDays < 0)
                expirationDetails.RemainingDays = 0;
            

            if (DateTime.Now > creationDate.AddDays(expirationDays))
            {
                expirationDetails.isExpired = true;
            } else
            {
                expirationDetails.isExpired = false;                
            }

            return expirationDetails;
        }                
    }

    public class ExpirationDetails
    {
        public bool isExpired { get; set; } = false;
        public DateTime ExpirationDate { get; set; }
        public int RemainingDays { get; set; }
    }   
}