using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Payments;
using Telegram.Bot.Requests;
using BotKenyaNews.Helpers;
using BotKenyaNews.BotSettings;
using BotKenyaNews.RssController;

namespace BotKenyaNews
{
    public class Program
    {
        static int lastUpdateId = 0;
        static ITelegramBotClient bot = new TelegramBotClient(JsonReader.GetValues().telegramApiToken);
        private static Dictionary<long, DateTime> LastActiveUser = new Dictionary<long, DateTime>();

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                // Некоторые действия
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                {
                    var message = update.Message;
                    string userName = string.Empty;

                    if (!string.IsNullOrEmpty(message.Chat.FirstName))
                    {
                        userName = message.Chat.FirstName;
                    }
                    else if (!string.IsNullOrEmpty(message.Chat.LastName))
                    {
                        userName = message.Chat.LastName;
                    }
                    else if (!string.IsNullOrEmpty(message.Chat.Username))
                    {
                        userName = message.Chat.Username;
                    }
                    else
                    {
                        userName = "Unknown";
                    }


                    if (message.Text.ToLower() == "/start")
                    {
                        string welcomeMessage = $"🌍✨ Welcome {userName} to our African news aggregator bot! ✨🌍\n\nWe gather the latest news from multiple sources across Africa, bringing you the most relevant and up-to-date information. 📰🌍\n\nPlease choose your preferred country or topic to get started: 🌟";

                        //var sendingGif = await botClient.SendAnimationAsync(
                        //    chatId: message.Chat.Id,
                        //    animation: "https://media.giphy.com/media/Kbc5SZgO7re8/giphy.gif");

                        var newsKeyboard = ReplyKeyboardTelegram.CreateNewsKeyboard();
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: welcomeMessage,
                            replyMarkup: newsKeyboard);
                    }
                    else
                    {
                        string newsMatrix = message.Text.Trim().ToLower();
                        switch (newsMatrix)
                        {
                            case "africanews.com":
                            case "news empty":
                            case "news empty two":
                                {
                                    if (LastActiveUser.TryGetValue(message.Chat.Id, out DateTime ActiveUser))
                                    {
                                        if (DateTime.UtcNow.Date == ActiveUser.Date)
                                        {
                                            var sendingGifDanger = await botClient.SendAnimationAsync(
                                            chatId: message.Chat.Id,
                                            animation: "https://media.giphy.com/media/lXiRLb0xFzmreM8k8/giphy.gif");

                                            await botClient.SendTextMessageAsync(
                                                chatId: message.Chat.Id,
                                                text: $"The universe only answers you once a day, don't try to force it, dear {message.Chat.FirstName}.");
                                            return;
                                        }
                                    }

                                    var sendingGif = await botClient.SendAnimationAsync(
                                        chatId: message.Chat.Id,
                                        animation: "https://media.giphy.com/media/lXiRLb0xFzmreM8k8/giphy.gif");

                                    try
                                    {
                                        // Generate the horoscope
                                        //Make request to news
                                        //var generatedHoroscope = await gptDriver.GenerateHoroscope(zodiacSign, false, userName);
                                        RssFeedsDriver rssFeedsDriver = new RssFeedsDriver();
                                        var rssRequest = await rssFeedsDriver.GetNews();

                                        string textNews = rssRequest;
                                        await botClient.DeleteMessageAsync(
                                              chatId: sendingGif.Chat.Id,
                                              messageId: sendingGif.MessageId);

                                        // Send the horoscope
                                        await botClient.SendTextMessageAsync(
                                            chatId: message.Chat.Id,
                                            text: textNews);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Error generating horoscope: " + ex.ToString());
                                        await botClient.SendTextMessageAsync(
                                            chatId: message.Chat.Id,
                                            text: "Sorry, we encountered an error generating your horoscope. Please try again later.");

                                    }

                                    LastActiveUser[message.Chat.Id] = DateTime.UtcNow;
                                }
                                break;
                            case "🌟✨ donate now! 💖":
                                {
                                    var price = new List<LabeledPrice>
                                    {
                                        new LabeledPrice("Donate", 1),
                                    };

                                    var invoice = new SendInvoiceRequest(
                                        chatId: message.Chat.Id,
                                        title: "Donate",
                                        description: "Donate for us",
                                        payload: "unique_invoice_id", // unique invoice identifier
                                        providerToken: "provider_token", // your Payoneer API token
                                        currency: "USD", // currency
                                        prices: price
                                    );
                                    await botClient.SendInvoiceAsync(
                                        chatId: invoice.ChatId,
                                        title: invoice.Title,
                                        description: invoice.Description,
                                        payload: invoice.Payload,
                                        providerToken: invoice.ProviderToken,
                                        currency: invoice.Currency,
                                        prices: invoice.Prices
                                    );
                                }
                                break;
                            default:
                                await botClient.SendTextMessageAsync(message.Chat, "Sorry, I didn't understand that.");
                                break;
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