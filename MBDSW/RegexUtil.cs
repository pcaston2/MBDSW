using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MBDSW
{
    public class RegexUtil
    {
        public static string FindInString(string content, string pattern, string groupName)
        {
            var r = new Regex(pattern);
            var match = r.Match(content);
            var groups = match.Groups;
            if (groups.ContainsKey(groupName))
            {
                return groups[groupName].Value;
            } else
            {
                throw new KeyNotFoundException($"Could not find {groupName} match in regular expression");
            }

        }
    }
}
