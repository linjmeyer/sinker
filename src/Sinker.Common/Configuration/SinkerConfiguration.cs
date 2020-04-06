using System.Collections.Generic;

namespace Sinker.Common
{
    public class SinkerConfiguration
    {
        /// <summary>
        /// Labels that are used when creating and querying for Kubernetes resources managed by Sinker.
        /// Default is managed-by-sinker=true.
        /// </summary>
        public Dictionary<string,string> Labels { get; set; } = new Dictionary<string,string>() 
        {
            { "managed-by-sinker", "true" }
        };

        /// <summary>
        /// How frequently the secrets should be syncronized in seconds.
        /// Default is 30
        /// </summary>
        public int RefreshInterval { get; set; } = 30;
    }
}
