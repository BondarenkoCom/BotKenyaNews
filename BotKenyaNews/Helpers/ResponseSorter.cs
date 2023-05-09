using BotKenyaNews.Interfaces;
using HtmlAgilityPack;
using Html2Markdown;

namespace BotKenyaNews.Helpers
{
    public class ResponseSorter : IResponseSorter
    {
        private HtmlDocument LoadHtmlDocument(string htmlContent)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);
            return htmlDoc;
        }

        public string HtmlConverter(string responseSort, string propName)
        {
            var htmlDoc = LoadHtmlDocument(responseSort);
            var htmlElement = htmlDoc.DocumentNode.SelectSingleNode($"//section[@class='{propName}']");

            var content = htmlElement.InnerText.ToString();
            var converter = new Converter();
            
            string markdownText = converter.Convert(content);

            markdownText = TruncateToMaxLength(markdownText, 4096);


            return markdownText;

            //return htmlElement.InnerText.Trim();
        }

        private string TruncateToMaxLength(string text, int maxLength)
        {
            if (text.Length > maxLength)
            {
                return text.Substring(0, maxLength - 3) + "...";
            }
            return text;
        }

        public Dictionary<string, Func<string, string, string>> GetResponseSorterMethods()
        {
            return new Dictionary<string, Func<string, string, string>>
            {
                { "africa.com", (responseSort, propName) => HtmlConverter(responseSort, propName) },
            };
        }
    }
}
