using DreamPoeBot.Loki;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Lboto.Helpers.Mapping
{
    /// <summary>
    /// General settings class for MapBot configuration
    /// Contains all bot behavior settings, upgrade configurations, and collections management
    /// </summary>
    public class GeneralSettings : JsonSettings
    {
        // Singleton instance
        private static GeneralSettings _instance;

        // Static configuration lists
        [JsonIgnore]
        public static List<int> SlotValues;

        [JsonIgnore]
        public static List<string> OilTypes;

        // Private backing fields for properties with custom setters
        private int _auraSwapSlot;
        private bool _useFiveSlotMapDevice;
        private bool _useSixSlotMapDevice;
        private bool _isOnRun;
        private bool _stopRequested;
        private string _replaceAuraSkillName;

        // Core bot behavior settings

        private bool _enableMapLootStatistics;
        private bool _openPortals;
        private bool _anointMaps;
        private bool _startLegions;
        private bool _activateDelirium;
        private bool _openBreaches;
        private bool _onlyRunEnchantedMaps;
        private bool _buildMetamorph;
        private bool _enableHarvest;
        private bool _pickSulphite;
        private bool _runConquerorMaps;
        private bool _socketVoidstonesFromInventory;
        private bool _useMapDeviceMods;
        private string _mapDeviceModName;
        private double _maxFragmentCost;
        // NPC interaction settings

        private bool _buyMapsFromKirac;
        private bool _openMavenInvitations;
        private bool _openMiniBossQuestInvitations;
        private bool _openBossQuestInvitations;
        private int _minTierToUsePrioritizedScarabs;
        // Blight map settings

        private bool _runBlightedMaps;
        private bool _runBlightRavagedMaps;
        private bool _alchemyBlightedMaps;
        private bool _alchemyRavagedMaps;
        private bool _vaalBlightedMaps;

        // Master mission settings

        private bool _runAlvaMissions;
        private bool _runEinhardMissions;
        private bool _runJunMissions;
        private bool _runNikoMissions;
        private bool _runKiracMissions;

        // Map tier and completion settings

        private int _maxMapTier;
        private int _mobsRemaining;
        private int _explorationPercent;

        // Additional map running options

        private bool _enterCorruptedAreas;
        private bool _fastTransition;
        private bool _runUnidentifiedMaps;
        private bool _returnForChaosRecipe;

        // Selling configuration

        private bool _sellEnabled;
        private bool _sellIgnoredMaps;
        private int _maxSellTier;
        private int _maxSellPriority;
        private int _minMapAmount;

        // Atlas and exploration settings

        private bool _atlasExplorationEnabled;
        private bool _kiracChecked;
        private bool _bossRushMode;

        // Map upgrade configurations

        private Upgrade _magicUpgrade = new Upgrade
        {
            TierEnabled = true
        };


        private Upgrade _rareUpgrade = new Upgrade();
        private Upgrade _chiselUpgrade = new Upgrade();
        private Upgrade _vaalUpgrade = new Upgrade();
        private Upgrade _fragmentUpgrade = new Upgrade();
        private Upgrade _magicRareUpgrade = new Upgrade();

        // Map quality requirements

        private int _minMapIIQMinTier;
        private int _minMapIIQ;
        private bool _useChargedCompass;

        // Rare item handling

        private ExistingRares _existingRares;
        private RareReroll _rareRerollMethod;

        // Collections for various game items and configurations

        private ObservableCollection<NameEntry> _scarabsToIgnore = new ObservableCollection<NameEntry>();
        private ObservableCollection<NameEntry> _scarabsToPrioritize = new ObservableCollection<NameEntry>();
        private ObservableCollection<NameEntry> _enchantsToPrioritize = new ObservableCollection<NameEntry>();
        private ObservableCollection<NameEntry> _itemNamesToIgnoreInTotalChaosValueCalc = new ObservableCollection<NameEntry>();
        private ObservableCollection<OilEntry> _anointOils = new ObservableCollection<OilEntry>();
        //private ObservableCollection<GolemEntry> _golemsToSummon = new ObservableCollection<GolemEntry>();

        // Atlas influence settings

        private bool _mavenInfluence;
        private bool _searingExarchInfluence;
        private bool _eaterOfWorldsInfluence;

        /// <summary>
        /// Singleton instance accessor
        /// </summary>
        public static GeneralSettings Instance => _instance ?? (_instance = new GeneralSettings());

        /// <summary>
        /// Whether to use the five-slot map device instead of four-slot
        /// </summary>
        [DefaultValue(false)]
        public bool UseFiveSlotMapDevice
        {
            get => _useFiveSlotMapDevice;
            set
            {
                if (!value.Equals(_useFiveSlotMapDevice))
                {
                    _useFiveSlotMapDevice = value;
                    //((NotificationObject)this).NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => UseFiveSlotMapDevice));
                }
            }
        }

        /// <summary>
        /// Whether to use the six-slot map device instead of four-slot
        /// </summary>
        public bool UseSixSlotMapDevice
        {
            get => _useSixSlotMapDevice;
            set
            {
                if (!value.Equals(_useSixSlotMapDevice))
                {
                    _useSixSlotMapDevice = value;
                    //((NotificationObject)this).NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => UseFiveSlotMapDevice));
                }
            }
        }

        /// <summary>
        /// Indicates if the bot is currently running a map
        /// </summary>
        [DefaultValue(false)]
        public bool IsOnRun
        {
            get => _isOnRun;
            set
            {
                if (!value.Equals(_isOnRun))
                {
                    _isOnRun = value;
                   //((NotificationObject)this).NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => IsOnRun));
                }
            }
        }

        /// <summary>
        /// Check if Simulacrum plugin is enabled
        /// </summary>
        public static bool SimulacrumsEnabled => PluginManager.EnabledPlugins.Any((IPlugin p) => ((IAuthored)p).Name.Equals("SimulacrumPluginEx"));

        /// <summary>
        /// Enable collection of map loot statistics
        /// </summary>
        [DefaultValue(false)]
        public bool EnableMapLootStatistics
        {

            get => _enableMapLootStatistics;

            set => _enableMapLootStatistics = value;
        }

        /// <summary>
        /// Automatically open portals when entering areas
        /// </summary>
        [DefaultValue(true)]
        public bool OpenPortals
        {

            get => _openPortals;

            set => _openPortals = value;
        }

        /// <summary>
        /// Automatically anoint maps with oils
        /// </summary>
        [DefaultValue(true)]
        public bool AnointMaps
        {

            get => _anointMaps;

            set => _anointMaps = value;
        }

        /// <summary>
        /// Automatically activate Legion monoliths
        /// </summary>
        [DefaultValue(true)]
        public bool StartLegions
        {

            get => _startLegions;

            set => _startLegions = value;
        }

        /// <summary>
        /// Automatically activate Delirium mirrors
        /// </summary>
        [DefaultValue(true)]
        public bool ActivateDelirium
        {

            get => _activateDelirium;

            set => _activateDelirium = value;
        }

        /// <summary>
        /// Automatically open Breach hands
        /// </summary>
        [DefaultValue(true)]
        public bool OpenBreaches
        {

            get => _openBreaches;

            set => _openBreaches = value;
        }

        /// <summary>
        /// Only run maps that have enchantments
        /// </summary>
        [DefaultValue(false)]
        public bool OnlyRunEnchantedMaps
        {

            get => _onlyRunEnchantedMaps;

            set => _onlyRunEnchantedMaps = value;
        }

        /// <summary>
        /// Automatically build Metamorph bosses when possible
        /// </summary>
        [DefaultValue(false)]
        public bool BuildMetamorph
        {

            get => _buildMetamorph;

            set => _buildMetamorph = value;
        }

        /// <summary>
        /// Enable interaction with Harvest gardens
        /// </summary>
        [DefaultValue(true)]
        public bool EnableHarvest
        {

            get => _enableHarvest;

            set => _enableHarvest = value;
        }

        /// <summary>
        /// Automatically collect Sulphite deposits
        /// </summary>
        [DefaultValue(true)]
        public bool PickSulphite
        {

            get => _pickSulphite;

            set => _pickSulphite = value;
        }

        /// <summary>
        /// Run maps with Conqueror influence
        /// </summary>
        [DefaultValue(false)]
        public bool RunConqMaps
        {

            get => _runConquerorMaps;

            set => _runConquerorMaps = value;
        }

        /// <summary>
        /// Automatically socket Voidstones from inventory
        /// </summary>
        [DefaultValue(true)]
        public bool SocketVoidstonesFromInventory
        {

            get => _socketVoidstonesFromInventory;

            set => _socketVoidstonesFromInventory = value;
        }

        /// <summary>
        /// Enable use of Map Device modifications
        /// </summary>
        [DefaultValue(false)]
        public bool UseMapDeviceMods
        {

            get => _useMapDeviceMods;

            set => _useMapDeviceMods = value;
        }

        /// <summary>
        /// Name of the Map Device mod to use (e.g., "Free", "Breach", etc.)
        /// </summary>
        [DefaultValue("Free")]
        public string MapDeviceModName
        {

            get => _mapDeviceModName;

            set => _mapDeviceModName = value;
        }

        /// <summary>
        /// Maximum cost in chaos for fragment-based map device mods
        /// </summary>
        [DefaultValue(1)]
        public double MaxFragmentCost
        {

            get => _maxFragmentCost;

            set => _maxFragmentCost = value;
        }

        /// <summary>
        /// Purchase maps from Kirac when available
        /// </summary>
        [DefaultValue(true)]
        public bool BuyMapsFromKirac
        {

            get => _buyMapsFromKirac;

            set => _buyMapsFromKirac = value;
        }

        /// <summary>
        /// Automatically run Maven witness invitations
        /// </summary>
        [DefaultValue(true)]
        public bool OpenMavenIvitations
        {

            get => _openMavenInvitations;

            set => _openMavenInvitations = value;
        }

        /// <summary>
        /// Run mini-boss quest invitations (e.g., Elder Guardians)
        /// </summary>
        [DefaultValue(true)]
        public bool OpenMiniBossQuestInvitations
        {

            get => _openMiniBossQuestInvitations;

            set => _openMiniBossQuestInvitations = value;
        }

        /// <summary>
        /// Run main boss quest invitations (e.g., Shaper, Elder)
        /// </summary>
        [DefaultValue(true)]
        public bool OpenBossQuestInvitations
        {

            get => _openBossQuestInvitations;

            set => _openBossQuestInvitations = value;
        }

        /// <summary>
        /// Minimum map tier to start using prioritized scarabs
        /// </summary>
        [DefaultValue(12)]
        public int MinTierToUsePrioritisedScarabs
        {

            get => _minTierToUsePrioritizedScarabs;

            set => _minTierToUsePrioritizedScarabs = value;
        }

        /// <summary>
        /// Run Blighted maps when encountered
        /// </summary>
        [DefaultValue(true)]
        public bool RunBlightedMaps
        {

            get => _runBlightedMaps;

            set => _runBlightedMaps = value;
        }

        /// <summary>
        /// Run Blight-ravaged maps when encountered
        /// </summary>
        [DefaultValue(false)]
        public bool RunBlightRavagedMaps
        {

            get => _runBlightRavagedMaps;

            set => _runBlightRavagedMaps = value;
        }

        /// <summary>
        /// Use Alchemy Orbs on Blighted maps
        /// </summary>
        [DefaultValue(false)]
        public bool AlchBlightedMaps
        {

            get => _alchemyBlightedMaps;

            set => _alchemyBlightedMaps = value;
        }

        /// <summary>
        /// Use Alchemy Orbs on Blight-ravaged maps
        /// </summary>
        [DefaultValue(false)]
        public bool AlchRavagedMaps
        {

            get => _alchemyRavagedMaps;

            set => _alchemyRavagedMaps = value;
        }

        /// <summary>
        /// Use Vaal Orbs on Blighted maps
        /// </summary>
        [DefaultValue(false)]
        public bool VaalBlightedMaps
        {

            get => _vaalBlightedMaps;

            set => _vaalBlightedMaps = value;
        }

        /// <summary>
        /// Run Alva (Incursion) missions when available
        /// </summary>
        [DefaultValue(true)]
        public bool RunAlvaMissions
        {

            get => _runAlvaMissions;

            set => _runAlvaMissions = value;
        }

        /// <summary>
        /// Run Einhard (Bestiary) missions when available
        /// </summary>
        [DefaultValue(true)]
        public bool RunEinhardMissions
        {

            get => _runEinhardMissions;

            set => _runEinhardMissions = value;
        }

        /// <summary>
        /// Run Jun (Betrayal) missions when available
        /// </summary>
        [DefaultValue(true)]
        public bool RunJunMissions
        {

            get => _runJunMissions;

            set => _runJunMissions = value;
        }

        /// <summary>
        /// Run Niko (Delve) missions when available
        /// </summary>
        [DefaultValue(true)]
        public bool RunNikoMissions
        {

            get => _runNikoMissions;

            set => _runNikoMissions = value;
        }

        /// <summary>
        /// Run Kirac missions when available
        /// </summary>
        [DefaultValue(true)]
        public bool RunKiracMissions
        {

            get => _runKiracMissions;

            set => _runKiracMissions = value;
        }

        /// <summary>
        /// Maximum tier of maps to run
        /// </summary>
        [DefaultValue(10)]
        public int MaxMapTier
        {

            get => _maxMapTier;

            set => _maxMapTier = value;
        }

        /// <summary>
        /// Maximum number of monsters remaining before leaving map
        /// </summary>
        [DefaultValue(20)]
        public int MobRemaining
        {

            get => _mobsRemaining;

            set => _mobsRemaining = value;
        }

        /// <summary>
        /// Minimum exploration percentage before leaving map
        /// </summary>
        [DefaultValue(85)]
        public int ExplorationPercent
        {

            get => _explorationPercent;

            set => _explorationPercent = value;
        }

        /// <summary>
        /// Enter corrupted side areas when found
        /// </summary>
        [DefaultValue(false)]
        public bool EnterCorruptedAreas
        {

            get => _enterCorruptedAreas;

            set => _enterCorruptedAreas = value;
        }

        /// <summary>
        /// Use fast transition between areas (skip some interactions)
        /// </summary>
        [DefaultValue(false)]
        public bool FastTransition
        {

            get => _fastTransition;

            set => _fastTransition = value;
        }

        /// <summary>
        /// Run unidentified maps
        /// </summary>
        [DefaultValue(false)]
        public bool RunUnId
        {

            get => _runUnidentifiedMaps;

            set => _runUnidentifiedMaps = value;
        }

        /// <summary>
        /// Return to hideout specifically for chaos recipe items
        /// </summary>
        [DefaultValue(false)]
        public bool ReturnForChaosRecipe
        {

            get => _returnForChaosRecipe;

            set => _returnForChaosRecipe = value;
        }

        /// <summary>
        /// Enable selling of items to vendors
        /// </summary>
        [DefaultValue(true)]
        public bool SellEnabled
        {

            get => _sellEnabled;

            set => _sellEnabled = value;
        }

        /// <summary>
        /// Sell maps that are in the ignore list
        /// </summary>
        [DefaultValue(true)]
        public bool SellIgnoredMaps
        {

            get => _sellIgnoredMaps;

            set => _sellIgnoredMaps = value;
        }

        /// <summary>
        /// Maximum tier of maps to sell
        /// </summary>
        [DefaultValue(10)]
        public int MaxSellTier
        {

            get => _maxSellTier;

            set => _maxSellTier = value;
        }

        /// <summary>
        /// Maximum priority level of maps to sell
        /// </summary>
        [DefaultValue(10)]
        public int MaxSellPriority
        {

            get => _maxSellPriority;

            set => _maxSellPriority = value;
        }

        /// <summary>
        /// Minimum number of maps to keep in inventory before selling extras
        /// </summary>
        [DefaultValue(7)]
        public int MinMapAmount
        {

            get => _minMapAmount;

            set => _minMapAmount = value;
        }

        /// <summary>
        /// Enable Atlas exploration for completion bonuses
        /// </summary>
        [DefaultValue(true)]
        public bool AtlasExplorationEnabled
        {

            get => _atlasExplorationEnabled;

            set => _atlasExplorationEnabled = value;
        }

        /// <summary>
        /// Whether Kirac has been checked this session
        /// </summary>
        [DefaultValue(false)]
        public bool KiracChecked
        {

            get => _kiracChecked;

            set => _kiracChecked = value;
        }

        /// <summary>
        /// Focus only on boss encounters, skip most mapping
        /// </summary>
        [DefaultValue(false)]
        public bool BossRushMode
        {

            get => _bossRushMode;

            set => _bossRushMode = value;
        }

        /// <summary>
        /// Configuration for upgrading maps to magic quality
        /// </summary>
        public Upgrade MagicUpgrade
        {

            get => _magicUpgrade;

            set => _magicUpgrade = value;
        }

        /// <summary>
        /// Configuration for upgrading maps to rare quality
        /// </summary>
        public Upgrade RareUpgrade
        {

            get => _rareUpgrade;

            set => _rareUpgrade = value;
        }

        /// <summary>
        /// Configuration for using Cartographer's Chisels on maps
        /// </summary>
        public Upgrade ChiselUpgrade
        {

            get => _chiselUpgrade;

            set => _chiselUpgrade = value;
        }

        /// <summary>
        /// Configuration for using Vaal Orbs on maps
        /// </summary>
        public Upgrade VaalUpgrade
        {

            get => _vaalUpgrade;

            set => _vaalUpgrade = value;
        }

        /// <summary>
        /// Configuration for using fragments with maps
        /// </summary>
        public Upgrade FragmentUpgrade
        {

            get => _fragmentUpgrade;

            set => _fragmentUpgrade = value;
        }

        /// <summary>
        /// Configuration for upgrading from magic to rare quality
        /// </summary>
        public Upgrade MagicRareUpgrade
        {

            get => _magicRareUpgrade;

            set => _magicRareUpgrade = value;
        }

        /// <summary>
        /// Minimum tier to enforce IIQ requirements
        /// </summary>
        [DefaultValue(16)]
        public int MinMapIIQMinTier
        {

            get => _minMapIIQMinTier;

            set => _minMapIIQMinTier = value;
        }

        /// <summary>
        /// Minimum Item Increased Quantity required for high-tier maps
        /// </summary>
        [DefaultValue(40)]
        public int MinMapIIQ
        {

            get => _minMapIIQ;

            set => _minMapIIQ = value;
        }

        /// <summary>
        /// Skill bar slot for aura swapping (1-13)
        /// </summary>
        [DefaultValue(12)]
        public int AuraSwapSlot
        {
            get => _auraSwapSlot;
            set
            {
                if (!value.Equals(_auraSwapSlot))
                {
                    _auraSwapSlot = value;
                    //((NotificationObject)this).NotifyPropertyChanged<int>((Expression<Func<int>>)(() => AuraSwapSlot));
                }
            }
        }

        /// <summary>
        /// Name of aura skill to replace during swapping
        /// </summary>
        [DefaultValue("")]
        public string ReplaceAuraSkillName
        {
            get => _replaceAuraSkillName;
            set
            {
                if (!value.Equals(_replaceAuraSkillName))
                {
                    _replaceAuraSkillName = value;
                    //((NotificationObject)this).NotifyPropertyChanged<string>((Expression<Func<string>>)(() => ReplaceAuraSkillName));
                }
            }
        }

        /// <summary>
        /// How to handle existing rare maps
        /// </summary>
        public ExistingRares ExistingRares
        {

            get => _existingRares;

            set => _existingRares = value;
        }

        /// <summary>
        /// Method for rerolling rare maps
        /// </summary>
        public RareReroll RerollMethod
        {

            get => _rareRerollMethod;

            set => _rareRerollMethod = value;
        }

        /// <summary>
        /// Collection of scarab names to ignore/not use
        /// </summary>
        public ObservableCollection<NameEntry> ScarabsToIgnore
        {

            get => _scarabsToIgnore;

            set => _scarabsToIgnore = value;
        }

        /// <summary>
        /// Collection of scarab names to prioritize for use
        /// </summary>
        public ObservableCollection<NameEntry> ScarabsToPrioritize
        {

            get => _scarabsToPrioritize;

            set => _scarabsToPrioritize = value;
        }

        /// <summary>
        /// Collection of map enchantments to prioritize
        /// </summary>
        public ObservableCollection<NameEntry> EnchantsToPrioritize
        {

            get => _enchantsToPrioritize;

            set => _enchantsToPrioritize = value;
        }

        /// <summary>
        /// Collection of item names to ignore when calculating total chaos value
        /// </summary>
        public ObservableCollection<NameEntry> ItemNamesToIgnoreInTotalChaosValueCalc
        {

            get => _itemNamesToIgnoreInTotalChaosValueCalc;

            set => _itemNamesToIgnoreInTotalChaosValueCalc = value;
        }

        /// <summary>
        /// Collection of oils configured for anointing maps
        /// </summary>
        public ObservableCollection<OilEntry> AnointOils
        {

            get => _anointOils;

            set => _anointOils = value;
        }


        /// <summary>
        /// Collection of golems to automatically summon
        /// </summary>
        //public ObservableCollection<GolemEntry> GolemsToSummon
        //{

        //    get => _golemsToSummon;

        //    set => _golemsToSummon = value;
        //}

        /// <summary>
        /// Enable Maven influence on Atlas
        /// </summary>
        public bool MavenInfluence
        {

            get => _mavenInfluence;

            set => _mavenInfluence = value;
        }

        /// <summary>
        /// Enable Searing Exarch influence on Atlas
        /// </summary>
        public bool SearingExarchInfluence
        {

            get => _searingExarchInfluence;

            set => _searingExarchInfluence = value;
        }

        /// <summary>
        /// Enable Eater of Worlds influence on Atlas
        /// </summary>
        public bool EaterOfWorldsInfluence
        {

            get => _eaterOfWorldsInfluence;

            set => _eaterOfWorldsInfluence = value;
        }

        /// <summary>
        /// Flag indicating if a stop has been requested (runtime control)
        /// </summary>
        [JsonIgnore]
        public bool StopRequested
        {
            get => _stopRequested;
            set
            {
                if (value != _stopRequested)
                {
                    _stopRequested = value;
                    //((NotificationObject)this).NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => StopRequested));
                }
            }
        }

        /// <summary>
        /// Private constructor for singleton pattern
        /// Initializes collections and sets up default values
        /// </summary>
        private GeneralSettings()
            : base(JsonSettings.GetSettingsFilePath(new string[3]
            {
            Configuration.Instance.Name,
            "MapBotEx",
            "GeneralSettings.json"
            }))
        {
            // Remove duplicates from collections based on Name/Golem property
            ScarabsToIgnore = new ObservableCollection<NameEntry>(from s in ScarabsToIgnore
                                                                  where !string.IsNullOrWhiteSpace(s.Name)
                                                                  select s into e
                                                                  group e by e.Name into g
                                                                  select g.First());

            EnchantsToPrioritize = new ObservableCollection<NameEntry>(from s in EnchantsToPrioritize
                                                                       where !string.IsNullOrWhiteSpace(s.Name)
                                                                       select s into e
                                                                       group e by e.Name into g
                                                                       select g.First());

            ScarabsToPrioritize = new ObservableCollection<NameEntry>(from s in ScarabsToPrioritize
                                                                      where !string.IsNullOrWhiteSpace(s.Name)
                                                                      select s into e
                                                                      group e by e.Name into g
                                                                      select g.First());

            ItemNamesToIgnoreInTotalChaosValueCalc = new ObservableCollection<NameEntry>(from s in ItemNamesToIgnoreInTotalChaosValueCalc
                                                                                         where !string.IsNullOrWhiteSpace(s.Name)
                                                                                         select s into e
                                                                                         group e by e.Name into g
                                                                                         select g.First());

            AnointOils = new ObservableCollection<OilEntry>(from s in AnointOils
                                                            where !string.IsNullOrWhiteSpace(s.Name)
                                                            select s into e
                                                            group e by e.Name into g
                                                            select g.First());

            //GolemsToSummon = new ObservableCollection<GolemEntry>(from s in GolemsToSummon
            //                                                      where !string.IsNullOrWhiteSpace(s.Golem)
            //                                                      select s into e
            //                                                      group e by e.Golem into g
            //                                                      select g.First());

            // Initialize default prioritized scarabs if collection is null
            if (ScarabsToPrioritize == null)
            {
                ScarabsToPrioritize = new ObservableCollection<NameEntry>
            {
                new NameEntry("Blight"),      // Blight scarabs for tower defense encounters
				new NameEntry("Cartography")  // Cartography scarabs for additional maps
			};
            }

            // Initialize default ignored scarabs if collection is null
            if (ScarabsToIgnore == null)
            {
                ScarabsToIgnore = new ObservableCollection<NameEntry>
            {
                new NameEntry("Shaper"),  // Shaper scarabs (legacy content)
				new NameEntry("Elder")    // Elder scarabs (legacy content)
			};
            }

            // Initialize default items to ignore in chaos value calculation
            if (ItemNamesToIgnoreInTotalChaosValueCalc == null)
            {
                ItemNamesToIgnoreInTotalChaosValueCalc = new ObservableCollection<NameEntry>
            {
                new NameEntry("Clear Oil"),  // Low-value oil
				new NameEntry("Sepia Oil")   // Low-value oil
			};
            }

            // Initialize default oil configuration for anointing
            if (AnointOils == null)
            {
                AnointOils = new ObservableCollection<OilEntry>
            {
				// Crimson Oil: High-tier oil, quantity 3, both blighted and ravaged maps
				new OilEntry("Crimson Oil", 3, -1, blighted: true, ravaged: true),
				
				// Amber Oil: Low-tier oil, quantity 1, only ravaged maps
				new OilEntry("Amber Oil", 1, -1, blighted: false, ravaged: true),
				
				// Silver Oil: High-tier oil, quantity 3, only ravaged maps
				new OilEntry("Silver Oil", 3, -1, blighted: false, ravaged: true),
				
				// Black Oil: Mid-tier oil, quantity 2, only ravaged maps
				new OilEntry("Black Oil", 2, -1, blighted: false, ravaged: true)
            };
            }
        }

        /// <summary>
        /// Static constructor to initialize static collections
        /// </summary>
        static GeneralSettings()
        {
            // Available skill bar slots (1-13, corresponding to skill bar positions)
            SlotValues = new List<int>
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13
        };

            // All available oil types in Path of Exile, ordered by tier/value
            OilTypes = new List<string>
        {
            "Clear Oil",      // Tier 1 (lowest)
			"Sepia Oil",      // Tier 2
			"Amber Oil",      // Tier 3
			"Verdant Oil",    // Tier 4
			"Teal Oil",       // Tier 5
			"Azure Oil",      // Tier 6
			"Indigo Oil",     // Tier 7
			"Violet Oil",     // Tier 8
			"Crimson Oil",    // Tier 9
			"Black Oil",      // Tier 10
			"Opalescent Oil", // Tier 11
			"Silver Oil",     // Tier 12
			"Golden Oil"      // Tier 13 (highest)
		};
        }
    }
}
