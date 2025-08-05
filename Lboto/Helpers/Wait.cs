using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Coroutine;
using DreamPoeBot.Loki.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Lboto.Helpers
{
    public static class Wait
    {
        private static readonly log4net.ILog Log = Logger.GetLoggerInstanceForType();
        private static readonly Stopwatch stopwatch_0;
        public static async Task<bool> For(Func<bool> condition, string desc, int step = 100, int timeout = 3000)
        {
            return await For(condition, desc, () => step, timeout);
        }

        public static async Task TownMoveRandomDelay()
        {
            var settings = LbotoSettings.Instance;

            int pauseDurationMs = 0;
            int roll = LokiPoe.Random.Next(0, 100_000);

            // Рассчитываем базовый множитель, учитывая динамический фактор и время, прошедшее с последней большой паузы
            double multiplier = 0.9 + settings.TownPauseDynamicFactor + stopwatch_0.ElapsedMilliseconds / 10_000_000.0;
            int weightedRoll = (int)(roll * multiplier * settings.TownMovePauseFactor);

            // Костыль для специфики QuestBotEx
            if (((IAuthored)BotManager.Current).Name == "QuestBotEx")
            {
                weightedRoll = (int)(weightedRoll * 1.1);
            }

            // Определяем длительность паузы в зависимости от roll
            if (weightedRoll > 99_990)
            {
                stopwatch_0.Restart();
                settings.TownPauseDynamicFactor = 0.01;
                pauseDurationMs = LokiPoe.Random.Next(20_000, 120_000);
            }
            else if (weightedRoll > 99_900)
            {
                stopwatch_0.Restart();
                settings.TownPauseDynamicFactor = 0.01;
                pauseDurationMs = LokiPoe.Random.Next(10_000, 20_000);
            }
            else if (weightedRoll <= 95_000)
            {
                if (weightedRoll > 45_000 && weightedRoll <= 55_000)
                {
                    pauseDurationMs = LokiPoe.Random.Next(40, 100);
                }
                else if (weightedRoll > 55_000)
                {
                    pauseDurationMs = LokiPoe.Random.Next(100, 500);
                }
            }
            else
            {
                pauseDurationMs = LokiPoe.Random.Next(3_000, 10_000);
                settings.TownPauseDynamicFactor = Math.Max(0, settings.TownPauseDynamicFactor - 0.01);
            }

            // Если нужно, делаем паузу
            if (pauseDurationMs > 0)
            {
                Log.Warn($"[TownMoveRandomDelay(rolled: {weightedRoll})] Pause will last {pauseDurationMs} ms. [range: 0, 0]");

                if (!settings.PauseDataCollection.ContainsKey(PauseTypeEnum.TownMovePause))
                    settings.PauseDataCollection[PauseTypeEnum.TownMovePause] = new List<PauseData>();

                settings.PauseDataCollection[PauseTypeEnum.TownMovePause].Add(
                    new PauseData("TownMoveRandomDelay", weightedRoll, pauseDurationMs, new Range(0, 0)));

                await Coroutine.Sleep(pauseDurationMs);
            }
            else
            {
                settings.TownPauseDynamicFactor += 0.001;
            }
        }


        public static async Task<bool> For(Func<bool> condition, string desc, Func<int> step, int timeout = 3000)
        {
            try
            {
                Log.Debug($"[Wait] Start waiting for {desc} (timeout: {timeout}ms)");

                if (SafeCheck(condition, desc))
                    return true;

                var timer = Stopwatch.StartNew();
                while (timer.ElapsedMilliseconds < timeout)
                {
                    Log.Debug($"[Wait] Waiting for {desc} ({Math.Round(timer.ElapsedMilliseconds / 1000f, 2)}/{timeout / 1000f})");

                    if (SafeCheck(condition, desc))
                        return true;

                    await Coroutine.Sleep(step());
                }

                Log.Error($"[Wait] Timeout waiting for {desc} after {timeout} ms.");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"[Wait] Exception while waiting for {desc}: {ex}");
                return false;
            }
        }

        private static bool SafeCheck(Func<bool> condition, string desc)
        {
            try
            {
                Log.Debug($"[Wait] Checking condition for {desc}");
                return condition();
            }
            catch (Exception ex)
            {
                Log.Error($"[Wait] Condition threw exception during wait for {desc}: {ex}");
                return false;
            }
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

        public static bool StashPauseProbability(int probability)
        {
            //// Проверяем, находится ли бот в состоянии "мул" (переноска предметов, например)
            //bool isInMuleState = false;
            //Message response = Utility.BroadcastMessage(null, "Is_In_Mule_State", Array.Empty<object>());

            //if (response.TryGetOutput<bool>(0, ref isInMuleState) && isInMuleState)
            //{
            //    // Если бот в состоянии мула, не выполняем паузу
            //    return false;
            //}

            // Генерируем случайное число от 0 до 9999
            int roll = LokiPoe.Random.Next(0, 10000);

            // Проверяем, попадает ли число в диапазон, соответствующий указанной вероятности
            if (roll <= probability * 100)
            {
                Log.Warn($"[StashPauseProbability(rolled:{roll})] Trigger random move before interacting with stash");

                // Добавляем информацию о паузе в лог/настройки
                LbotoSettings.Instance.PauseDataCollection[PauseTypeEnum.StashPauseProbability]
                    .Add(new PauseData(
                        "StashPauseProbability",
                        roll,
                        0,
                        new Range(0, 0)));

                return true; // Указываем, что пауза будет выполнена
            }

            // Если число не попало в диапазон — паузы не будет
            return false;
        }

    }
}

