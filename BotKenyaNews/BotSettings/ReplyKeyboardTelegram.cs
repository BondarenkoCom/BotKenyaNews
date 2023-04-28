using Telegram.Bot.Types.ReplyMarkups;

namespace BotKenyaNews.BotSettings
{
    public class ReplyKeyboardTelegram
    {
        public static ReplyKeyboardMarkup CreateNewsKeyboard()
        {
            var newsKeyboard = new ReplyKeyboardMarkup(
                new[]
                {
                    new []
                    {
                        new KeyboardButton("africanews.com"),
                        new KeyboardButton("news empty"),
                        new KeyboardButton("news empty two"),
                    },
                    new[]
                    {
                        new KeyboardButton("✨ Donate now! ")
                    }
                });

            newsKeyboard.ResizeKeyboard = true;
            newsKeyboard.OneTimeKeyboard = false;
            newsKeyboard.Selective = true;

            return newsKeyboard;
        }
    }
}
