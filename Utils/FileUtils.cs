namespace Project3.Utils
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
    }
}