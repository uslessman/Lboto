using System;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.GameData;
using Lboto.Helpers.CachedObjects;
using Lboto.Helpers.Global;

namespace Lboto.Helpers
{
    public static class Events
    {
        public static event Action IngameBotStart;
        public static event EventHandler<AreaChangedArgs> AreaChanged;
        public static event EventHandler<AreaChangedArgs> CombatAreaChanged;
        public static event Action<int> PlayerDied;
        public static event Action<int> PlayerLeveled;
        public static event Action<CachedItem> ItemLooted;
        public static event Action<CachedItem> ItemStashed;
        public static event EventHandler<ItemsSoldArgs> ItemsSold;

        private static bool _pendingStart = true;
        private static bool _wasDead;
        private static string _cachedName;
        private static int _cachedLevel;
        private static uint _lastAreaHash;
        private static DatWorldAreaWrapper _lastArea;
        private static uint _lastCombatHash;
        private static DatWorldAreaWrapper _lastCombatArea;

        public static void Tick()
        {
            if (!LokiPoe.IsInGame) return;

            if (_pendingStart)
            {
                _pendingStart = false;
                IngameBotStart?.Invoke();
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
            ItemStashed?.Invoke(item);
        }

        public static void RaiseItemsSold(ItemsSoldArgs args)
        {
            ItemsSold?.Invoke(null, args);
        }

        public static void Reset()
        {
            _pendingStart = true;
            _lastAreaHash = 0;
            _lastArea = null;
            _lastCombatHash = 0;
            _lastCombatArea = null;
            _wasDead = false;
        }
    }
}
