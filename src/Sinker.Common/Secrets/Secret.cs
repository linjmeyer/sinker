using System;

namespace Sinker.Common
{
    public class Secret : ISecret
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string SecretProvider { get; set; }

        public string Value { get; set; }

        public string Version { get; set; }

        public DateTime Retreived { get; set; }
    }
}