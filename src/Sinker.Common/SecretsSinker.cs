using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KubeClient;
using KubeClient.Models;
using Microsoft.Extensions.Logging;

namespace Sinker.Common
{
    public class SecretsSinker : IDisposable
    {
        private IKubeApiClient _kubeClient;
        private ILogger<SecretsSinker> _logger;
        private IEnumerable<ISecretsProvider> _providers;
        private SinkerConfiguration _configuration;

        public SecretsSinker(ILogger<SecretsSinker> logger, KubeClientOptions kubeOptions, 
            IEnumerable<ISecretsProvider> providers, SinkerConfiguration configuration)
        {
            _logger = logger;
            _kubeClient = KubeApiClient.Create(kubeOptions);
            _providers = providers;
            _configuration = configuration;
        }

        public async Task UpdateAllSecretsAsync()
        {
            _logger.LogInformation($"Checking all secrets for updates");
            // Update all secrets from providers
            var updatedSecrets = new List<ISecret>();
            foreach(var provider in _providers)
            {
                var secrets = await provider.GetAsync();
                var updatedSecretsForProvider = await UpdateSecretsAsync(secrets);
                updatedSecrets.AddRange(updatedSecretsForProvider);
            }
            // Then purge any abandoned secrets
            await PurgeKubeSecretsAsync(updatedSecrets);
            _logger.LogInformation($"Finished updating all secrets");
        }

        private async Task<IEnumerable<ISecret>> UpdateSecretsAsync(IEnumerable<ISecret> secrets)
        {
            var updatedSecrets = new List<ISecret>();
            foreach(var secret in secrets)
            {
                var updatedSecret = await UpdateSecretAsync(secret);
                updatedSecrets.Add(updatedSecret);
            }
            return updatedSecrets;
        }

        private async Task<ISecret> UpdateSecretAsync(ISecret secret)
        {
            var kubeSecret = await GetKubeSecretAsync(secret);
            if(kubeSecret == null)
            {
                // Create new secret
                var newKubeSecret = CreateNewKubeSecretModel(secret);
                var createdKubeSecret = await _kubeClient.SecretsV1().Create(newKubeSecret);
                return secret;
            }

            // Secret exists and should be updated
            // ToDo: Update value
            return secret;
        }

        private async Task<SecretV1> GetKubeSecretAsync(ISecret secret)
        {
            return await _kubeClient.SecretsV1().Get(secret.Name, kubeNamespace: secret.Namespace);
        }

        private async Task<IEnumerable<SecretV1>> GetAllSecretsAsync()
        {
            var allSecrets = new List<SecretV1>();
            var allNamespaces = await _kubeClient.NamespacesV1().List();
            
            foreach(var ns in allNamespaces)
            {
                var labelSelector = _configuration.Labels.ToLabelSelector();
                var secrets = await _kubeClient.SecretsV1().List(kubeNamespace: ns.Metadata.Name, labelSelector: labelSelector);
                if(secrets.Any())
                {
                    allSecrets.AddRange(secrets);
                }
            }

            return allSecrets;
        }

        private async Task PurgeKubeSecretsAsync(IEnumerable<ISecret> secretsToKeep)
        {
            var allSecrets = await GetAllSecretsAsync();
            var secretsToDelete = allSecrets
                .Where(d => !secretsToKeep
                    .Any(k => k.Name == d.Metadata.Name && k.Namespace == d.Metadata.Namespace))
                .ToList();
            foreach(var secretToDelete in secretsToDelete)
            {
                await PurgeKubeSecretAsync(secretToDelete.Metadata.Name, secretToDelete.Metadata.Namespace);
            }
        }

        private async Task PurgeKubeSecretAsync(string name, string kubeNamespace)
        {  
           var result = await _kubeClient.SecretsV1().Delete(name, kubeNamespace: kubeNamespace);
           if(result.IsSuccess)
           {
               _logger.LogInformation($"Deleted secret {kubeNamespace}/{name}");
               return;
           }
            _logger.LogWarning($"Error deleting secret {kubeNamespace}/{name}, reason: {result.Reason}, message: {result.Message}");
        }

        private SecretV1 CreateNewKubeSecretModel(ISecret secret)
        {
            var kubeSecret = new SecretV1()
            {
                Metadata = new ObjectMetaV1()
                {
                    Name = secret.Name,
                    Namespace = secret.Namespace
                }
            };

            foreach(var label in _configuration.Labels)
            {
                kubeSecret.Metadata.Labels.Add(label.Key, label.Value);
            }
            
            return kubeSecret;
        }

        public void Dispose()
        {
            _kubeClient?.Dispose();
        }
    }
}