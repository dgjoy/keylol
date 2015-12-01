using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Keylol.SteamBot
{
    public static class Utils
    {
        public static async Task<T> Retry<T>(Func<Task<T>> action, Action<int> beforeEachRetry = null,
            uint maxCount = SteamBotService.GlobalMaxRetryCount,
            int cooldownDuration = SteamBotService.GlobalRetryCooldownDuration)
        {
            for (var i = 1; i <= maxCount; i++)
            {
                beforeEachRetry?.Invoke(i);
                try
                {
                    return await action();
                }
                catch (Exception)
                {
                    if (cooldownDuration > 0)
                        Thread.Sleep(cooldownDuration);
                }
            }
            return default(T);
        }

        public static async Task<bool> Retry(Func<Task> action, Action<int> beforeEachRetry = null,
            uint maxCount = SteamBotService.GlobalMaxRetryCount,
            int cooldownDuration = SteamBotService.GlobalRetryCooldownDuration)
        {
            return await Retry(async () =>
            {
                await action();
                return true;
            }, beforeEachRetry, maxCount, cooldownDuration);
        }
    }
}