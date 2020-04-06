using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sinker.Common;
using KubeClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sinker.HostedServices
{
    public class SecretsWatcherHostedService : IHostedService, IDisposable
    {
        private ILogger<SecretsWatcherHostedService> _logger;
        private ILoggerFactory _loggerFactory;
        private KubeClientOptions _kubeOptions;
        private IEnumerable<ISecretsProvider> _providers;
        private SinkerConfiguration _configuration;
        private Timer _timer;
        private SecretsSinker _sinker;

        public SecretsWatcherHostedService(ILoggerFactory loggerFactory, KubeClientOptions kubeOptions, 
            IEnumerable<ISecretsProvider> providers, IOptions<SinkerConfiguration> configuration)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<SecretsWatcherHostedService>();
            _kubeOptions = kubeOptions;
            _providers = providers;
            _configuration = configuration.Value;
        }

        public void Dispose()
        {
            _logger.LogInformation($"Disposing {nameof(SecretsWatcherHostedService)}");
            _sinker?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var sinkerLogger = _loggerFactory.CreateLogger<SecretsSinker>();
            var schedule = TimeSpan.FromSeconds(_configuration.RefreshInterval);
         
            _logger.LogInformation($"Starting {nameof(SecretsWatcherHostedService)} with a refresh rate of {schedule}");
         
            _sinker = new SecretsSinker(sinkerLogger, _kubeOptions, _providers, _configuration);
            _timer = new Timer(async (object state) => { await _sinker.UpdateAllSecretsAsync(); }, null, TimeSpan.Zero, schedule);
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Stopping {nameof(SecretsWatcherHostedService)}");
            return Task.CompletedTask;
        }
        
    }
}