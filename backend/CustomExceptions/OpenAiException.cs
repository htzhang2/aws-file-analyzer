namespace OpenAiChat.CustomExceptions
{
    public class OpenAiException: System.Exception
    {
        public OpenAiException(string message) : base(message)
        {
        }
    }
}
