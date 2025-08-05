using DreamPoeBot.Common;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Game.Objects;
using Lboto.Helpers.Positions;

namespace Lboto.Helpers
{
    public class CachedWorldItem : CachedObject
    {
        public Vector2i Size { get; }
        public Rarity Rarity { get; }

        public CachedWorldItem(int id, WalkablePosition position, Vector2i size, Rarity rarity)
            : base(id, position)
        {
            Size = size;
            Rarity = rarity;
        }

        public CachedWorldItem(WorldItem worldItem)
        : base(worldItem.Item.Id, new WalkablePosition(worldItem.Item.Name, worldItem.Item.WorldItemPosition))
        {
            Size = worldItem.Item.Size;
            Rarity = worldItem.Item.Rarity;
        }

        public Vector2 LabelPos => Object == null ? Vector2.Zero : Object.WorldItemLabel.Coordinate;

        public new WorldItem Object
        {
            get
            {
                NetworkObject obj = GetObject();
                return obj as WorldItem;
            }
        }
    }
}
