using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace BotKenyaNews.HtmlParser
{
    public class ResponseHtmlReader
    {
        private HtmlDocument LoadHtmlDocument(string htmlContent)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);
            return htmlDoc;
        }


        public string HtmlConverterForAdricaNews(string responseSort, string propName)
        {
            var htmlDoc = LoadHtmlDocument(responseSort);
            var htmlElement = htmlDoc.DocumentNode.SelectSingleNode($"//div[@class='{propName}']");

            return htmlElement?.InnerText ?? $"Element {propName} is null (Empty)";
        }

    }
}
