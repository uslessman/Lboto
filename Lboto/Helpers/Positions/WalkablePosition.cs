using DreamPoeBot.Common;
using DreamPoeBot.Loki.Common;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Lboto.Helpers.Positions
{
    public class WalkablePosition : WorldPosition
    {
        private static readonly log4net.ILog Log = Logger.GetLoggerInstanceForType();

        protected int Radius;
        protected int Step;

        private readonly string _name;

        public string Name => _name;

        public virtual bool Initialized { get; set; }

        public WalkablePosition(string name, Vector2i vector, int step = 10, int radius = 30) : base(vector)
        {
            _name = SanitizeName(name);
            Step = step;
            Radius = radius;
        }

        public WalkablePosition(string name, int x, int y, int step = 10, int radius = 30) : base(x, y)
        {
            _name = SanitizeName(name);
            Step = step;
            Radius = radius;
        }

        private string SanitizeName(string name)
        {
            if (name.Contains("<rgb("))
            {
                var clean = name.Substring(name.IndexOf(">", StringComparison.Ordinal) + 1)
                               .Replace("{", "").Replace("}", "");
                return clean;
            }
            return name;
        }

        public void Come()
        {
            if (!Initialized)
                HardInitialize();

            Move.TowardsWalkable(Vector, Name);
        }

        public async Task ComeAtOnce(int distance = 20)
        {
            if (!Initialized)
                HardInitialize();

            await Move.AtOnce(Vector, Name, distance);
        }

        public bool TryCome()
        {
            if (!Initialized && !Initialize())
                return false;

            return Move.Towards(Vector, Name);
        }

        public async Task<bool> TryComeAtOnce(int distance = 20)
        {
            if (!Initialized && !Initialize())
                return false;

            await Move.AtOnce(Vector, Name, distance);
            return true;
        }

        public virtual bool Initialize()
        {
            if (FindWalkable())
            {
                Initialized = true;
                return true;
            }

            Log.Debug($"[WalkablePosition] Failed to find walkable position for {this}");
            return false;
        }

        protected virtual void HardInitialize()
        {
            if (FindWalkable())
            {
                Initialized = true;
                return;
            }

            Log.Error($"[WalkablePosition] Failed to find walkable position for {this}, requesting new instance.");
            var currentArea = DreamPoeBot.Loki.Game.LokiPoe.CurrentWorldArea;
            //Travel.RequestNewInstance(currentArea); А как? 
        }

        protected bool FindWalkable()
        {
            if (PathExists)
                return true;

            Log.Debug($"[WalkablePosition] {this} is unwalkable, trying to find nearby position.");

            var walkable = WorldPosition.FindPositionForMove(this, Step, Radius);

            if (walkable == null)
                return false;

            Log.Debug($"[WalkablePosition] Found walkable position at {walkable.AsVector} ({Vector.Distance(walkable)} units from original).");
            Vector = walkable;
            return true;
        }

        public override string ToString()
        {
            return $"[{Name}] at {Vector} (distance: {Distance})";
        }
    }
}
