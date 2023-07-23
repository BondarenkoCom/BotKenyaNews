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
                    InlineKeyboardButton.WithCallbackData("comming Soon", "section_news_empty")
                    //InlineKeyboardButton.WithCallbackData("Comming Soon", "section_news_empty_t"),
                },
                new[]
                {
                   InlineKeyboardButton.WithUrl("✨ Donate now! ", "https://www.donationalerts.com/r/cyborgnull")
                }
            });

            return newsKeyboard;
        }
    }
}
