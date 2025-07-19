using System.Diagnostics;

namespace Lboto.Helpers.Utils
{
    public class Interval
    {
        private readonly int _milliseconds;
        private readonly Stopwatch _stopwatch;

        public Interval(int milliseconds)
        {
            _milliseconds = milliseconds;
            _stopwatch = Stopwatch.StartNew();
        }

        public bool Elapsed
        {
            get
            {
                if (_stopwatch.ElapsedMilliseconds >= _milliseconds)
                {
                    _stopwatch.Restart();
                    return true;
                }
                return false;
            }
        }

        public void Reset()
        {
            _stopwatch.Restart();
        }
    }
}
