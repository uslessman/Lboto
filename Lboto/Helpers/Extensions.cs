using DreamPoeBot.Loki.Game.Objects;
using Lboto.Helpers.Positions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lboto.Helpers
{
    public static class Extensions
    {
        public static WalkablePosition WalkablePosition(this NetworkObject obj, int step = 5, int radius = 20)
        {
            return new WalkablePosition(obj.Name, obj.Position, step, radius);
        }
    }
}
