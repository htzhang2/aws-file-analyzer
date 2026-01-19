namespace OpenAiChat.CustomExceptions
{
    public class AzureCloudException: System.Exception
    {
        public AzureCloudException(string message) : base(message)
        {
        }
    }
}
