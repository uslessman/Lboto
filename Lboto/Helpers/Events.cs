using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Common;
using Lboto.Helpers.CachedObjects;
using Lboto.Helpers.Global;
using log4net;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Lboto.Helpers
{
    public static class Events
    {
        public static event EventHandler<AreaChangedArgs> AreaChanged;
        public static event EventHandler<AreaChangedArgs> CombatAreaChanged;
        public static event Action<int> PlayerDied;
        public static event Action<int> PlayerLeveled;
        public static event Action<CachedItem> ItemLooted;
        public static event Action<CachedItem> ItemStashedEvent;
        public static event Action IngameBotStart;
        public static event Action PlayerResurrected;
        public static event Action<CachedItem> ItemLootedEvent;
        public static event EventHandler<ItemsSoldArgs> ItemsSoldEvent;

        private static bool _wasDead;
        private static string _cachedName;
        private static int _cachedLevel;
        private static uint _lastAreaHash;
        private static DatWorldAreaWrapper _lastArea;
        private static uint _lastCombatHash;
        private static DatWorldAreaWrapper _lastCombatArea;

        public static readonly ILog Log = Logger.GetLoggerInstanceForType();

        private static bool _checkStart;
        public static void Start()
        {
            _checkStart = true;
        }

        public static void Tick()
        {
            if (!LokiPoe.IsInGame) return;

            if (!LokiPoe.IsInGame)
                return;

            if (_checkStart)
            {
                _checkStart = false;
                Log.Info("[Events] Ingame bot start.");
                Utility.BroadcastMessage(null, Messages.IngameBotStart);
            }

            var currentHash = LokiPoe.LocalData.AreaHash;
            if (currentHash != _lastAreaHash)
            {
                var oldArea = _lastArea;
                var newArea = World.CurrentArea;
                var oldHash = _lastAreaHash;

                _lastArea = newArea;
                _lastAreaHash = currentHash;

                AreaChanged?.Invoke(null, new AreaChangedArgs(oldHash, currentHash, oldArea, newArea));

                if (newArea.IsCombatArea && currentHash != _lastCombatHash)
                {
                    var oldCombat = _lastCombatArea;
                    _lastCombatArea = newArea;
                    _lastCombatHash = currentHash;

                    CombatAreaChanged?.Invoke(null, new AreaChangedArgs(_lastCombatHash, currentHash, oldCombat, newArea));
                }
            }

            var me = LokiPoe.Me;
            if (me.IsDead)
            {
                if (!_wasDead)
                {
                    _wasDead = true;
                    int deathCount = ++CombatAreaCache.Current.DeathCount;
                    PlayerDied?.Invoke(deathCount);
                }
            }
            else
            {
                _wasDead = false;
            }

            var name = me.Name;
            var level = me.Level;

            if (name != _cachedName)
            {
                _cachedName = name;
                _cachedLevel = level;
            }
            else if (level > _cachedLevel)
            {
                _cachedLevel = level;
                PlayerLeveled?.Invoke(level);
            }
        }

        public static void RaiseItemLooted(CachedItem item)
        {
            ItemLooted?.Invoke(item);
        }

        public static void RaiseItemStashed(CachedItem item)
        {
            ItemStashedEvent?.Invoke(item);
        }

        public static void RaiseItemsSold(ItemsSoldArgs args)
        {
            ItemsSoldEvent?.Invoke(null, args);
        }

        public static void FireEventsFromMessage(Message message)
        {
            switch (message.Id)
            {
                case Messages.IngameBotStart:
                    IngameBotStart?.Invoke();
                    return;

                case Messages.AreaChanged:
                    AreaChanged?.Invoke(null, new AreaChangedArgs(message));
                    return;

                case Messages.CombatAreaChanged:
                    CombatAreaChanged?.Invoke(null, new AreaChangedArgs(message));
                    return;

                case Messages.PlayerDied:
                    PlayerDied?.Invoke(message.GetInput<int>());
                    return;

                case Messages.PlayerResurrected:
                    PlayerResurrected?.Invoke();
                    return;

                case Messages.PlayerLeveled:
                    PlayerLeveled?.Invoke(message.GetInput<int>());
                    return;

                case Messages.ItemLootedEvent:
                    ItemLootedEvent?.Invoke(message.GetInput<CachedItem>());
                    return;

                case Messages.ItemStashedEvent:
                    ItemStashedEvent?.Invoke(message.GetInput<CachedItem>());
                    return;

                case Messages.ItemsSoldEvent:
                    ItemsSoldEvent?.Invoke(null, new ItemsSoldArgs(message));
                    return;
            }
        }

        public static void Reset()
        {           
            _lastAreaHash = 0;
            _lastArea = null;
            _lastCombatHash = 0;
            _lastCombatArea = null;
            _wasDead = false;
        }
        
        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        public static class Messages
        {
            public const string IngameBotStart = "ingame_bot_start_event";
            public const string AreaChanged = "area_changed_event";
            public const string CombatAreaChanged = "combat_area_changed_event";
            public const string PlayerDied = "player_died_event";
            public const string PlayerResurrected = "player_resurrected_event";
            public const string PlayerLeveled = "player_leveled_event";
            public const string ItemLootedEvent = "item_looted_event";
            public const string ItemStashedEvent = "item_stashed_event";
            public const string ItemsSoldEvent = "items_sold_event";
        }
    }
    
}
