namespace DataShare_API.DTO
{
    /// <summary>
    /// Modèle de requête pour le téléchargement d'un fichier.
    /// </summary>
    public class DownloadFileRequest
    {
        /// <summary>
        /// Mot de passe requis pour accéder au fichier protégé par mot de passe). Si le fichier n'est pas protégé, ce champ peut être laissé vide ou null.
        /// </summary>
        public string? Password { get; set; }
    }
}