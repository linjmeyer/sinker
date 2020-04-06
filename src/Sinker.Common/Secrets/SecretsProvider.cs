using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinker.Common
{
    public class SecretsProvider : ISecretsProvider
    {
        public Task<IEnumerable<ISecret>> GetAsync()
        {
            var secrets = new List<ISecret>();
            var random = new Random();

            for(var i=0; i < 5; i++)
            {
                var secret = new Secret();
                secret.Name = "secret-" + random.Next();
                secret.Namespace = "default";
                secret.Retreived = DateTime.Now;
                secret.SecretProvider = "DEBUG";
                secret.Version = "1";
                secret.Value = "enhance$$$";

                secrets.Add(secret);
            }
            
            return Task.FromResult(secrets as IEnumerable<ISecret>);
        }
    }
}