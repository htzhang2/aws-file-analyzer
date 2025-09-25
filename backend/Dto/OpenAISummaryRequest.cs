namespace OpenAiChat.Dto
{
    public class OpenAISummaryRequest
    {
        // Property name MUST match the key in your UI Axios payload ("fileUrl")
        public string fileUrl { get; set; }
    }
}
