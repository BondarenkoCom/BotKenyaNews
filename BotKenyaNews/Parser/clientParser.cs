using BotKenyaNews.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotKenyaNews.Parser
{
    public class clientParser : IWebDriver
    {
        private readonly IResponseSorter _responseSorter;
        public clientParser(IResponseSorter responseSorter)
        {
            _responseSorter = responseSorter;
        }

        public async Task<string> RunDriverClient(string url)
        {
            var uri = new Uri(url);
            var host = uri.Host;

            if (host.Contains("www"))
            {
                host = host.Replace("www.", "");
            }

            var client = new HttpClient();

            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.82 Safari/537.36");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
            };

            using (var response = await client.SendAsync(request))
            {
                // Получение содержимого страницы как строки
                string content = await response.Content.ReadAsStringAsync();

                var responseSorterMethods = _responseSorter.HtmlConverter(content, "article__body");
                return responseSorterMethods.ToString();
            }
            return null;
        }
    }
}
