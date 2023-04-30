using Telegram.Bot.Types.ReplyMarkups;

namespace BotKenyaNews.BotSettings
{
    public class InlineKeyBoardSites
    {
        public static InlineKeyboardMarkup CreateNewsKeyboard()
        {
            var newsKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("africanews.com", "section_africanews.com"),
                    InlineKeyboardButton.WithCallbackData("news empty", "section_news_empty"),
                    InlineKeyboardButton.WithCallbackData("news empty t", "section_news_empty_t"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("✨ Donate now! ", "donate")
                }
            });

            return newsKeyboard;
        }
    }
}
