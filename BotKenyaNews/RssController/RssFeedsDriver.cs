using System;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Xml;

namespace BotKenyaNews.RssController
{
    public class RssFeedsDriver
    {
        Dictionary<string, string> rssDictionary = new Dictionary<string, string>();

        public async Task<string> GetNews()
        {
            var httpClient = new HttpClient();
            //url
            var url = "http://www.africanews.com/feed/rss";
            try
            {
                //Later get url from Dictionary
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = XmlReader.Create(stream))
                {
                    var feed = SyndicationFeed.Load(reader);

                    Console.WriteLine("RSS Feed:");
                    foreach (var item in feed.Items)
                    {
                        Console.WriteLine(item.Content);
                        return item.Title.Text;
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }


            return null;
        }

    }
}
