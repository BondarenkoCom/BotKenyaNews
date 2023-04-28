using BotKenyaNews.Models;
using Newtonsoft.Json;
using System.Reflection;

namespace BotKenyaNews.Helpers
{
    public class JsonReader
    {
        public static TelegramApiModels? GetValues()
        {
            try
            {
                string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string jsonFilePath = Path.Combine(basePath, "Datas", "values.json");

                if (!File.Exists(jsonFilePath))
                    return null;

                string json = File.ReadAllText(jsonFilePath);

                return JsonConvert.DeserializeObject<TelegramApiModels>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing element at path {ex.ToString()}");
                return null;
            }
        }
    }
}
