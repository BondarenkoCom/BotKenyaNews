using BotKenyaNews.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotKenyaNews.Helpers
{
    public static class InlineKeyBoardArticles
    {
        public static InlineKeyboardMarkup CreateArticleKeyboard(List<Article> articles, Dictionary<string, string> articleUrls)
        {
            var rows = new List<InlineKeyboardButton[]>();
            int articleId = 1;

            foreach (var article in articles)
            {
                string articleKey = "article_" + articleId;
                articleUrls[articleKey] = article.Url;

                var button = new InlineKeyboardButton(article.Title)
                {
                    CallbackData = articleKey
                };
                rows.Add(new[] { button });
                articleId++;
            }
            return new InlineKeyboardMarkup(rows);
        }
    }
}
