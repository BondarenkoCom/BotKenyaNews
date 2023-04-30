using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using System.Linq;
using BotKenyaNews.Models;

namespace BotKenyaNews.RssController
{
    public class RssFeedsDriver
    {
        Dictionary<string, string> rssDictionary = new Dictionary<string, string>
        {
            { "section_africanews.com", "http://www.africanews.com/feed/rss" },
            { "source2", "http://www.source2.com/feed/rss" },
        };

        public async Task<List<Article>> GetNews(string webResourse, string webCategory)
        {
            Console.WriteLine($"is site - {webResourse}, is category - {webCategory}");
            var httpClient = new HttpClient();

            if (!rssDictionary.TryGetValue(webResourse, out var url))
            {
                Console.WriteLine("I can't find url " + webResourse);
                return null;
            }

            try
            {
                //Later get url from Dictionary
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = XmlReader.Create(stream))
                {
                    var feed = SyndicationFeed.Load(reader);

                    StringBuilder sb = new StringBuilder();

                    int maxItems = 10;
                    List<Article> articles = new List<Article>();

                    foreach (var item in feed.Items.Take(maxItems))
                    {
                        Article article = new Article
                        {
                            Title = item.Title.Text,
                            Url = item.Links[0].Uri.ToString()
                        };
                        articles.Add(article);
                    }

                    return articles;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"sorry we have a problem - {ex.Message}");
                return new List<Article>(); // Return an empty list in case of an error
            }
        }

        public async Task<Article> GetArticleByUrl(string url)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var reader = XmlReader.Create(stream))
            {
                var feed = SyndicationFeed.Load(reader);

                var item = feed.Items.FirstOrDefault(i => i.Links[0].Uri.ToString() == url);

                if (item == null)
                {
                    return null;
                }

                var article = new Article
                {
                    Title = item.Title.Text,
                    Url = item.Links[0].Uri.ToString(),
                    Author = item.Authors.FirstOrDefault()?.Name,
                    Summary = item.Summary?.Text,
                    PublishedDate = item.PublishDate.UtcDateTime.ToString()
                };

                return article;
            }
        }


        private IEnumerable<SyndicationItem> FilterFeedItemsByCategory(IEnumerable<SyndicationItem> items, string category)
        {
            return items.Where(item => item.Categories.Any(cat => cat.Name.ToLowerInvariant() == category.ToLowerInvariant()));
        }
    }
}
