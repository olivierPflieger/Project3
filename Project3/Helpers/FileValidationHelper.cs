namespace Project3.Helpers
{
    public class FileValidationHelper
    {
        public static string[] GetAllowedExtensions()
        {
            return new[]
            {
                ".jpg",".jpeg",".png",".gif",".bmp",".pdf",".docx",".xlsx",".txt",".csv",".json",".xml"
            };
        }
        
        public static readonly Dictionary<string, byte[]> ExpectedMagicNumbers = new()
        {
            // Images
            { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
            { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
            { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47 } },
            { ".gif", new byte[] { 0x47, 0x49, 0x46, 0x38 } },
            { ".bmp", new byte[] { 0x42, 0x4D } },
            { ".webp", new byte[] { 0x52, 0x49, 0x46, 0x46 } },

            // Documents
            { ".pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 } },
            { ".docx", new byte[] { 0x50, 0x4B, 0x03, 0x04 } },
            { ".xlsx", new byte[] { 0x50, 0x4B, 0x03, 0x04 } },

            // Textes
            { ".txt", new byte[] { } },
            { ".csv", new byte[] { } },
            { ".json", new byte[] { } },
            { ".xml", new byte[] { } },

            // Audio / Video
            { ".mp3", new byte[] { 0xFF, 0xFB } },
            { ".wav", new byte[] { 0x52, 0x49, 0x46, 0x46 } },
            { ".mp4", new byte[] { 0x00, 0x00, 0x00 } }            
        };

        public static readonly Dictionary<string, string> ExpectedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            // Images
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".bmp", "image/bmp" },
            { ".webp", "image/webp" },

            // Documents
            { ".pdf", "application/pdf" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },

            // Textes
            { ".txt", "text/plain" },
            { ".csv", "text/csv" },
            { ".json", "application/json" },
            { ".xml", "application/xml" },

            // Audio / Video
            { ".mp3", "audio/mpeg" },
            { ".wav", "audio/wav" },
            { ".mp4", "video/mp4" }
        };

        /// <summary>
        /// Vérifie si le fichier correspond à sa signature (magic number)
        /// </summary>
        /// <param name="fileStream">Stream du fichier</param>
        /// <param name="extension">Extension du fichier (.jpg, .png, etc.)</param>
        /// <returns>true si la signature est valide</returns>
        public static async Task<bool> IsValidFile_MagicNumberAsync(Stream fileStream, string extension)
        {
            extension = extension.ToLowerInvariant();

            // Extension non autorisée
            if (!ExpectedMagicNumbers.ContainsKey(extension))
                return false;

            byte[] magic = ExpectedMagicNumbers[extension];

            // Pas de signature à vérifier (txt, csv, json, xml)
            if (magic.Length == 0)
                return true;

            byte[] buffer = new byte[magic.Length];

            // Lire seulement les premiers octets
            int bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);

            // Remet le stream au début pour qu’il puisse être lu ensuite (S3 upload)
            if (fileStream.CanSeek)
                fileStream.Seek(0, SeekOrigin.Begin);

            return bytesRead == magic.Length && buffer.SequenceEqual(magic);
        }            
    }
}