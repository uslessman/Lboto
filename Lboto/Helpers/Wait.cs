using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Coroutine;
using DreamPoeBot.Loki.Common;

namespace Lboto.Helpers
{
    public static class Wait
    {
        private static readonly log4net.ILog Log = Logger.GetLoggerInstanceForType();

        public static async Task<bool> For(Func<bool> condition, string desc, int step = 100, int timeout = 3000)
        {
            return await For(condition, desc, () => step, timeout);
        }

        public static async Task<bool> For(Func<bool> condition, string desc, Func<int> step, int timeout = 3000)
        {
            if (condition())
                return true;

            var timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < timeout)
            {
                Log.Debug($"[Wait] Waiting for {desc} ({Math.Round(timer.ElapsedMilliseconds / 1000f, 2)}/{timeout / 1000f})");

                if (condition())
                    return true;

                await Coroutine.Sleep(step());
            }

            Log.Error($"[Wait] Timeout waiting for {desc} after {timeout} ms.");
            return false;
        }

        public static async Task Sleep(int ms)
        {
            var timeout = Math.Max(LatencyTracker.Current, ms);
            await Coroutine.Sleep(timeout);
        }

        public static async Task SleepSafe(int ms)
        {
            var timeout = Math.Max(LatencyTracker.Current, ms);
            await Coroutine.Sleep(timeout);
        }

        public static async Task SleepSafe(int min, int max)
        {
            int latency = LatencyTracker.Current;
            if (latency > max)
            {
                await Coroutine.Sleep(latency);
            }
            else
            {
                await Coroutine.Sleep(LokiPoe.Random.Next(min, max + 1));
            }
        }

        public static async Task LatencySleep()
        {
            var timeout = Math.Max((int)(LatencyTracker.Current * 1.15), 25);
            Log.Debug($"[LatencySleep] Sleeping {timeout} ms.");
            await Coroutine.Sleep(timeout);
        }

        public static async Task ArtificialDelay()
        {
            var ms = LokiPoe.Random.Next(15, 40);
            Log.Debug($"[ArtificialDelay] Sleeping {ms} ms.");
            await Coroutine.Sleep(ms);
        }

        public static async Task<bool> ForAreaChange(uint previousAreaHash, int timeout = 60000)
        {
            if (await For(() => LokiPoe.StateManager.IsAreaLoadingStateActive, "loading screen", 100, 3000))
            {
                return await For(() => LokiPoe.IsInGame, "in game after area change", 200, timeout);
            }

            return false;
        }

        public static async Task<bool> ForHOChange(int timeout = 60000)
        {
            if (await For(() => LokiPoe.StateManager.IsAreaLoadingStateActive, "loading screen", 100, 3000))
            {
                return await For(() => LokiPoe.IsInGame, "in game after hideout change", 200, timeout);
            }

            return false;
        }
    }
}

