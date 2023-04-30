using BotKenyaNews.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotKenyaNews.Helpers
{
    public static class InlineKeyBoardArticles
    {
        public static InlineKeyboardMarkup CreateArticleKeyboard(List<Article> articles)
        {
            var rows = new List<InlineKeyboardButton[]>();
    
            foreach (var article in articles)
            {
                var button = new InlineKeyboardButton(article.Title)
                {
                    Url = article.Url
                };
                rows.Add(new[] { button });
            }
            return new InlineKeyboardMarkup(rows);
        }
    }
}
