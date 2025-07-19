using System;
using System.Collections.Generic;
using System.Linq;
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot.Pathfinding;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Common;

namespace Lboto.Helpers.Positions
{
    public class WorldPosition : Position
    {
        private static readonly log4net.ILog Log = Logger.GetLoggerInstanceForType();

        public WorldPosition(Vector2i vector) : base(vector) { }

        public WorldPosition(int x, int y) : base(x, y) { }

        public int Distance => LokiPoe.MyPosition.Distance(Vector);
        public int DistanceSqr => LokiPoe.MyPosition.DistanceSqr(Vector);
        public float PathDistance => ExilePather.PathDistance(LokiPoe.MyPosition, Vector);

        public bool IsNear => Distance <= 20;
        public bool IsFar => Distance > 20;

        public bool IsNearByPath => PathDistance <= 23;
        public bool IsFarByPath => PathDistance > 23;

        public bool PathExists => ExilePather.PathExistsBetween(LokiPoe.MyPosition, Vector);

        public WorldPosition GetWalkable(int step = 5, int radius = 30)
        {
            return PathExists ? this : FindPositionForMove(this, step, radius);
        }

        public override string ToString()
        {
            return $"{Vector} (distance: {Distance})";
        }

        public static WorldPosition FindPositionForMove(Vector2i pos, int step = 5, int radius = 30)
        {
            var walkable = FindWalkablePosition(pos, radius);
            if (walkable != null) return walkable;

            return FindPathablePosition(pos, step, radius);
        }

        public static WorldPosition FindWalkablePosition(Vector2i pos, int radius = 20)
        {
            var walkable = ExilePather.FastWalkablePositionFor(pos, radius);
            if (ExilePather.PathExistsBetween(LokiPoe.MyPosition, walkable))
            {
                Log.Debug($"[WorldPosition] Found walkable position at {walkable}");
                return new WorldPosition(walkable);
            }
            return null;
        }

        public static WorldPosition FindPathablePosition(Vector2i pos, int step = 5, int radius = 30)
        {
            var myPos = LokiPoe.MyPosition;
            int x = pos.X;
            int y = pos.Y;

            for (int r = step; r <= radius; r += step)
            {
                int minX = x - r;
                int minY = y - r;
                int maxX = x + r;
                int maxY = y + r;

                for (int i = minX; i <= maxX; i += step)
                {
                    for (int j = minY; j <= maxY; j += step)
                    {
                        if (i != minX && i != maxX && j != minY && j != maxY)
                            continue;

                        var p = new Vector2i(i, j);
                        if (ExilePather.PathExistsBetween(myPos, p))
                        {
                            Log.Debug($"[WorldPosition] Found pathable position at {p}");
                            return new WorldPosition(p);
                        }
                    }
                }
            }

            return null;
        }

        public static WorldPosition FindPathablePositionAtDistance(int min, int max, int step)
        {
            var x = LokiPoe.MyPosition.X;
            var y = LokiPoe.MyPosition.Y;
            var pathMax = max + 5;

            for (int i = min; i <= max; i += step)
            {
                var directions = new[]
                {
                    new Vector2i(x, y + i),     // Top
                    new Vector2i(x + i, y + i), // Top right
                    new Vector2i(x - i, y + i), // Top left
                    new Vector2i(x + i, y),     // Right
                    new Vector2i(x - i, y),     // Left
                    new Vector2i(x + i, y - i), // Bottom right
                    new Vector2i(x - i, y - i), // Bottom left
                    new Vector2i(x, y - i)      // Bottom
                };

                foreach (var pos in directions)
                {
                    var wp = new WorldPosition(pos);
                    if (wp.PathDistance <= pathMax)
                    {
                        Log.Debug($"[WorldPosition] Found pathable position at distance {i}: {pos}");
                        return wp;
                    }
                }
            }

            return null;
        }

        public static WorldPosition FindRandomPositionForMove(Vector2i pos, int step = 10, int radius = 60, int angle = 359)
        {
            var myPosition = LokiPoe.Me.Position;
            var rnd = LokiPoe.Random.Next(0, angle);
            radius = LokiPoe.Random.Next(radius / 4, radius);

            for (int rad = radius; rad > 0; rad -= step)
            {
                for (int angleModificator = rnd; angleModificator < rnd + angle; angleModificator += step)
                {
                    var realAngle = angleModificator <= angle ? angleModificator : angleModificator - angle;
                    var p = GetPointOnCircle(pos, ConvertToRadians(realAngle), rad);

                    if (ExilePather.IsWalkable(p) && ExilePather.PathExistsBetween(myPosition, p))
                    {
                        Log.Debug($"[WorldPosition] Found random walkable position at {p}");
                        return new WalkablePosition("Random Walkable Position", p);
                    }
                }
            }

            return null;
        }

        protected static Vector2 GetAveragePoint(List<Vector2> pts)
        {
            return new Vector2()
            {
                X = pts.Average(p => p.X),
                Y = pts.Average(p => p.Y)
            };
        }

        protected static double GetAngleBetweenPoints(Vector2 pt1, Vector2 pt2)
        {
            return Math.Atan2(pt2.Y - pt1.Y, pt2.X - pt1.X);
        }

        protected static Vector2i GetPointOnCircle(Vector2i center, double radian, double radius)
        {
            return new Vector2i()
            {
                X = center.X + (int)(radius * Math.Cos(radian)),
                Y = center.Y + (int)(radius * Math.Sin(radian))
            };
        }

        public static double ConvertToRadians(double angle)
        {
            return 0.174532922 * angle; // Math.PI / 180 * angle;
        }

        public class ComparerByDistanceSqr : IComparer<WorldPosition>
        {
            public static readonly ComparerByDistanceSqr Instance = new ComparerByDistanceSqr();

            private ComparerByDistanceSqr() { }

            public int Compare(WorldPosition first, WorldPosition second)
            {
                return first.DistanceSqr - second.DistanceSqr;
            }
        }

        public class ComparerByPathDistance : IComparer<WorldPosition>
        {
            public static readonly ComparerByPathDistance Instance = new ComparerByPathDistance();

            private ComparerByPathDistance() { }

            public int Compare(WorldPosition first, WorldPosition second)
            {
                return first.PathDistance.CompareTo(second.PathDistance);
            }
        }
    }
}
