using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sinker.Common;
using KubeClient;
using KubeClient.Models;
using KubeClient.ResourceClients;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sinker.HostedServices
{
    public class SecretsWatcherHostedService : IHostedService, IDisposable
    {
        
        private static TimeSpan SCHEDULE = TimeSpan.FromSeconds(30);
        private IKubeApiClient _kubeClient;
        private ILogger<SecretsWatcherHostedService> _logger;
        private IEnumerable<ISecretsProvider> _providers;
        private SinkerConfiguration _configuration;
        private Timer _timer;

        public SecretsWatcherHostedService(ILogger<SecretsWatcherHostedService> logger, KubeClientOptions kubeOptions, 
            IEnumerable<ISecretsProvider> providers, IOptions<SinkerConfiguration> configuration)
        {
            _logger = logger;
            _kubeClient = KubeApiClient.Create(kubeOptions);
            _providers = providers;
            _configuration = configuration.Value;
        }

        public void Dispose()
        {
            _logger.LogInformation($"Disposing {nameof(SecretsWatcherHostedService)}");
            _kubeClient.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting {nameof(SecretsWatcherHostedService)}");
            _timer = new Timer(UpdateAllSecrets, null, TimeSpan.Zero, SCHEDULE);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Stopping {nameof(SecretsWatcherHostedService)}");
            return Task.CompletedTask;
        }

        ///////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////

        private async void UpdateAllSecrets(object state)
        {
            var random = new Random().Next();
            _logger.LogInformation($"Checking all secrets for updates ({random})");
            // Update all secrets from providers
            var updatedSecrets = new List<ISecret>();
            foreach(var provider in _providers)
            {
                var secrets = await provider.GetAsync();
                var updatedSecretsForProvider = await UpdateSecrets(secrets);
                updatedSecrets.AddRange(updatedSecretsForProvider);
            }
            // Then purge any abandoned secrets
            await PurgeKubeSecrets(updatedSecrets);
            _logger.LogInformation($"Finished updating all secrets ({random})");
        }

        private async Task<IEnumerable<ISecret>> UpdateSecrets(IEnumerable<ISecret> secrets)
        {
            var updatedSecrets = new List<ISecret>();
            foreach(var secret in secrets)
            {
                var updatedSecret = await UpdateSecret(secret);
                updatedSecrets.Add(updatedSecret);
            }
            return updatedSecrets;
        }

        private async Task<ISecret> UpdateSecret(ISecret secret)
        {
            var kubeSecret = await GetKubeSecret(secret);
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

        private async Task<SecretV1> GetKubeSecret(ISecret secret)
        {
            return await _kubeClient.SecretsV1().Get(secret.Name, kubeNamespace: secret.Namespace);
        }

        private async Task<IEnumerable<SecretV1>> GetAllSecrets()
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

        private async Task PurgeKubeSecrets(IEnumerable<ISecret> secretsToKeep)
        {
            var allSecrets = await GetAllSecrets();
            var secretsToDelete = allSecrets
                .Where(d => !secretsToKeep
                    .Any(k => k.Name == d.Metadata.Name && k.Namespace == d.Metadata.Namespace))
                .ToList();
            foreach(var secretToDelete in secretsToDelete)
            {
                await PurgeKubeSecret(secretToDelete.Metadata.Name, secretToDelete.Metadata.Namespace);
            }
        }

        private async Task PurgeKubeSecret(string name, string kubeNamespace)
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
    }
}