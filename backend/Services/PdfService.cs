using OpenAI.Chat;
using System.Text;
using UglyToad.PdfPig;

namespace OpenAiChat.Services
{
    public class PdfService : IPdfService
    {
        private const int MAX_FILE_SIZE_IN_ONE_CHUNK = 12000;
        private const int CHUNK_SIZE_IN_BYTES = 4000;

        private readonly ChatClient _chatClient;
        private readonly HttpClient _httpClient;

        public PdfService(ChatClient chatClient, HttpClient httpClient)
        {
            _chatClient = chatClient;
            _httpClient = httpClient;
        }

        public async Task<string> SummarizePdfAsync(string pdfUrl)
        {
            using var response = await _httpClient.GetAsync(pdfUrl);

            // 2. Extract stream since pdf is binary
            response.EnsureSuccessStatusCode();
            await using var pdfStream = await response.Content.ReadAsStreamAsync();

            string text = ExtractTextFromPdfStream(pdfStream);

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new Exception("No text extracted from PDF.");
            }


            // Chunk if too long
            if (text.Length > MAX_FILE_SIZE_IN_ONE_CHUNK)
            {
                var chunks = text.Chunk(CHUNK_SIZE_IN_BYTES).Select(c => new string(c)).ToList();
                var partialSummaries = new List<string>();
                foreach (var chunk in chunks)
                {
                    partialSummaries.Add(await SummarizeTextAsync(chunk));
                }
                return await SummarizeTextAsync(string.Join("\n", partialSummaries));
            }
            else
            {
                return await SummarizeTextAsync(text);
            }
        }

        private string ExtractTextFromPdfStream(System.IO.Stream pdfStream)
        {
            var sb = new StringBuilder();
            using (var pdf = PdfDocument.Open(pdfStream))
            {
                foreach (var page in pdf.GetPages())
                {
                    sb.AppendLine(page.Text);
                }
            }
            return sb.ToString();
        }

        private async Task<string> SummarizeTextAsync(string inputText)
        {
            var systemPrompt = $"You are an PDF assistant that outputs only strict JSON.";
            var userPrompt = @"
Summarize this PDF and output valid JSON in this format:
{
    ""caption"": ""string"",
    ""summary"": ""string"",
    ""highlights"": [""string""],
    ""keywords"": [""string""],
    ""sentiment"": ""positive|neutral|negative""
}
Rules:
- caption should be one sentence describing the overall topic of the PDF.
- summary should be 2-3 sentences summarizing the main points.
";

            try
            {
                // Ask the model to summarize
                var response = await _chatClient.CompleteChatAsync(
                    new[]
                    {
                        new UserChatMessage($"{userPrompt}:\n\n{inputText}")
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
