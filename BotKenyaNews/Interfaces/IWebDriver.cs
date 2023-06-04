namespace BotKenyaNews.Interfaces
{
    public interface IWebDriver
    {
        Task<(string, List<string>)> RunDriverClient(string url);
    }
}
