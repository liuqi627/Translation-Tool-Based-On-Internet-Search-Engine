using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace 琦哥的翻译软件
{
    class googleSearch
    {

        public List<Keyword> GetKeywords(string html, string word)
        {
            int i = 1;
            List<Keyword> keywords = new List<Keyword>();

            Regex regTable = new Regex("<h3 class=\"r\"><a.*?href=\"(?<url>.*?)\".*?>(?<content>.*?)</a>", RegexOptions.IgnoreCase);
            MatchCollection mcTable = regTable.Matches(html);
            foreach (Match mTable in mcTable)
            {
                if (mTable.Success)
                {
                    Keyword keyword = new Keyword();
                    keyword.ID = i++;
                    keyword.Title = Regex.Replace(mTable.Groups["content"].Value, "<[^>]*>", string.Empty);
                    keyword.Link = mTable.Groups["url"].Value.Substring(7);
                    keywords.Add(keyword);
                }
            }

            return keywords;
        }
    }
   
}
