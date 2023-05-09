using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Payments;
using Telegram.Bot.Requests;
using BotKenyaNews.Helpers;
using BotKenyaNews.BotSettings;
using BotKenyaNews.RssController;
using System.Collections.Generic;
using System.Threading;
using BotKenyaNews.Models;
using BotKenyaNews.Parser;
using BotKenyaNews.Interfaces;

namespace BotKenyaNews
{
    public class Program
    {
        static int lastUpdateId = 0;
        static ITelegramBotClient bot = new TelegramBotClient(JsonReader.GetValues().telegramApiToken);
        private static Dictionary<long, DateTime> LastActiveUser = new Dictionary<long, DateTime>();
        private static Dictionary<long, UserState> UserStates = new Dictionary<long, UserState>();
        private static Dictionary<string, string> articleUrls = new Dictionary<string, string>();
        private static clientParser _clientParserDriver;

        static Program()
        {
            IResponseSorter responseSorter = new ResponseSorter();
            _clientParserDriver = new clientParser(responseSorter); // Pass the responseSorter instance to the constructor
        }

        private static Dictionary<long, UserChoice> userChoices = new Dictionary<long, UserChoice>();

        public class UserState
        {
            public string NewsSection { get; set; }
            public string TimePeriod { get; set; }
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                Message message = null;
                long chatId = 0;

                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                {
                    message = update.Message; // Добавьте эту строку
                    chatId = message.Chat.Id;

                    if (message.Text == "/start")
                    {
                        string userName = message.From.FirstName;
                        string welcomeMessage = $"🌍✨ Welcome {userName} to our African news aggregator bot! ✨🌍\n\nWe gather the latest news from multiple sources across Africa, bringing you the most relevant and up-to-date information. 📰🌍\n\nPlease choose your preferred country or topic to get started: 🌟";
                        var sectionKeyboard = InlineKeyBoardSites.CreateNewsKeyboard();
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: welcomeMessage,
                            replyMarkup: sectionKeyboard);
                    }
                }
                else if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
                {
                    chatId = update.CallbackQuery.Message.Chat.Id;

                    Console.WriteLine(update.CallbackQuery.Data);

                    var categoryKeyboard = InlineKeyBoardCategory.CreateSectionKeyboard();
                    RssFeedsDriver rssFeedsDriver = new RssFeedsDriver();

                    if (update.CallbackQuery.Data.StartsWith("section_"))
                    {
                        userChoices[chatId] = new UserChoice { Site = update.CallbackQuery.Data };
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Choose category",
                            replyMarkup: categoryKeyboard);
                        Console.WriteLine($"{categoryKeyboard} is active category");
                    }
                    else if (update.CallbackQuery.Data.StartsWith("category_"))
                    {
                        string selectedCategory = update.CallbackQuery.Data.Substring("category_".Length);
                        userChoices[chatId].Category = selectedCategory;

                        var sendingGif = await botClient.SendAnimationAsync(
                                           chatId: chatId,
                                           animation: "https://media.giphy.com/media/lXiRLb0xFzmreM8k8/giphy.gif");

                        List<Article> articles = await rssFeedsDriver.GetNews(userChoices[chatId].Site, userChoices[chatId].Category);
                        var articleKeyboard = InlineKeyBoardArticles.CreateArticleKeyboard(articles, articleUrls);

                        await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: "Here are the latest articles:",
                           replyMarkup: articleKeyboard);

                        await botClient.DeleteMessageAsync(
                                       chatId: sendingGif.Chat.Id,
                                       messageId: sendingGif.MessageId);
                    }
                    else if (update.CallbackQuery.Data.StartsWith("article_"))
                    {
                        string articleKey = update.CallbackQuery.Data;
                        if (articleUrls.TryGetValue(articleKey, out string selectedArticleUrl))
                        {
                            Console.WriteLine($"selected article - {selectedArticleUrl}");

                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: $"Here is the link to the article you selected:\n{selectedArticleUrl}");
                            //Тут надо вызывать парсер и читать URL
                            //Придумать как аккуратно писать текст пользователю

                            //http://www.africanews.com/2023/05/07/battles-rage-in-khartoum-ahead-of-talks-in-sarabia/

                            var res = await _clientParserDriver.RunDriverClient(selectedArticleUrl);
                            Console.WriteLine(res);

                            await botClient.SendTextMessageAsync(
                               chatId: chatId,
                               text: $"Here is the article you selected:\n{res}");
                        }
                        else
                        {
                            Console.WriteLine($"Article not found for key: {articleKey}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in HandleUpdateAsync: " + ex.ToString());
                return;
            }
        }


        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ApiRequestException apiRequestException && apiRequestException.ErrorCode == 403)
            {
                Console.WriteLine("Bot was blocked by user.");
                return;
            }
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Bot is online");
            Console.WriteLine("Bot is start " + bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }
    }
}