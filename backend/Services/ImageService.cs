using OpenAI.Chat;

namespace OpenAiChat.Services
{
    public class ImageService : IImageService
    {
        private readonly ChatClient _chatClient;

        public ImageService(ChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task<string> AnalyzeImageAsync(string imageUrl)
        {
            var systemPrompt = "Where was this photo taken? Please be as specific as possible, including the city, country, and a brief justification.";

            var userMessage = $"Analyze this image for geolocation information: {imageUrl}";

            List<ChatMessage> messages =
            [
                new UserChatMessage(
                    ChatMessageContentPart.CreateTextPart(systemPrompt),
                    ChatMessageContentPart.CreateImagePart(new Uri(imageUrl)))

            ];

            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages).ConfigureAwait(false);
            var result = completion.Content[0].Text;

            return result;
        }
    }
}
