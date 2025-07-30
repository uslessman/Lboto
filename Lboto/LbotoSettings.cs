using DreamPoeBot.Loki;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Common.MVVM;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Game.Objects;
using Lboto.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Lboto
{
    public class LbotoSettings : JsonSettings
    {
        private static LbotoSettings _instance;
        public static LbotoSettings Instance => _instance ?? (_instance = new LbotoSettings());

        /// <summary>
        /// Использовать переключение табов клавиатурой (вместо мыши)
        /// </summary>
        public bool ForceKeyboardSwitch { get; set; } = false;

        /// <summary>
        /// Список табов с обычными картами
        /// </summary>
        public List<string> MapTabs { get; set; } = new List<string> { "Maps" };

        /// <summary>
        /// Список табов с симулакрумами
        /// </summary>
        public List<string> SimulacrumTabs { get; set; } = new List<string> { "Simulacrums" };

        /// <summary>
        /// Список табов с заражёнными картами (Blighted Maps)
        /// </summary>
        public List<string> BlightedMapTabs { get; set; } = new List<string> { "Blighted Maps" };

        /// <summary>
        /// Список табов с картами под влиянием
        /// </summary>
        public List<string> InfluencedMapTabs { get; set; } = new List<string> { "Influenced Maps" };

        /// <summary>
        /// Таблист, которые следует игнорировать
        /// </summary>
        public List<string> IgnoredTabs { get; set; } = new List<string> { "Remove-only" };

        /// <summary>
        /// Список табов с валютой
        /// </summary>
        public List<string> CurrencyTabs { get; set; } = new List<string> { "Currency" };

        public double TownPauseDynamicFactor
        {
            get
            {
                return _townPauseDynamicFactor;
            }
            set
            {
                _townPauseDynamicFactor = value;
                //((NotificationObject)this).NotifyPropertyChanged<double>((Expression<Func<double>>)(() => TownPauseDynamicFactor));
            }
        }

        private double _townPauseDynamicFactor = 0.01;

        public double TownMovePauseFactor
        {
            
            get
            {
                return _townMovePauseFactor;
            }
            
            set
            {
                _townMovePauseFactor = value;
            }
        }

        
        private double _townMovePauseFactor = 1.0;

        private LbotoSettings()
            : base(GetSettingsFilePath(Configuration.Instance.Name, "Lboto.json")) { }

        [JsonConverter(typeof(StringEnumConverter))]
        public Dictionary<PauseTypeEnum, List<PauseData>> PauseDataCollection { get; set; } = new Dictionary<PauseTypeEnum, List<PauseData>>();
    }

}

