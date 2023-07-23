using Telegram.Bot.Types.ReplyMarkups;

namespace BotKenyaNews.BotSettings
{
    internal class InlineKeyBoardCategory
    {
        public static InlineKeyboardMarkup CreateSectionKeyboard()
        {
            return new InlineKeyboardMarkup(new[]
            {
        new []
        {
            InlineKeyboardButton.WithCallbackData("last ten news", "category_economy")
            //InlineKeyboardButton.WithCallbackData("sports", "category_sports"),
        },
    });
        }
    }
}
