using Microsoft.Identity.Client;
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
            var userPrompt = @"
You are an image analysis assistant. 
Analyze the uploaded image and respond ONLY in valid JSON format, with the following schema:

{
  ""city"": string | null,
  ""country"": string | null,
  ""landmark"": string | null,
  ""weather"": string | null,
  ""category"": string,   // e.g. 'architecture', 'nature', 'food', 'people'
}

Rules:
- Return only JSON, no explanation.
- If a field cannot be inferred, use null.
";

            var systemPrompt = $"You are an image analysis assistant.";

            List<ChatMessage> messages =
            [
                new SystemChatMessage(ChatMessageContentPart.CreateTextPart(systemPrompt)),
                new UserChatMessage(
                    ChatMessageContentPart.CreateTextPart(userPrompt),
                    ChatMessageContentPart.CreateImagePart(new Uri(imageUrl)))
            ];

            var options = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
                // Removed Model property assignment, as ChatCompletionOptions does not have a Model property.
            };

            var completionResult = await _chatClient.CompleteChatAsync(messages).ConfigureAwait(false);
            var result = completionResult.Value.Content[0].Text;
            var metaData = System.Text.Json.JsonSerializer.Deserialize<MetaData.ImageMetaData>(result);

            return result;
        }
    }
}
