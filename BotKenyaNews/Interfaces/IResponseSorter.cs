namespace BotKenyaNews.Interfaces
{
    public interface IResponseSorter
    {
        string HtmlConverter(string responseSort, string propName);
        Dictionary<string, Func<string, string, string>> GetResponseSorterMethods();

    }
}
