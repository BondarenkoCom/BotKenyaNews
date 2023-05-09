namespace BotKenyaNews.Interfaces
{
    public interface IWebDriver
    {
        Task<string> RunDriverClient(string url);
    }
}
