using System.Collections.Generic;

namespace Sinker.Common
{
    public class SinkerConfiguration
    {
        public Dictionary<string,string> Labels { get; set; } = new Dictionary<string,string>() {
            { "managed-by-sinker", "true" }
        };
    }
}
