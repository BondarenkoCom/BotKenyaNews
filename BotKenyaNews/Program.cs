﻿using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
using BotKenyaNews.Helpers;
using BotKenyaNews.BotSettings;
using BotKenyaNews.RssController;
using BotKenyaNews.Models;
using BotKenyaNews.Parser;
using BotKenyaNews.Interfaces;

namespace BotKenyaNews
{
    public class Program
    {
        static int lastUpdateId = 0;
        static ITelegramBotClient bot = new TelegramBotClient(JsonReader.GetValues().telegramApiToken);
        private static Dictionary<long, DateTime> activeUsers = new Dictionary<long, DateTime>();
        private static Dictionary<long, UserState> UserStates = new Dictionary<long, UserState>();
        private static Dictionary<string, string> articleUrls = new Dictionary<string, string>();
        private static Dictionary<long, HashSet<string>> sentImages = new Dictionary<long, HashSet<string>>();
        private static clientParser _clientParserDriver;
        private static GenerateLog generateLog = new GenerateLog();

        static Program()
        {
            IResponseSorter responseSorter = new ResponseSorter();
            _clientParserDriver = new clientParser(responseSorter);
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
                int activeUsersCount = GetActiveUsersCount(TimeSpan.FromHours(24));
                string logMessage = generateLog.GenerateLogMessage(activeUsersCount, update);
                Console.WriteLine(logMessage);

                Telegram.Bot.Types.Message message = null;
                long chatId = 0;
                long myChatId = 5686406180;

                string updateJson = Newtonsoft.Json.JsonConvert.SerializeObject(update);

                Console.WriteLine($"Active users in Kenya News {GetActiveUsersCount(TimeSpan.FromHours(24))}");

                try
                {
                    await botClient.SendTextMessageAsync(
                        chatId: myChatId,
                        text: logMessage
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Problem with your chatId - {myChatId} \n error message - {ex.Message}");
                }

                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                {
                    message = update.Message;
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

                    var categoryKeyboard = InlineKeyBoardCategory.CreateSectionKeyboard();
                    RssFeedsDriver rssFeedsDriver = new RssFeedsDriver();

                    if (update.CallbackQuery.Data.StartsWith("section_"))
                    {
                        userChoices[chatId] = new UserChoice { Site = update.CallbackQuery.Data };
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Choose category",
                            replyMarkup: categoryKeyboard);
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

                            GPTDriver gPTDriver = new GPTDriver();

                            var sendingGif = await botClient.SendAnimationAsync(
                                         chatId: chatId,
                                         animation: "https://media.giphy.com/media/lXiRLb0xFzmreM8k8/giphy.gif");

                            var res = await _clientParserDriver.RunDriverClient(selectedArticleUrl);
                            var contentFormated = await gPTDriver.RewritePost(res.Item1);

                            int counter = 0;
                            string sixthUrl = null;

                            foreach (var item in res.Item2)
                            {
                                Console.WriteLine($"check url - {item}");
                                if (item.StartsWith("https") && (item.EndsWith(".jpg") || item.EndsWith(".png")
                                    || item.EndsWith(".gif")) && !item.EndsWith("_news.jpg"))
                                {
                                    sixthUrl = item;
                                    break;
                                }
                            }

                            await botClient.DeleteMessageAsync(
                                     chatId: sendingGif.Chat.Id,
                                     messageId: sendingGif.MessageId);

                            try
                            {
                                if (sixthUrl != null)
                                {
                                    if (!sentImages.ContainsKey(chatId) || !sentImages[chatId].Contains(sixthUrl))
                                    {
                                        await botClient.SendPhotoAsync(
                                            chatId: chatId,
                                            photo: sixthUrl
                                        );

                                        if (!sentImages.ContainsKey(chatId))
                                        {
                                            sentImages[chatId] = new HashSet<string>();
                                        }
                                        sentImages[chatId].Add(sixthUrl);
                                    }
                                }

                                await botClient.SendTextMessageAsync(
                                       chatId: chatId,
                                       text: $"Here is the article you selected:\n{contentFormated}\n"
                                );

                                var sectionKeyboard = InlineKeyBoardSites.CreateNewsKeyboard();

                                await botClient.SendTextMessageAsync(
                                      chatId: chatId,
                                      text: "Choose your next news:",
                                      replyMarkup: sectionKeyboard
                                  );

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error sending message: {ex}");
                                Console.WriteLine($"url image - {sixthUrl}");
                            }
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

        public static int GetActiveUsersCount(TimeSpan timeSpan)
        {
            DateTime now = DateTime.UtcNow;
            return activeUsers.Count(user => (now - user.Value) <= timeSpan);
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
                AllowedUpdates = { },
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