using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.Objects;
using Lboto.Helpers;
using log4net;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Lboto.Helpers.Tasks;

namespace Lboto.Tasks.Common
{
    public class AfkTask : ITask
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        private readonly Stopwatch _breakTimer = new Stopwatch();
        private readonly Stopwatch _runTimer = Stopwatch.StartNew();
        private readonly Random _rng = new Random();

        private bool _initialized;
        private int _nextBreakInMinutes;
        private int _breakDurationMinutes;

        // Настройки можно вынести отдельно, сейчас зададим прямо в коде
        private const bool EnableBreaks = true;
        private const int DefaultBreakEveryMinutes = 45;
        private const int MinBreakMinutes = 5;
        private const int MaxBreakMinutes = 12;

        public string Name => "AfkTask";
        public string Description => "A task that randomly AFKs the bot at safe locations like hideout.";
        public string Author => "Lajt";
        public string Version => "1.0";

        public void Start()
        {
            if (!EnableBreaks || _initialized)
                return;

            _nextBreakInMinutes = _rng.Next(DefaultBreakEveryMinutes - 10, DefaultBreakEveryMinutes + 15);
            Log.Warn($"[AfkTask] Next break in {_nextBreakInMinutes} minutes.");
            _initialized = true;
        }

        public void Stop() { }

        public void Tick() { }

        public async Task<bool> Run()
        {
            if (!EnableBreaks)
                return false;

            if (!LokiPoe.IsInGame || LokiPoe.Me.IsDead)
                return false;

            if (!LokiPoe.Me.IsInTown && !LokiPoe.Me.IsInHideout)
                return false;

            // Уже на паузе?
            if (_breakTimer.IsRunning)
            {
                if (_breakTimer.Elapsed.TotalMinutes < _breakDurationMinutes)
                {
                    Log.Debug($"[AfkTask] Still AFKing: {_breakTimer.Elapsed:mm\\:ss} / {_breakDurationMinutes}m");
                    if (LokiPoe.Me.IsInTown && ((Player)LokiPoe.Me).Hideout != null)
                    {
                        await TpToHideoutTask.GoToHideout();
                    }
                    await Wait.SleepSafe(_rng.Next(10000, 25000));
                    return true;
                }

                Log.Warn("[AfkTask] Break finished.");
                _breakTimer.Reset();
                _runTimer.Restart();
                _nextBreakInMinutes = _rng.Next(DefaultBreakEveryMinutes - 10, DefaultBreakEveryMinutes + 15);
                Log.Warn($"[AfkTask] Next break in {_nextBreakInMinutes} minutes.");
                return false;
            }

            // Пора уйти на перерыв?
            if (_runTimer.Elapsed.TotalMinutes >= _nextBreakInMinutes)
            {
                _breakDurationMinutes = _rng.Next(MinBreakMinutes, MaxBreakMinutes);
                Log.Warn($"[AfkTask] Taking a break for {_breakDurationMinutes} minutes...");
                _breakTimer.Restart();
                return true;
            }

            return false;
        }

        public MessageResult Message(Message message) => MessageResult.Unprocessed;
        public Task<LogicResult> Logic(Logic logic) => Task.FromResult(LogicResult.Unprovided);
    }
}
