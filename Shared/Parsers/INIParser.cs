using System.Collections.Generic;
using System.IO;

namespace Shared.Parsers
{
    public static class INIParser
    {
        public static IDictionary<string, string> ParseIni(TextReader textReader)
        {
            var dictionary = new Dictionary<string, string>();
            
            string line;
            while ((line = textReader.ReadLine()) != null)
            {
                var split = line.Split('=', 2);
                if (split.Length != 2)
                    continue;
                var key = split[0];
                var value = split[1];
                dictionary.Add(key, value);
            }

            return dictionary;
        }
    }
}
