using BotKenyaNews.Helpers;
using BotKenyaNews.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotKenyaNews.ParsingDictionary
{
    internal class AfricaNewsResponseSorter
    {
        private ResponseSorter _responseSorter;

        public AfricaNewsResponseSorter()
        {
            _responseSorter = new ResponseSorter();
        }

        public string HtmlConverter(string responseSort, string propName)
        {
            return _responseSorter.HtmlConverter(responseSort, propName);
        }

        //public Dictionary<string, Func<string, string, string>> GetResponseSorterMethods()
        //{
        //    return new Dictionary<string, Func<string, string, string>>
        //    {
        //        { "doctu.ru", (responseSort, propName) => HtmlConverter(responseSort, propName) },
        //    };
        //}
    }
}
