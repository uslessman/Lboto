using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Game.Objects;
using Lboto.Helpers.CachedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lboto.Helpers
{
    public static class MapExtentions
    {
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
    }
}
