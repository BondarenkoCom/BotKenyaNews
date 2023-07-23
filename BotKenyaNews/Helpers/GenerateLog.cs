using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BotKenyaNews.Helpers
{
    public class GenerateLog
    {
        public string GenerateLogMessage(int activeUsersCount, Update update)
        {
            StringBuilder logMessage = new StringBuilder();

            logMessage.AppendLine($"Active users in Kenya News: {activeUsersCount}");

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                logMessage.AppendLine($"User action: Callback Query");
                logMessage.AppendLine($"User ID: {update.CallbackQuery.From.Id}");
                logMessage.AppendLine($"User Name: {update.CallbackQuery.From.Username}");
                logMessage.AppendLine($"Callback Data: {update.CallbackQuery.Data}");
            }
            else if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                logMessage.AppendLine($"User action: Message");
                logMessage.AppendLine($"User ID: {update.Message.From.Id}");
                logMessage.AppendLine($"User Name: {update.Message.From.Username}");
                logMessage.AppendLine($"Message Text: {update.Message.Text}");
            }

            return logMessage.ToString();
        }
    }
}
