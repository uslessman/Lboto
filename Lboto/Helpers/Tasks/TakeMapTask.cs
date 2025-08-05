using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Game.Objects;
using Lboto.Helpers;
using log4net;
using System.Linq;
using System.Threading.Tasks;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;

namespace Lboto.Helpers.Tasks
{
    public class TakeMapTask : ITask, IAuthored, ILogicProvider, IMessageHandler, IStartStopEvents, ITickEvents
    {

        public string Name => "TakeMapTask";
        public string Author => "";
        public string Description => "Takes a map, identifies it, scours if needed.";
        public string Version => "1.0";

        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        public static bool Executed = false;

        public async Task<bool> Run()
        {
            if (Executed)
            {
                return false;
            }
            //Log.Debug("[TakeMapTask] Im here");
            if (SkillsUi.IsOpened || AtlasSkillsUi.IsOpened)
                return false;

            if (!World.CurrentArea.IsTown && !World.CurrentArea.IsMyHideoutArea)
                return false;

            // Ищем карту в инвентаре
            Item map = Inventories.InventoryItems.FirstOrDefault(i => i.IsMap());

            // Если карты нет — пробуем взять из тайника
            if (map == null)
            {
                var cachedMap = await Inventories.CacheMapTabs();
                if (cachedMap == null || !cachedMap.Maps.Any())
                {
                    Log.Error("[TakeMapTask] No maps found in map stash tabs.");
                    BotManager.Stop(new StopReasonData("no_maps", "No maps available in stash", null), false);
                    return true;
                }

                var anyMap = cachedMap.Maps.FirstOrDefault(x => !x.Name.ContainsIgnorecase("blighted")); //we don't want blighted maps
                if (anyMap == null)
                {
                    Log.Error("[TakeMapTask] Failed to find map.");
                    BotManager.Stop(new StopReasonData("no_maps", "No maps available in stash", null), false);
                    return true;
                }

                map = await Inventories.TakeMapFromStash(anyMap);
                if (map == null)
                    return true;
            }

            var mapPos = map.LocationTopLeft;

            // 1. Если не идентифицирована — применить Scroll of Wisdom
            if (!map.IsIdentified)// && !map.IsCorrupted)
            {
                if (HasCurrency(CurrencyNames.Wisdom))
                {
                    Log.Debug($"[TakeMapTask] Identifying map at {mapPos}");
                    await Inventories.ApplyOrb(mapPos, CurrencyNames.Wisdom);
                    map = InventoryUi.InventoryControl_Main.Inventory.FindItemByPos(mapPos);
                }
                else
                {
                    Log.Error("[TakeMapTask] No Scrolls of Wisdom.");
                    Executed = true;
                    return true;
                }
            }

            // 2. Если не Normal — применить Scouring
            if (map.Rarity != Rarity.Normal && !map.IsCorrupted)
            {
                if (HasCurrency(CurrencyNames.Scouring))
                {
                    Log.Debug($"[TakeMapTask] Scouring map at {mapPos}");
                    await Inventories.ApplyOrb(mapPos, CurrencyNames.Scouring);
                    map = InventoryUi.InventoryControl_Main.Inventory.FindItemByPos(mapPos);
                }
                else
                {
                    Log.Error("[TakeMapTask] No Orbs of Scouring.");
                    Executed = true;
                    return false;
                }
            }

            Log.Warn($"[TakeMapTask] Map is ready: {map.Name} (Rarity: {map.Rarity})");
            OpenMapTask.Enabled = true;
            //MaaanJustOpenDatMaps.Enabled = true;
            Executed = true;
            return false;
        }

        public MessageResult Message(Message message) => MessageResult.Unprocessed;

        public void Start() { }
        public void Stop() { }
        public void Tick() { }

        public Task<LogicResult> Logic(Logic logic) => Task.FromResult(LogicResult.Unprovided);

        private static bool HasCurrency(string name)
        {
            return Inventories.InventoryItems.Any(i => i.Name == name || i.FullName == name);
        }

        internal static string ScarabType(Item item)
        {
            string fullName = item.FullName;

            // Handle "Horned Scarab of X" case - return X
            if (fullName.StartsWith("Horned Scarab of "))
            {
                return fullName.Substring("Horned Scarab of ".Length);
            }

            // Handle "Scarab of X" case - return X
            if (fullName.StartsWith("Scarab of "))
            {
                return fullName.Substring("Scarab of ".Length);
            }

            // Handle standard cases - return first word before "Scarab"
            int scarabIndex = fullName.IndexOf("Scarab");
            if (scarabIndex > 0)
            {
                return fullName.Substring(0, scarabIndex).Trim();
            }

            // Fallback for unexpected cases
            return fullName.Replace("Scarab", "").Trim();
        }
    }

}
