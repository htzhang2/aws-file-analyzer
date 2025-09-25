using HtmlAgilityPack;
using OpenAI.Chat;
using System.Text.RegularExpressions;

namespace OpenAiChat.Services
{
    public class TextService : ITextService
    {
        private const int MAX_TEXT_BYTES = 1000;

        private readonly ChatClient _chatClient;
        private readonly HttpClient _httpClient;

        public TextService(ChatClient chatClient, HttpClient httpClient)
        {
            _chatClient = chatClient;
            _httpClient = httpClient;
        }

        public async Task<string> SummarizeTextAsync(string textUrl)
        {
            string html = await _httpClient.GetStringAsync(textUrl);

            // 2. Extract clean text
            var doc = new HtmlDocument();

            doc.LoadHtml(html);

            string rawText = doc.DocumentNode.InnerText;

            // Replace multiple whitespace (spaces, tabs, newlines) with a single space
            string inputText = Regex.Replace(rawText, @"\s+", " ").Trim();

            // Only process 1st chunk of large document
            if (inputText.Length > MAX_TEXT_BYTES)
            {
                inputText = inputText.Substring(0, MAX_TEXT_BYTES);
            }

            try
            {

                //ChatCompletion completion = await _chatClient.CompleteChatAsync(prompt).ConfigureAwait(false);

                // Ask the model to summarize
                var response = await _chatClient.CompleteChatAsync(
                    new[]
                    {
                    new UserChatMessage($"Summarize this text in 3 sentences:\n\n{inputText}")
                    });

                return response.Value.Content[0].Text;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
