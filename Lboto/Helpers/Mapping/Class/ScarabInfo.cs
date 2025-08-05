using DreamPoeBot.Loki.Game;
using System;

namespace Lboto.Helpers.Mapping
{
    public class ScarabInfo
    {
        public string Name { get; private set; }
        public string Type { get; private set; }
        public int TypeLimit { get; private set; }
        public Func<InventoryControlWrapper> Control { get; private set; }

        public ScarabInfo(string name, string type, int typeLimit, Func<InventoryControlWrapper> control)
        {
            Name = name;
            Type = type;
            TypeLimit = typeLimit;
            Control = control;
        }
    }
}
