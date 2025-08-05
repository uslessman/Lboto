using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game.GameData;
using Lboto.Helpers.Global;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lboto.Helpers.Tasks
{
    public class ExploreTask : ITask, IAuthored, ILogicProvider, IMessageHandler, IStartStopEvents, ITickEvents
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();
        public string Name => "ExploreTask";
        public string Description => "Running around. Complete handling.";
        public string Author => "L";
        public string Version => "1.0";

        public static bool MapCompleted = false;
        #region future settings
        private static int _mapTgtPercent = 80;
        #endregion
        public async Task<bool> Run()
        {
            Log.Debug($"[{Name}] Run");
            DatWorldAreaWrapper area = World.CurrentArea;
            if (!area.IsMap || MapCompleted)
            {
                return false;
            }
            var pos = CombatAreaCache.Current.Explorer.BasicExplorer.Location;
            Log.Debug($"[{Name}] AreaIsMap: {area.IsMap}");
            Move.Towards(pos, $"Now exploring{pos}");
            if (CombatAreaCache.Current.Explorer.BasicExplorer.PercentComplete >= 85) { Log.Warn($"[MapExplorationTask] Exploration limit has been reached ({_mapTgtPercent} Percent). Map is complete"); MapCompleted = true; return true; }
            return true;//await CombatAreaCache.Current.Explorer.Execute();
        }

        public void Tick()
        { 
        
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public MessageResult Message(Message message)
        {
            string id = message.Id;
            if (id == "MB_new_map_entered_event")
            {
                CombatAreaCache.Current.Explorer.BasicExplorer.Reset();
                Log.Info("[" + Name + "] Reset.");
                Reset(message.GetInput<string>(0));
                return MessageResult.Processed;
            }
            if (id == "explorer_local_transition_entered_event")
            {
                //SpecificTweaksOnLocalTransition();
                return MessageResult.Processed;
            }
            return MessageResult.Unprocessed;
        }

        private static void Reset(string areaName)
        {
            MapCompleted = false;
        }
    }
}
