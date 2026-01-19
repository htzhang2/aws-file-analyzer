namespace OpenAiChat.CustomExceptions
{
    public class UserSetupException : System.Exception
    {
        public UserSetupException(string message) : base(message)
        {
        }
    }
}
