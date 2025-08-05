using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game;
using log4net;
using System.Linq;
using System.Threading.Tasks;

namespace Lboto.Helpers.Tasks
{
    internal class TakeFragmentsTask : ITask, IAuthored, ILogicProvider, IMessageHandler, IStartStopEvents, ITickEvents
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();
        public static bool ShouldRefill = false;
        public string Name => "TakeFragmentTask";
        public string Author => "Lenson";
        public string Description => "Takes needed fragments to Inventory.";
        public string Version => "1.0";

        public static bool Executed = false;

        public async Task<bool> Run()
        {
            if (!ShouldRefill || !LokiPoe.IsInGame || Executed)
            {
                return false;
            }
            await Inventories.OpenStashTab(LbotoSettings.Instance.FragmentTabs.FirstOrDefault());
            Log.Debug("[TakeFragmentTask] Opened Fragment tab in stash.");
            foreach (var fragment in JustOpenDatMapsTask.ScarabsToPrioritizeVessel)
            {
                Log.Debug($"[TakeFragmentTask] Processing fragment: {fragment.Name} with type limit {fragment.TypeLimit}");
                if (fragment == null)
                {
                    Log.Error("[TakeFragmentTask] Null fragment in ScarabsToPrioritizeVessel.");
                    continue;
                }
                var control = fragment.Control?.Invoke();
                if (control == null)
                {
                    Log.Error($"[TakeFragmentTask] Control is null for fragment: {fragment.Name}");
                    continue;
                }
                int _counter = 0;
                while (_counter < fragment.TypeLimit)
                {
                    //var _I = LokiPoe.InGameState.StashUi.FragmentTab.Scarab.AnarchyScarab.CustomTabItem;
                    var item = control.CustomTabItem;
                    Log.Debug($"[TakeFragmentTask] we see {item.Name} x {item.StackCount}");
                    if (item == null)
                    {
                        Log.Error($"[TakeFragmentTask] No {fragment.Name} available in stash.");
                        BotManager.Stop(new StopReasonData("no_frags", $"No {fragment.Name} available in stash", null), false);
                        return true;
                    }
                    control.FastMove();
                    await Wait.SleepSafe(100);
                    _counter++;
                }
            }
            Executed = true;
            return true;
        }

        Task<LogicResult> ILogicProvider.Logic(Logic logic)
        {
            return Task.FromResult(LogicResult.Unprovided);
        }

        MessageResult IMessageHandler.Message(Message message)
        {
            return MessageResult.Unprocessed;
        }

        void IStartStopEvents.Start()
        {
            
        }

        void IStartStopEvents.Stop()
        {
            
        }

        void ITickEvents.Tick()
        {
            
        }
    }
}
