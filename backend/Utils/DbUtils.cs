using OpenAiChat.Data;

namespace OpenAiChat.Utils
{
    public class DbUtils
    {
        public static async Task<bool> IsDbConnectionStringGood(FileUploadEfDbContext dbContext)
        {
            bool isConnectionStringGood = await dbContext.Database.CanConnectAsync().ConfigureAwait(false);

            return isConnectionStringGood;
        }
    }
}
