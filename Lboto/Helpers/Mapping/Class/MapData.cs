using DreamPoeBot.Loki.Common;
using log4net;
using System.Collections.Generic;

namespace Lboto.Helpers.Mapping
{
    public class MapData
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();

        public static readonly List<string> AtlasBossNames;

        public static readonly Dictionary<string, HashSet<string>> MapBossesNames;

        private int _ExplorationPercent = -1;

        private bool? _FastTransition;

        private bool _Ignored;

        private bool _IgnoreBossroom;

        private readonly string _name;
               
        private readonly MapType _type;
               
        private int _Priority;
               
        private int _MobRemaining = -1;
               
        private int _FinalPulse = -1;
               
        private bool _BossRush;
               
        private bool _Unsupported;
               
        private bool _UnsupportedBossroom;

       
        private static MapData _Current;

        public string Name
        {
           
            get
            {
                return _name;
            }
        }

        public MapType Type
        {
           
            get
            {
                return _type;
            }
        }

        public int Priority
        {
           
            get
            {
                return _Priority;
            }
           
            set
            {
                _Priority = value;
            }
        }

        public int MobRemaining
        {
           
            get
            {
                return _MobRemaining;
            }
           
            set
            {
                _MobRemaining = value;
            }
        }

        public int FinalPulse
        {
           
            get
            {
                return _FinalPulse;
            }
           
            set
            {
                _FinalPulse = value;
            }
        }

        public bool BossRush
        {
           
            get
            {
                return _BossRush;
            }
           
            set
            {
                _BossRush = value;
            }
        }

        public bool Ignored
        {
            get
            {
                return Unsupported || _Ignored;
            }
            set
            {
                _Ignored = value;
            }
        }

        public bool IgnoreBossroom
        {
            get
            {
                return Type == MapType.Bossroom && (UnsupportedBossroom || _IgnoreBossroom);
            }
            set
            {
                _IgnoreBossroom = value;
            }
        }

        public int ExplorationPercent
        {
            get
            {
                return (Type == MapType.Regular || Type == MapType.Bossroom) ? _ExplorationPercent : (-1);
            }
            set
            {
                _ExplorationPercent = value;
            }
        }

        public bool? FastTransition
        {
            get
            {
                return (Type == MapType.Multilevel || Type == MapType.Complex) ? _FastTransition : new bool?(false);
            }
            set
            {
                _FastTransition = value;
            }
        }

        public bool Unsupported
        {
           
            get
            {
                return _Unsupported;
            }
           
            internal set
            {
                _Unsupported = value;
            }
        }

        public bool UnsupportedBossroom
        {
           
            get
            {
                return _UnsupportedBossroom;
            }
           
            internal set
            {
                _UnsupportedBossroom = value;
            }
        }

        public static MapData Current
        {
           
            get
            {
                return _Current;
            }
           
            private set
            {
                _Current = value;
            }
        }

        public static List<string> SupportedUniqueMapNames()
        {
            return new List<string> { "Acton's Nightmare", "Caer Blaidd, Wolfpack's Den", "Maelström of Chaos", "Mao Kun", "Oba's Cursed Trove", "Poorjoy's Asylum", "The Beachhead", "Untainted Paradise", "Vaults of Atziri" };
        }

        public MapData(string name, MapType type)
        {
            _name = name;
            _type = type;
        }

        public MapData(MapData other)
        {
            _name = other.Name;
            _type = other.Type;
            Priority = other.Priority;
            Ignored = other.Ignored;
            IgnoreBossroom = other.IgnoreBossroom;
            MobRemaining = other.MobRemaining;
            ExplorationPercent = other.ExplorationPercent;
            FinalPulse = other.FinalPulse;
            BossRush = other.BossRush;
            FastTransition = other.FastTransition;
            Unsupported = other.Unsupported;
            UnsupportedBossroom = other.UnsupportedBossroom;
        }

        internal static void ResetCurrent()
        {
            string name = World.CurrentArea.Name;
            if (!MapSettings.Instance.MapDict.TryGetValue(name, out var value))
            {
                Log.Error("[MapData] Unknown map name \"" + name + "\". MapBot will use global settings for this area.");
                Current = CreateFromGlobal(name);
                return;
            }
            MapData mapData = new MapData(value);
            GeneralSettings instance = GeneralSettings.Instance;
            if (mapData.MobRemaining == -1)
            {
                mapData.MobRemaining = instance.MobRemaining;
            }
            if (mapData.ExplorationPercent == -1)
            {
                mapData.ExplorationPercent = instance.ExplorationPercent;
            }
            if (!mapData.FastTransition.HasValue)
            {
                mapData.FastTransition = instance.FastTransition;
            }
            MapType type = mapData.Type;
            Log.Debug("[MapData] Name: " + mapData.Name);
            Log.Debug($"[MapData] Monster remaining: {mapData.MobRemaining}");
            Log.Debug($"[MapData] Final pulses: {mapData.FinalPulse}");
            if (type != 0 && type != MapType.Bossroom)
            {
                Log.Debug("[MapData] Exploration percent: not used");
            }
            else
            {
                Log.Debug($"[MapData] Exploration percent: {mapData.ExplorationPercent}");
            }
            Log.Debug($"[MapData] BossRush: {mapData.BossRush}");
            if (MapBossesNames.TryGetValue(World.CurrentArea.Name, out var value2))
            {
                string text = string.Join(",", value2);
                Log.Debug("[MapData] Bosses: " + text);
            }
            Current = mapData;
        }

        private static MapData CreateFromGlobal(string areaName)
        {
            GeneralSettings instance = GeneralSettings.Instance;
            return new MapData(areaName, MapType.Regular)
            {
                MobRemaining = instance.MobRemaining,
                ExplorationPercent = instance.ExplorationPercent,
                FastTransition = instance.FastTransition
            };
        }

        static MapData()
        {
            AtlasBossNames = new List<string> { "Veritania, The Redeemer", "Drox, The Warlord", "Al-Hezmin, The Hunter", "Baran, The Crusader", "The Eradicator", "The Constrictor", "The Purifier", "The Enslaver" };
            MapBossesNames = new Dictionary<string, HashSet<string>>
            {
                [MapNames.AridLake] = new HashSet<string> { "Drought-Maddened Rhoa" },
                //[MapNames.ColdRiver] = new HashSet<string> { "Ara, Sister of Light", "Khor, Sister of Shadows" },
                [MapNames.SpiderForest] = new HashSet<string> { "Enticer of Rot" },
                [MapNames.UndergroundSea] = new HashSet<string> { "Merveil, the Reflection", "Merveil, the Returned" },
                [MapNames.PutridCloister] = new HashSet<string> { "Headmistress Braeta" },
                [MapNames.Plaza] = new HashSet<string> { "The Goddess" },
                [MapNames.FrozenCabins] = new HashSet<string> { "Captain Clayborne, The Accursed" },
                [MapNames.Bog] = new HashSet<string> { "Skullbeak" },
                [MapNames.Peninsula] = new HashSet<string> { "Titan of the Grove" },
                [MapNames.Dungeon] = new HashSet<string> { "Penitentiary Incarcerator" },
                [MapNames.AcidCaverns] = new HashSet<string> { "Rama, The Kinslayer", "Kalria, The Fallen", "Invari, The Bloodshaper", "Lokan, The Deceiver", "Marchak, The Betrayer", "Berrots, The Breaker", "Vessider, The Unrivaled", "Morgrants, The Deafening" },
                [MapNames.Museum] = new HashSet<string> { "He of Many Pieces" },
                [MapNames.Tower] = new HashSet<string> { "Liantra", "Bazur" },
                [MapNames.TwilightTemple] = new HashSet<string> { "Helial, the Day Unending", "Selenia, the Endless Night" },
                [MapNames.Volcano] = new HashSet<string> { "Forest of Flames" },
                [MapNames.InfestedValley] = new HashSet<string> { "Gorulis, Will-Thief" },
                [MapNames.Ramparts] = new HashSet<string> { "The Reaver" },
                [MapNames.Reef] = new HashSet<string> { "Nassar, Lion of the Seas" },
                [MapNames.Necropolis] = new HashSet<string> { "Burtok, Conjurer of Bones" },
                [MapNames.Carcass] = new HashSet<string> { "Amalgam of Nightmares" },
                [MapNames.Basilica] = new HashSet<string> { "Konley, the Unrepentant" },
                [MapNames.MoonTemple] = new HashSet<string> { "Sebbert, Crescent's Point" },
                [MapNames.HallowedGround] = new HashSet<string> { "Maker of Mires" },
                [MapNames.ActonNightmare] = new HashSet<string> { "Rose", "Thorn" },
                [MapNames.PoorjoyAsylum] = new HashSet<string> { "Mistress Hyseria" },
                [MapNames.Arsenal] = new HashSet<string> { "The Steel Soul" },
                [MapNames.AshenWood] = new HashSet<string> { "Lord of the Ashen Arrow" },
                [MapNames.Bazaar] = new HashSet<string> { "Ancient Sculptor" },
                [MapNames.Cemetery] = new HashSet<string> { "Erebix, Light's Bane" },
                [MapNames.Armoury] = new HashSet<string> { "Warmonger" },
                [MapNames.Temple] = new HashSet<string> { "Jorus, Sky's Edge" },
                [MapNames.OvergrownShrine] = new HashSet<string> { "Maligaro the Mutilator" },
                [MapNames.CrimsonTemple] = new HashSet<string> { "The Sanguine Siren" },
                [MapNames.MaoKun] = new HashSet<string> { "Fairgraves, Never Dying" },
                [MapNames.DoryaniMachinarium] = new HashSet<string> { "The Apex Assembly" },
                [MapNames.DrySea] = new HashSet<string> { "Gazuul, Droughtspawn" },
                [MapNames.Arcade] = new HashSet<string> { "Herald of Ashes", "Herald of Thunder" },
                [MapNames.MudGeyser] = new HashSet<string> { "Tunneltrap" },
                [MapNames.TropicalIsland] = new HashSet<string> { "Blood Progenitor" },
                [MapNames.Shore] = new HashSet<string> { "Belcer, the Pirate Lord" },
                [MapNames.Maze] = new HashSet<string> { "Shadow of the Vaal" },
                [MapNames.Arena] = new HashSet<string> { "Avatar of the Huntress", "Avatar of the Skies", "Avatar of the Forge" },
                [MapNames.ToxicSewer] = new HashSet<string> { "Arachnoxia" },
                [MapNames.MaelstromOfChaos] = new HashSet<string> { "Merveil, the Reflection", "Merveil, the Returned" },
                [MapNames.PrimordialPool] = new HashSet<string> { "Nightmare's Omen" },
                [MapNames.BrambleValley] = new HashSet<string> { "Melur Thornmaul", "Elida Blisterclaw", "Orvi Acidbeak" },
                [MapNames.CrimsonTownship] = new HashSet<string> { "Kadaris, Crimson Mayor" },
                [MapNames.Orchard] = new HashSet<string> { "Vision of Justice" },
                [MapNames.Lair] = new HashSet<string> { "Lycius, Midnight's Howl" },
                [MapNames.Atoll] = new HashSet<string> { "Puruna, the Challenger" },
                [MapNames.Summit] = new HashSet<string> { "Mephod, the Earth Scorcher", "Mephod, the Earth Scorcher" },
                [MapNames.OlmecSanctum] = new HashSet<string> { "Olmec, the All Stone" },
                [MapNames.VinktarSquare] = new HashSet<string> { "Avatar of Thunder" },
                [MapNames.Lookout] = new HashSet<string> { "The Grey Plague" },
                [MapNames.CitySquare] = new HashSet<string> { "Carius, the Unnatural", "Pileah, Corpse Burner", "Pileah, Burning Corpse" },
                [MapNames.Courtyard] = new HashSet<string> { "Oriath's Virtue", "Oriath's Vengeance", "Oriath's Vigil" },
                [MapNames.Thicket] = new HashSet<string> { "Sanctum Enforcer" },
                [MapNames.IvoryTemple] = new HashSet<string> { "Sanctum Enforcer", "Sanctum Guardian" },
                [MapNames.BoneCrypt] = new HashSet<string> { "Xixic, High Necromancer" },
                [MapNames.Sepulchre] = new HashSet<string> { "Doedre the Defiler", "Doedre the Defiler" },
                [MapNames.Gardens] = new HashSet<string> { "Sallazzang", "Sallazzang" },
                [MapNames.Desert] = new HashSet<string> { "Preethi, Eye-Pecker" },
                [MapNames.Mesa] = new HashSet<string> { "Oak the Mighty" },
                [MapNames.DesertSpring] = new HashSet<string> { "Terror of the Infinite Drifts", "Terror of the Infinite Drifts" },
                [MapNames.Colonnade] = new HashSet<string> { "Tyrant" },
                [MapNames.Terrace] = new HashSet<string> { "Varhesh, Shimmering Aberration" },
                [MapNames.Core] = new HashSet<string> { "Eater of Souls", "Eater of Souls" },
                [MapNames.Glacier] = new HashSet<string> { "Rek'tar, the Breaker" },
                [MapNames.Promenade] = new HashSet<string> { "Blackguard Avenger", "Blackguard Tempest" },
                [MapNames.OvergrownRuin] = new HashSet<string> { "Visceris" },
                [MapNames.LavaChamber] = new HashSet<string> { "Fire and Fury" },
                [MapNames.Grotto] = new HashSet<string> { "Void Anomaly" },
                [MapNames.CrystalOre] = new HashSet<string> { "Lord of the Hollows", "Messenger of the Hollows", "Champion of the Hollows" },
                [MapNames.PillarsOfArun] = new HashSet<string> { "Talin, Faithbreaker" },
                [MapNames.Marshes] = new HashSet<string> { "Tore, Towering Ancient" },
                [MapNames.Dunes] = new HashSet<string> { "The Blacksmith" },
                [MapNames.Canyon] = new HashSet<string> { "Gnar, Eater of Carrion", "Stonebeak, Battle Fowl" },
                [MapNames.JungleValley] = new HashSet<string> { "Queen of the Great Tangle" },
                [MapNames.Mausoleum] = new HashSet<string> { "Tolman, the Exhumer" },
                [MapNames.WastePool] = new HashSet<string> { "Portentia, the Foul" },
                [MapNames.GraveTrough] = new HashSet<string> { "The Restless Shade" },
                [MapNames.Precinct] = new HashSet<string> { "Orra Greengate", "Torr Olgosso", "Damoi Tui", "Eoin Greyfur", "Wilorin Demontamer", "Augustina Solaria", "Igna Phoenix" },
                [MapNames.DarkForest] = new HashSet<string> { "The Cursed King" },
                [MapNames.Wharf] = new HashSet<string> { "Stone of the Currents" },
                [MapNames.Cage] = new HashSet<string> { "Executioner Bloodwing" },
                [MapNames.Villa] = new HashSet<string> { "The High Templar" },
                [MapNames.CaerBlaiddWolfpackDen] = new HashSet<string> { "Winterfang", "Storm Eye", "Solus, Pack Alpha" },
                [MapNames.Pit] = new HashSet<string> { "Olof, Son of the Headsman" },
                [MapNames.MineralPools] = new HashSet<string> { "Shock and Horror" },
                [MapNames.Port] = new HashSet<string> { "Unravelling Horror", "Unravelling Horror", "Unravelling Horror", "Unravelling Horror", "Unravelling Horror" },
                [MapNames.UndergroundRiver] = new HashSet<string> { "It That Fell" },
                [MapNames.VaalPyramid] = new HashSet<string> { "The Broken Prince", "The Fallen Queen", "The Hollow Lady" },
                [MapNames.Foundry] = new HashSet<string> { "The Shifting Ire", "Ciergan, Shadow Alchemist" },
                [MapNames.Wasteland] = new HashSet<string> { "The Brittle Emperor" },
                [MapNames.CoralRuins] = new HashSet<string> { "Captain Tanner Lightfoot" },
                [MapNames.SunkenCity] = new HashSet<string> { "Armala, the Widow" },
                [MapNames.Factory] = new HashSet<string> { "Pesquin, the Mad Baron" },
                [MapNames.Laboratory] = new HashSet<string> { "Riftwalker" },
                [MapNames.WhakawairuaTuahu] = new HashSet<string> { "Tormented Temptress" },
                [MapNames.Beach] = new HashSet<string> { "Glace" },
                [MapNames.Alleyways] = new HashSet<string> { "Calderus" },
                [MapNames.Fields] = new HashSet<string> { "Drek, Apex Hunter" },
                [MapNames.Strand] = new HashSet<string> { "Master of the Blade", "Massier" },
                [MapNames.FungalHollow] = new HashSet<string> { "Aulen Greychain" },
                [MapNames.CowardTrial] = new HashSet<string> { "Infector of Dreams" },
                [MapNames.Park] = new HashSet<string> { "Suncaller Asha" },
                [MapNames.Barrows] = new HashSet<string> { "Beast of the Pits" },
                [MapNames.CursedCrypt] = new HashSet<string> { "Pagan Bishop of Agony" },
                [MapNames.Cells] = new HashSet<string> { "Shavronne the Sickening" },
                [MapNames.Palace] = new HashSet<string> { "God's Chosen" },
                [MapNames.PitOfChimera] = new HashSet<string> { "Guardian of the Chimera" },
                [MapNames.Caldera] = new HashSet<string> { "The Infernal King" },
                [MapNames.LavaLake] = new HashSet<string> { "Kitava, The Destroyer" },
                [MapNames.LairOfHydra] = new HashSet<string> { "Guardian of the Hydra" },
                [MapNames.MazeOfMinotaur] = new HashSet<string> { "Guardian of the Minotaur" },
                [MapNames.VaalTemple] = new HashSet<string> { "K'aj Q'ura", "K'aj Y'ara'az", "K'aj A'alai" },
                [MapNames.BurialChambers] = new HashSet<string> { "Witch of the Cauldron" },
                [MapNames.ForgeOfPhoenix] = new HashSet<string> { "Guardian of the Phoenix" },
                [MapNames.Residence] = new HashSet<string> { "Excellis Aurafix" },
                ["Absence of Patience and Wisdom"] = new HashSet<string> { "The Searing Exarch", "The Searing Exarch", "The Searing Exarch", "The Searing Exarch" },
                ["Absence of Symmetry and Harmony"] = new HashSet<string> { "The Eater of Worlds", "The Eater of Worlds", "The Eater of Worlds", "The Eater of Worlds" },
                ["Polaric Void"] = new HashSet<string> { "The Black Star", "The Black Star", "The Black Star", "The Black Star" },
                ["Seething Chyme"] = new HashSet<string> { "The Infinite Hunger", "The Infinite Hunger", "The Infinite Hunger", "The Infinite Hunger" },
                [MapNames.Graveyard] = new HashSet<string> { "Thunderskull", "Champion of Frost", "Steelpoint the Avenger" },
                [MapNames.Pen] = new HashSet<string> { "Arwyn, the Houndmaster" },
                [MapNames.FloodedMine] = new HashSet<string> { "The Eroding One" },
                [MapNames.Iceberg] = new HashSet<string> { "Yorishi, Aurora-sage", "Jeinei Yuushu", "Otesha, the Giantslayer" },
                [MapNames.Excavation] = new HashSet<string> { "Shrieker Eihal", "Breaker Toruul" },
                [MapNames.Leyline] = new HashSet<string> { "Mirage of Bones" },
                [MapNames.RelicChambers] = new HashSet<string> { "Litanius, the Black Prayer" },
                [MapNames.Courthouse] = new HashSet<string> { "Bolt Brownfur, Earth Churner", "Thena Moga, the Crimson Storm", "Ion Darkshroud, the Hungering Blade" },
                [MapNames.Chateau] = new HashSet<string> { "Hephaeus, The Hammer" },
                [MapNames.Lighthouse] = new HashSet<string> { "Uruk Baleh", "El'Abin, Bloodeater", "Leli Goya, Daughter of Ash", "Bin'aia, Crimson Rain" },
                [MapNames.Conservatory] = new HashSet<string> { "The Forgotten Soldier" },
                [MapNames.SulphurVents] = new HashSet<string> { "The Gorgon" },
                [MapNames.HauntedMansion] = new HashSet<string> { "Barthol, the Pure", "Barthol, the Corruptor" },
                [MapNames.Channel] = new HashSet<string> { "The Winged Death" },
                [MapNames.AncientCity] = new HashSet<string> { "Legius Garhall" },
                [MapNames.SpiderLair] = new HashSet<string> { "Thraxia" },
                [MapNames.Phantasmagoria] = new HashSet<string> { "Erythrophagia" },
                [MapNames.Academy] = new HashSet<string> { "The Arbiter of Knowledge" },
                [MapNames.Crater] = new HashSet<string> { "Megaera" },
                [MapNames.ArachnidNest] = new HashSet<string> { "Spinner of False Hope" },
                [MapNames.Geode] = new HashSet<string> { "Avatar of Undoing" },
                [MapNames.Plateau] = new HashSet<string> { "Puruna, the Challenger", "Poporo, the Highest Spire" },
                [MapNames.Estuary] = new HashSet<string> { "Sumter the Twisted" },
                [MapNames.Vault] = new HashSet<string> { "Guardian of the Vault" },
                [MapNames.Scriptorium] = new HashSet<string> { "Gisale, Thought Thief" },
                [MapNames.Siege] = new HashSet<string> { "Tahsin, Warmaker" },
                [MapNames.Shipyard] = new HashSet<string> { "Musky \"Two-Eyes\" Grenn", "Susara, Siren of Pondium", "Lussi \"Rotmother\" Roth" },
                [MapNames.Belfry] = new HashSet<string> { "Lord of the Grey" },
                [MapNames.ArachnidTomb] = new HashSet<string> { "Hybrid Widow" },
                [MapNames.Pier] = new HashSet<string> { "Ancient Architect" },
                [MapNames.Coves] = new HashSet<string> { "Telvar, the Inebriated", "Pirate Treasure" },
                [MapNames.Waterways] = new HashSet<string> { "Fragment of Winter" },
                [MapNames.DefiledCathedral] = new HashSet<string> { "Woad, Mockery of Man" },
                [MapNames.CastleRuins] = new HashSet<string> { "Leif, the Swift-Handed" },
                [MapNames.Racecourse] = new HashSet<string> { "Shredder of Gladiators", "Crusher of Gladiators", "Bringer of Blood" },
                [MapNames.Ghetto] = new HashSet<string> { "Lady Stormflay" },
                [MapNames.Malformation] = new HashSet<string> { "Nightmare Manifest" },
                [MapNames.Shrine] = new HashSet<string> { "Piety the Empyrean" },
                [MapNames.Colosseum] = new HashSet<string> { "Ambrius, Legion Slayer" },
                [MapNames.CrimsonTemple] = new HashSet<string> { "The Sanguine Siren" },
                [MapNames.Dig] = new HashSet<string> { "Stalker of the Endless Dunes" },
                [MapNames.ForkingRiver] = new HashSet<string> { "Ormud, Fiend of the Flood" },
                [MapNames.Silo] = new HashSet<string> { "Renkarr, The Kiln Keeper" },
                [MapNames.Stagnation] = new HashSet<string> { "Murgeth Bogsong" },
                [MapNames.ForbiddenWoods] = new HashSet<string> { "Skictis, Frostkeeper", "Takatax Brittlethorn", "Corruptor Eedaiak" }
            };
        }
    }
}
