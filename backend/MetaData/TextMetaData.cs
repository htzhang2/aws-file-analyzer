namespace OpenAiChat.MetaData
{
    public class TextMetaData
    {
        public string caption { get; set; } = string.Empty;
        public string summary { get; set; } = string.Empty;
        public List<string> highlights { get; set; } = new();
        public List<string> keywords { get; set; } = new();
        public string sentiment { get; set; } = string.Empty;
    }
}
