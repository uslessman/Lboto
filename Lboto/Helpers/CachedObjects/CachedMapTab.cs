using DreamPoeBot.Common;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Game.Objects;
using System.Collections.Generic;
using System.Linq;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;
using static DreamPoeBot.Loki.Game.Objects.ModAffix;

namespace Lboto.Helpers.CachedObjects
{
    public class CachedMapTab
    {
        public List<CachedMapItem> Maps { get; set; }
    }


    public class CachedMapItem
    {
        private readonly string _name;
        private readonly string _fullName;
        private readonly string _metadata;
        private readonly string _class;
        private readonly int _quality;
        private readonly Rarity _rarity;
        private readonly Vector2i _location;
        private readonly bool _identified;
        private readonly bool _isCorrupted;
        private readonly bool _isMirrored;
        private readonly bool _isFractured;
        private readonly Vector2i _locationTopLeft;
        private readonly List<KeyValuePair<string, string>> _affixList;
        private readonly Dictionary<StatTypeGGG, int> _stats;
        private readonly int _mapTier;
        private readonly string _stashTab;

        public string Name => _name;
        public string FullName => _fullName;
        public string Metadata => _metadata;
        public string Class => _class;
        public int Quality => _quality;
        public Rarity Rarity => _rarity;
        public string StashTab => _stashTab;
        public Vector2i Location => _location;
        public bool Identified => _identified;
        public bool IsCorrupted => _isCorrupted;
        public bool IsMirrored => _isMirrored;
        public bool IsFractured => _isFractured;
        public Vector2i LocationTopLeft => _locationTopLeft;
        public List<KeyValuePair<string, string>> AffixList => _affixList;
        public Dictionary<StatTypeGGG, int> Stats => _stats;
        public int MapTier => _mapTier;


        public CachedMapItem()
        {

        }
        public CachedMapItem(Item item)
        {
            _name = item.Name;
            _fullName = item.FullName;
            _metadata = item.Metadata;
            _class = item.Class;
            _quality = item.Quality;
            _rarity = item.Rarity;
            _stashTab = StashUi.TabControl.CurrentTabName;
            _locationTopLeft = item.LocationTopLeft;
            _identified = item.IsIdentified;
            _stats = item.Stats;
            _isCorrupted = item.IsCorrupted;
            _isMirrored = item.IsMirrored;
            _isFractured = item.IsFractured;
            _affixList = new List<KeyValuePair<string, string>>();

            foreach (ModAffix affix in item.Affixes)
            {
                string key = "";
                StatContainer val = Enumerable.FirstOrDefault(affix.Stats, (StatContainer a) => !string.IsNullOrWhiteSpace(a.Description) && !a.Description.ContainsIgnorecase("quantity") && !a.Description.ContainsIgnorecase("rarity") && !a.Description.ContainsIgnorecase("pack size"));
                string value = ((val != null) ? val.Description.Replace("{0:+d}", "#").Replace("{0}", "#").Replace("\\n", " ") : "");
                if (!string.IsNullOrWhiteSpace(affix.DisplayName))
                {
                    key = affix.DisplayName;
                }
                AffixList.Add(new KeyValuePair<string, string>(key, value));
            }

        }

    }


}
