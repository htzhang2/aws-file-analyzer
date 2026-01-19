namespace OpenAiChat.CustomExceptions
{
    public class AwsCloudException: System.Exception
    {
        public AwsCloudException(string message) : base(message)
        {
        }
    }
}
