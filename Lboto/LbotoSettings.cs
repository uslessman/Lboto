using DreamPoeBot.Loki;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Game.Objects;

namespace Lboto
{
    public class LbotoSettings : JsonSettings
    {
        private static LbotoSettings _instance;
        public static LbotoSettings Instance => _instance ?? (_instance = new LbotoSettings());

        private LbotoSettings()
            : base(GetSettingsFilePath(Configuration.Instance.Name, "Lboto.json")) { }
    }
}
