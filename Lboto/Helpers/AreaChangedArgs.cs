using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DreamPoeBot.Loki.Game.GameData;

namespace Lboto.Helpers
{
    public class AreaChangedArgs : EventArgs
    {
        public uint OldHash { get; }
        public uint NewHash { get; }
        public DatWorldAreaWrapper OldArea { get; }
        public DatWorldAreaWrapper NewArea { get; }

        public AreaChangedArgs(uint oldHash, uint newHash, DatWorldAreaWrapper oldArea, DatWorldAreaWrapper newArea)
        {
            OldHash = oldHash;
            NewHash = newHash;
            OldArea = oldArea;
            NewArea = newArea;
        }
    }
}
