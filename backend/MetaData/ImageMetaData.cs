namespace OpenAiChat.MetaData
{
    public class ImageMetaData
    {
        public string city { get; set; } = string.Empty;
        public string region { get; set; } = string.Empty;
        public string country { get; set; } = string.Empty;
        public string landmark { get; set; } = string.Empty;
        public string weather { get; set; } = string.Empty;
        public string category { get; set; } = string.Empty;
        public string caption { get; set; } = string.Empty;
        public float confidence { get; set; }
        public string justification { get; set; } = string.Empty;
    }
}
