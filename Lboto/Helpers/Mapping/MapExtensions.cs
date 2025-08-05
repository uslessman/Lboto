using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Game.Objects;
using Lboto.Helpers.CachedObjects;
using Lboto.Helpers.Mapping;
using log4net;
using System.Collections.Generic;
using static DreamPoeBot.Loki.Game.LokiPoe.InstanceInfo;
using System.Linq;

namespace Lboto.Helpers
{

    public static class MapExtensions
    {
        private static readonly Dictionary<string, AffixData> _shoEtoDictionary;
        private static readonly Dictionary<string, MapData> _ignoredMapsDict;
        private static readonly GeneralSettings generalSettings_0;
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();
        public static bool IsMap(this Item item)
        {
            return item.Class == "Map";
        }
        public static bool IsInfluencedMap(this Item i)
        {
            return i.Stats.ContainsKey((StatTypeGGG)6827) || i.Stats.ContainsKey((StatTypeGGG)13845);
        }

        public static bool IsInfluencedMap(this CachedMapItem i)
        {
            return i.Stats.ContainsKey((StatTypeGGG)6827) || i.Stats.ContainsKey((StatTypeGGG)13845);
        }

        public static bool IsBlightedMap(this Item i)
        {
            return i.Stats.ContainsKey((StatTypeGGG)10342);
        }

        public static string GetBannedAffix(this Item map)
        {
            Rarity rarity = map.Rarity;
            if (!map.Stats.ContainsKey((StatTypeGGG)10342) || !map.IsCorrupted)
            {
                if ((int)rarity == 1 || (int)rarity == 2)
                {
                    bool _penMap = map.CleanName() == MapNames.Peninsula;
                    foreach (ModAffix explicitAffix in map.ExplicitAffixes)
                    {
                        string displayName = explicitAffix.DisplayName;
                        if (!_penMap || !(displayName == "Twinned"))
                        {
                            if (!_shoEtoDictionary.TryGetValue(displayName, out var value))
                            {
                                Log.Debug("[GetBannedAffix] Unknown map affix \"" + displayName + "\".");
                                continue;
                            }
                            if ((int)rarity != 1 || !value.RerollMagic)
                            {
                                if (!value.RerollRare)
                                {
                                    continue;
                                }
                                return displayName;
                            }
                            return displayName;
                        }
                        return displayName;
                    }
                    return null;
                }
                return null;
            }
            return null;
        }

        public static string CleanName(this CachedMapItem map)
        { 
            return ((int)map.Rarity == 3) ? map.FullName : map.Name.Replace(" Map", "");
        }

        public static string CleanName(this Item map)
        {
            return ((int)map.Rarity == 3) ? map.FullName : map.Name.Replace(" Map", "");
        }

        public static bool Ignored(this Item map)
        {
            MapData value;
            return !_ignoredMapsDict.TryGetValue(map.CleanName(), out value) || value.Ignored;
        }

        public static bool IsMavenInvitation(this Item item)
        {
            return item.Metadata.StartsWith("Metadata/Items/MapFragments/Maven/MavenMap") && item.Class != "MiscMapItem";
        }

        public static bool IsMavenInvitation(this CachedMapItem item)
        {
            return item.Metadata.StartsWith("Metadata/Items/MapFragments/Maven/MavenMap") && item.Class != "MiscMapItem";
        }

        public static bool Completed(this Item map)
        {
            return map.CleanName().Equals("Vaal Temple") || AtlasData.IsCompleted(map.CleanName());
        }

        public static bool Completed(this CachedMapItem map)
        {
            return map.CleanName().Equals("Vaal Temple") || AtlasData.IsCompleted(map.CleanName());
        }

        public class AtlasData
        {
            private static readonly HashSet<string> hashSet_0;

            public static bool IsCompleted(Item map)
            {
                if (map.Stats.ContainsKey((StatTypeGGG)10342))
                {
                    return true;
                }
                string item = map.CleanName();
                return hashSet_0.Contains(item);
            }

            public static bool IsCompleted(string fullName)
            {
                return hashSet_0.Contains(fullName.Replace(" Map", ""));
            }

            static AtlasData()
            {
                hashSet_0 = Atlas.BonusCompletedAreas.Select((DatWorldAreaWrapper a) => a.Name).ToHashSet();
            }
        }

        public static int Priority(this Item map)
        { 
            if (map.IsMavenInvitation() || map.Metadata.Contains("MapFragments/CurrencyAfflictionFragment"))
            {
                return 10001;
            }
            if (!_ignoredMapsDict.TryGetValue(map.CleanName(), out var value))
            {
                return int.MinValue;
            }
            int num = value.Priority;
            if (generalSettings_0.AtlasExplorationEnabled && !value.UnsupportedBossroom && !map.Ignored())
            {
                int mapTier = map.MapTier;
                num += 10000;
                num += mapTier * 3;
                if (!map.Completed())
                {
                    num += 400;
                    if (mapTier < 5 && (int)map.Rarity >= 1)
                    {
                        num += 50;
                    }
                    if (mapTier >= 5 && (int)map.Rarity == 2)
                    {
                        num += 100;
                    }
                    if (mapTier >= 11 && map.IsCorrupted)
                    {
                        num += 300;
                    }
                }
            }
            return num;
        }
        static MapExtensions()
        {
            generalSettings_0 = GeneralSettings.Instance;
            _ignoredMapsDict = MapSettings.Instance.MapDict;
            _shoEtoDictionary = AffixSettings.Instance.AffixDict;
        }
    }
}
