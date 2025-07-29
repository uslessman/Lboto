using DreamPoeBot.Loki.Game.GameData;
using Lboto.Helpers.Positions;
using DreamPoeBot.Loki.Game.Objects;

namespace Lboto.Helpers.CachedObjects
{
    public class CachedStrongbox : CachedObject
    {
        public Rarity Rarity { get; }

        public CachedStrongbox(int id, WalkablePosition position, Rarity rarity)
            : base(id, position)
        {
            Rarity = rarity;
        }

        public CachedStrongbox(Chest chest) 
            : base(chest.Id, new WalkablePosition(chest.Name, chest.Position))
        {
            Rarity = chest.Rarity;
        }

        public new Chest Object => GetObject() as Chest;
    }
}
