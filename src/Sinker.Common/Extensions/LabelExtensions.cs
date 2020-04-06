using System.Collections.Generic;
using System.Text;

namespace Sinker.Common
{
    public static class LabelExtensions
    {
        public static string ToLabelSelector(this Dictionary<string,string> dictionary)
        {
            var prependCommon = false;
            var builder = new StringBuilder();
            foreach(var item in dictionary)
            {
                if(prependCommon) builder.Append(",");
                builder.Append($"{item.Key}={item.Value}");
            }
            return builder.ToString();
        }
    }
}