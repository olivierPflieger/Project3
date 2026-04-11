using System.Net;

namespace Project3.Exceptions
{
    public class CustomDatashareException : Exception
    {
        public HttpStatusCode Code { get; }
        public string Message { get; }

        public CustomDatashareException(HttpStatusCode code, string message)
            : base($"Erreur {code} : '{message}'")
        {
            Code = code;
            Message = message;
        }                
    }
}
