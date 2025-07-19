using System.Threading.Tasks;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;

namespace Lboto.Helpers.Tasks
{
    public class FallbackTask : ITask
    {
        private static readonly log4net.ILog Log = Logger.GetLoggerInstanceForType();

        public async Task<bool> Run()
        {
            //if (!LokiPoe.IsInGame || LokiPoe.Me.IsDead)
            //    return false;

            Log.Warn("[FallbackTask] Fallback task is running — no other task acted. Resetting task chain.");
            await Wait.Sleep(200);
            return true;
        }

        public MessageResult Message(Message message) => MessageResult.Unprocessed;

        public async Task<LogicResult> Logic(Logic logic) => LogicResult.Unprovided;

        public void Start() { }

        public void Tick() { }

        public void Stop() { }

        public string Name => "FallbackTask";
        public string Description => "Fallback task that executes when no other task acted. Forces task manager restart.";
        public string Author => "";
        public string Version => "1.0";
    }
}
