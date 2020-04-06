using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sinker.Extensions
{
    public static class TaskExtensions
    {
        // https://stackoverflow.com/questions/12803012/fire-and-forget-with-async-vs-old-async-delegate
        public static async void FireAndForget(this Task task, ILogger logger)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                logger?.LogError(e, $"Uncaught exception in {nameof(FireAndForget)}");
            }
        }
    }
}