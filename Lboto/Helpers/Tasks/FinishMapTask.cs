using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.GameData;
using Lboto.Helpers.Mapping;
using log4net;
using System.Threading.Tasks;
using static DreamPoeBot.Loki.Game.LokiPoe;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;

namespace Lboto.Helpers.Tasks
{
    public class FinishMapTask : ITask, IAuthored, ILogicProvider, IMessageHandler, IStartStopEvents, ITickEvents
    {
        // Счетчик текущих импульсов/попыток завершения карты
        private static int currentPulseCount;

        public static readonly ILog Log = Logger.GetLoggerInstanceForType();

        private static readonly Interval pulseInterval;

        private static int MaxPulses
        {
            get
            {
                DatWorldAreaWrapper currentArea = World.CurrentArea;
                string name = currentArea.Name;
                if (!LocalData.MapMods.ContainsKey((StatTypeGGG)10342) && !currentArea.Id.Contains("Affliction"))
                {
                    switch (name)
                    {
                        default:
                            if (!(name == "Polaric Void"))
                            {
                                if (name == MapNames.JungleValley || name == MapNames.Mausoleum || name == MapNames.UndergroundSea || name == MapNames.Conservatory)
                                {
                                    return 20;
                                }
                                if (!(name == MapNames.ArachnidNest) && !(name == MapNames.Lookout))
                                {
                                    if (!(name == MapNames.Geode))
                                    {
                                        return 3;
                                    }
                                    return 1;
                                }
                                return 8;
                            }
                            goto case "Absence of Patience and Wisdom";
                        case "Absence of Patience and Wisdom":
                        case "Absence of Symmetry and Harmony":
                        case "Seething Chyme":
                            return 25;
                    }
                }
                return 1;
            }
        }
        public string Name => "FinishMapTask";

        public string Description => "Task for leaving current map.";

        public string Author => "ExVault";

        public string Version => "1.0";

        public async Task<bool> Run()
        {
            DatWorldAreaWrapper area = World.CurrentArea;
            if (area.IsMap)
            {
                if (area.Id.Contains("Affliction") && GeneralSettings.Instance.IsOnRun)
                {
                    return true;
                }
                              
                if (!DeliriumUi.IsDeliriumActive)
                {
                    int maxPulses = ((MapData.Current.FinalPulse > 0) ? MapData.Current.FinalPulse : MaxPulses);
                    if (currentPulseCount < maxPulses)
                    {
                        if (pulseInterval.Elapsed)
                        {
                            currentPulseCount++;
                            Log.Info($"[FinishMapTask] Final pulse {currentPulseCount}/{maxPulses}");
                            return true;
                        }
                        return true;
                    }
                    int mobsLeft = InstanceInfo.MonstersRemaining;
                    Log.Warn($"[FinishMapTask] Now leaving current map. Mobs left {mobsLeft}.");
                    await PlayerAction.TpToTown();
                    GeneralSettings.Instance.IsOnRun = false;                    
                    return true;
                }
                DeliriumUi.FinishDelirium();
                Log.Info("[FinishMapTask] Pressed end Delirium button, resetting pulse.");
                currentPulseCount = 0;
                await Wait.SleepSafe(5000);
                return true;
            }
            return false;
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == "MB_new_map_entered_event")
            {
                currentPulseCount = 0;
                return (MessageResult)0;
            }
            return (MessageResult)1;
        }

        public void Tick()
        {
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return (LogicResult)1;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        static FinishMapTask()
        {
            pulseInterval = new Interval(LokiPoe.Random.Next(750, 1250));
        }

    }
}
