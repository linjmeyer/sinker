using System;

namespace Sinker.Common
{
    public interface ISecret
    {
        string SecretProvider { get; }
        string Value { get; }
        string Version { get; }
        string Name { get; }
        string Namespace { get; }
        DateTime Retreived { get; }
    }
}
