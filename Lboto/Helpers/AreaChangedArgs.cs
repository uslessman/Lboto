using System;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Bot;

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

        public AreaChangedArgs(Message message)
        {
            OldHash = message.GetInput<uint>(0);
            NewHash = message.GetInput<uint>(1);
            OldArea = message.GetInput<DatWorldAreaWrapper>(2);
            NewArea = message.GetInput<DatWorldAreaWrapper>(3);
        }
    }
}
