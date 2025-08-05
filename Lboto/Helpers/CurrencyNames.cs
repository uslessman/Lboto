using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game.GameData;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Lboto.Helpers
{
    [SuppressMessage("ReSharper", "UnassignedReadonlyField")]
    public static class CurrencyNames
    {
        // Basic Currency
        [ItemMetadata("Metadata/Items/Currency/CurrencyIdentification")]
        public static readonly string Wisdom;

        [ItemMetadata("Metadata/Items/Currency/CurrencyPortal")]
        public static readonly string Portal;

        [ItemMetadata("Metadata/Items/Currency/CurrencyUpgradeToMagic")]
        public static readonly string Transmutation;

        [ItemMetadata("Metadata/Items/Currency/CurrencyAddModToMagic")]
        public static readonly string Augmentation;

        [ItemMetadata("Metadata/Items/Currency/CurrencyRerollMagic")]
        public static readonly string Alteration;

        [ItemMetadata("Metadata/Items/Currency/CurrencyArmourQuality")]
        public static readonly string Scrap;

        [ItemMetadata("Metadata/Items/Currency/CurrencyWeaponQuality")]
        public static readonly string Whetstone;

        [ItemMetadata("Metadata/Items/Currency/CurrencyFlaskQuality")]
        public static readonly string Glassblower;

        [ItemMetadata("Metadata/Items/Currency/CurrencyMapQuality")]
        public static readonly string Chisel;

        [ItemMetadata("Metadata/Items/Currency/CurrencyRerollSocketColours")]
        public static readonly string Chromatic;

        [ItemMetadata("Metadata/Items/Currency/CurrencyUpgradeRandomly")]
        public static readonly string Chance;

        [ItemMetadata("Metadata/Items/Currency/CurrencyUpgradeToRare")]
        public static readonly string Alchemy;

        [ItemMetadata("Metadata/Items/Currency/CurrencyRerollSocketNumbers")]
        public static readonly string Jeweller;

        [ItemMetadata("Metadata/Items/Currency/CurrencyRerollSocketLinks")]
        public static readonly string Fusing;

        [ItemMetadata("Metadata/Items/Currency/CurrencyConvertToNormal")]
        public static readonly string Scouring;

        [ItemMetadata("Metadata/Items/Currency/CurrencyRerollImplicit")]
        public static readonly string Blessed;

        [ItemMetadata("Metadata/Items/Currency/CurrencyUpgradeMagicToRare")]
        public static readonly string Regal;

        [ItemMetadata("Metadata/Items/Currency/CurrencyRerollRare")]
        public static readonly string Chaos;

        [ItemMetadata("Metadata/Items/Currency/CurrencyCorrupt")]
        public static readonly string Vaal;

        [ItemMetadata("Metadata/Items/Currency/CurrencyPassiveRefund")]
        public static readonly string Regret;

        [ItemMetadata("Metadata/Items/Currency/CurrencyGemQuality")]
        public static readonly string Gemcutter;

        [ItemMetadata("Metadata/Items/Currency/CurrencyModValues")]
        public static readonly string Divine;

        [ItemMetadata("Metadata/Items/Currency/CurrencyAddModToRare")]
        public static readonly string Exalted;

        [ItemMetadata("Metadata/Items/Currency/CurrencyImprintOrb")]
        public static readonly string Eternal;

        [ItemMetadata("Metadata/Items/Currency/CurrencyDuplicate")]
        public static readonly string Mirror;

        [ItemMetadata("Metadata/Items/Currency/CurrencyStackedDeck")]
        public static readonly string StackedDeck;

        // Veiled Currency
        [ItemMetadata("Metadata/Items/Currency/CurrencyRerollRareVeiledChaos")]
        public static readonly string VeiledChaos;

        [ItemMetadata("Metadata/Items/Currency/CurrencyRerollRareVeiled")]
        public static readonly string VeiledExaltedOrb;

        // Ritual Currency
        [ItemMetadata("Metadata/Items/Currency/CurrencyRitualStone")]
        public static readonly string RitualVessel;

        // League Specific Currency - Perandus
        [ItemMetadata("Metadata/Items/Currency/CurrencyPerandusCoin")]
        public static readonly string PerandusCoin;

        [ItemMetadata("Metadata/Items/Currency/CurrencySilverCoin")]
        public static readonly string SilverCoin;

        [ItemMetadata("Metadata/Items/Currency/CurrencyItemisedProphecy")]
        public static readonly string Prophecy;

        // Atlas Currency
        [ItemMetadata("Metadata/Items/Currency/CurrencyAddAtlasMod")]
        public static readonly string SextantApprentice;

        [ItemMetadata("Metadata/Items/Currency/CurrencyAddAtlasModMid")]
        public static readonly string SextantJourneyman;

        [ItemMetadata("Metadata/Items/Currency/CurrencyAddAtlasModHigh")]
        public static readonly string SextantMaster;

        [ItemMetadata("Metadata/Items/Currency/CurrencySealMapLow")]
        public static readonly string SealApprentice;

        [ItemMetadata("Metadata/Items/Currency/CurrencySealMapMid")]
        public static readonly string SealJourneyman;

        [ItemMetadata("Metadata/Items/Currency/CurrencySealMapHigh")]
        public static readonly string SealMaster;

        [ItemMetadata("Metadata/Items/Currency/CurrencyRespecShapersOrb")]
        public static readonly string Unshaping;

        // Breach Currency
        [ItemMetadata("Metadata/Items/Currency/CurrencyBreachFireShard")]
        public static readonly string SplinterXoph;

        [ItemMetadata("Metadata/Items/Currency/CurrencyBreachColdShard")]
        public static readonly string SplinterTul;

        [ItemMetadata("Metadata/Items/Currency/CurrencyBreachLightningShard")]
        public static readonly string SplinterEsh;

        [ItemMetadata("Metadata/Items/Currency/CurrencyBreachPhysicalShard")]
        public static readonly string SplinterUulNetol;

        [ItemMetadata("Metadata/Items/Currency/CurrencyBreachChaosShard")]
        public static readonly string SplinterChayula;

        [ItemMetadata("Metadata/Items/Currency/CurrencyBreachUpgradeUniqueFire")]
        public static readonly string BlessingXoph;

        [ItemMetadata("Metadata/Items/Currency/CurrencyBreachUpgradeUniqueCold")]
        public static readonly string BlessingTul;

        [ItemMetadata("Metadata/Items/Currency/CurrencyBreachUpgradeUniqueLightning")]
        public static readonly string BlessingEsh;

        [ItemMetadata("Metadata/Items/Currency/CurrencyBreachUpgradeUniquePhysical")]
        public static readonly string BlessingUulNetol;

        [ItemMetadata("Metadata/Items/Currency/CurrencyBreachUpgradeUniqueChaos")]
        public static readonly string BlessingChayula;

        // Harbinger Currency
        [ItemMetadata("Metadata/Items/Currency/CurrencyRemoveMod")]
        public static readonly string Annulment;

        [ItemMetadata("Metadata/Items/Currency/CurrencyUpgradeToRareAndSetSockets")]
        public static readonly string Binding;

        [ItemMetadata("Metadata/Items/Currency/CurrencyRerollMapType")]
        public static readonly string Horizon;

        [ItemMetadata("Metadata/Items/Currency/CurrencyUpgradeMapTier")]
        public static readonly string Harbinger;

        [ItemMetadata("Metadata/Items/Currency/CurrencyStrongboxQuality")]
        public static readonly string Engineer;

        [ItemMetadata("Metadata/Items/Currency/CurrencyRerollUnique")]
        public static readonly string Ancient;

        // Influence Currency (Atlas Exiles)
        [ItemMetadata("Metadata/Items/AtlasExiles/AddModToRareCrusader")]
        public static readonly string CrusadersExaltedOrb;

        [ItemMetadata("Metadata/Items/AtlasExiles/AddModToRareWarlord")]
        public static readonly string WarlordsExaltedOrb;

        [ItemMetadata("Metadata/Items/AtlasExiles/AddModToRareRedeemer")]
        public static readonly string RedeemersExaltedOrb;

        [ItemMetadata("Metadata/Items/AtlasExiles/AddModToRareHunter")]
        public static readonly string HuntersExaltedOrb;

        [ItemMetadata("Metadata/Items/AtlasExiles/AddModToRareElder")]
        public static readonly string EldersExaltedOrb;

        [ItemMetadata("Metadata/Items/AtlasExiles/ApplyInfluence")]
        public static readonly string AwakenersOrb;

        // Maven Currency
        [ItemMetadata("Metadata/Items/Currency/MavenChisel1")]
        public static readonly string MavenChiselProcurement;

        [ItemMetadata("Metadata/Items/Currency/MavenChisel2")]
        public static readonly string MavenChiselProliferation;

        [ItemMetadata("Metadata/Items/Currency/MavenChisel3")]
        public static readonly string MavenChiselScarabs;

        [ItemMetadata("Metadata/Items/Currency/MavenChisel4")]
        public static readonly string MavenChiselAvarice;

        [ItemMetadata("Metadata/Items/Currency/MavenChisel5")]
        public static readonly string MavenChiselDivination;

        [ItemMetadata("Metadata/Items/Currency/MavenOrb")]
        public static readonly string OrbOfDominance;

        // Atlas Pinnacle Currency
        [ItemMetadata("Metadata/Items/Currency/SecretsoftheAtlas/ShaperExaltedOrb")]
        public static readonly string ShapersExaltedOrb;

        [ItemMetadata("Metadata/Items/Currency/SecretsoftheAtlas/ZanaPinnacleOrb1")]
        public static readonly string OrbOfRemembrance;

        [ItemMetadata("Metadata/Items/Currency/SecretsoftheAtlas/ZanaPinnacleOrb2")]
        public static readonly string OrbOfUnravelling;

        [ItemMetadata("Metadata/Items/Currency/SecretsoftheAtlas/ZanaPinnacleOrb3")]
        public static readonly string OrbOfIntention;

        // Regrading Lenses
        [ItemMetadata("Metadata/Items/Currency/AlternateSkillGemCurrency")]
        public static readonly string PrimeRegradingLens;

        [ItemMetadata("Metadata/Items/Currency/AlternateSupportGemCurrency")]
        public static readonly string SecondaryRegradingLens;

        // Enchant Currency
        [ItemMetadata("Metadata/Items/Currency/DivineEnchantBodyArmourCurrency")]
        public static readonly string TemperingOrb;

        [ItemMetadata("Metadata/Items/Currency/DivineEnchantWeaponCurrency")]
        public static readonly string TailoringOrb;

        // Binding & Engineer
        [ItemMetadata("Metadata/Items/Currency/BindingOrb")]
        public static readonly string OrbOfBinding;

        [ItemMetadata("Metadata/Items/Currency/InfusedEngineersOrb")]
        public static readonly string InfusedEngineersOrb;

        // Fracturing
        [ItemMetadata("Metadata/Items/Currency/FracturingOrbCombined")]
        public static readonly string FracturingOrb;

        // Eldritch Currency
        [ItemMetadata("Metadata/Items/Currency/CleansingFireOrbRank1")]
        public static readonly string LesserEldritchEmber;

        [ItemMetadata("Metadata/Items/Currency/CleansingFireOrbRank2")]
        public static readonly string GreaterEldritchEmber;

        [ItemMetadata("Metadata/Items/Currency/CleansingFireOrbRank3")]
        public static readonly string GrandEldritchEmber;

        [ItemMetadata("Metadata/Items/Currency/CleansingFireOrbRank4")]
        public static readonly string ExceptionalEldritchEmber;

        [ItemMetadata("Metadata/Items/Currency/TangleOrbRank1")]
        public static readonly string LesserEldritchIchor;

        [ItemMetadata("Metadata/Items/Currency/TangleOrbRank2")]
        public static readonly string GreaterEldritchIchor;

        [ItemMetadata("Metadata/Items/Currency/TangleOrbRank3")]
        public static readonly string GrandEldritchIchor;

        [ItemMetadata("Metadata/Items/Currency/TangleOrbRank4")]
        public static readonly string ExceptionalEldritchIchor;

        [ItemMetadata("Metadata/Items/Currency/ConflictOrbRank1")]
        public static readonly string OrbOfConflict;

        [ItemMetadata("Metadata/Items/Currency/EldritchChaosOrb")]
        public static readonly string EldritchChaosOrb;

        [ItemMetadata("Metadata/Items/Currency/EldritchExaltedOrb")]
        public static readonly string EldritchExaltedOrb;

        [ItemMetadata("Metadata/Items/Currency/EldritchAnnulmentOrb")]
        public static readonly string EldritchOrbOfAnnulment;

        // Heist Currency
        [ItemMetadata("Metadata/Items/Currency/Heist/HeistCoinCurrency")]
        public static readonly string RoguesMarker;

        // Expedition Currency
        [ItemMetadata("Metadata/Items/Currency/Expedition/FlaskPlate")]
        public static readonly string EnkindlingOrb;

        [ItemMetadata("Metadata/Items/Currency/Expedition/FlaskInjector")]
        public static readonly string InstillingOrb;

        // Tainted Currency (Scourge)
        [ItemMetadata("Metadata/Items/Currency/Hellscape/HellscapeChromaticOrb")]
        public static readonly string TaintedChromaticOrb;

        [ItemMetadata("Metadata/Items/Currency/Hellscape/HellscapeOrbOfFusing")]
        public static readonly string TaintedOrbOfFusing;

        [ItemMetadata("Metadata/Items/Currency/Hellscape/HellscapeChaosOrb")]
        public static readonly string TaintedChaosOrb;

        [ItemMetadata("Metadata/Items/Currency/Hellscape/HellscapeExaltedOrb")]
        public static readonly string TaintedExaltedOrb;

        [ItemMetadata("Metadata/Items/Currency/Hellscape/HellscapeMythicOrb")]
        public static readonly string TaintedMythicOrb;

        [ItemMetadata("Metadata/Items/Currency/Hellscape/HellscapeTeardropOrb")]
        public static readonly string TaintedDivineTeardrop;

        // Scouting Reports
        [ItemMetadata("Metadata/Items/Currency/ScoutingReport")]
        public static readonly string ScoutingReport;

        // Oil Extractor
        [ItemMetadata("Metadata/Items/Currency/Oils/IchorExtractor")]
        public static readonly string OilExtractor;

        // Sentinel Currency
        [ItemMetadata("Metadata/Items/Currency/Sentinel/PowerCore")]
        public static readonly string PowerCore;

        [ItemMetadata("Metadata/Items/Currency/Sentinel/TransformingPowerCore")]
        public static readonly string TransformingPowerCore;

        [ItemMetadata("Metadata/Items/Currency/Sentinel/AmplifyingPowerCore")]
        public static readonly string AmplifyingPowerCore;

        [ItemMetadata("Metadata/Items/Currency/Sentinel/AugmentingPowerCore")]
        public static readonly string AugmentingPowerCore;

        [ItemMetadata("Metadata/Items/Currency/Sentinel/ArmourRecombinator")]
        public static readonly string ArmourRecombinator;

        [ItemMetadata("Metadata/Items/Currency/Sentinel/WeaponRecombinator")]
        public static readonly string WeaponRecombinator;

        [ItemMetadata("Metadata/Items/Currency/Sentinel/JewelleryRecombinator")]
        public static readonly string JewelleryRecombinator;

        // Sanctum Currency
        [ItemMetadata("Metadata/Items/Currency/Sanctum/LyciaCorruptCurrency")]
        public static readonly string LyciasInvocation;

        // Ancestors Currency
        [ItemMetadata("Metadata/Items/Currency/Ancestors/NavaliCoin")]
        public static readonly string SilverCoinAncestors;

        // Affliction Currency
        [ItemMetadata("Metadata/Items/Currency/CurrencyAfflictionOrbTrinkets")]
        public static readonly string DeliriumJeweller;

        [ItemMetadata("Metadata/Items/Currency/HinekorasLock")]
        public static readonly string HinekorasLock;

        // Reflecting Mist
        [ItemMetadata("Metadata/Items/Currency/ReflectiveMist")]
        public static readonly string ReflectingMist;

        [ItemMetadata("Metadata/Items/Currency/ReflectiveMist")]
        public static readonly string FadingReflectingMist;

        // Gemcutter's Lens
        [ItemMetadata("Metadata/Items/Currency/GemcuttersLense")]
        public static readonly string GemcuttersLens;

        // Djinn-Touched Vaal Orb
        [ItemMetadata("Metadata/Items/Currency/SuperVaal")]
        public static readonly string DjinnTouchedVaalOrb;

        // Shards
        [ItemMetadata("Metadata/Items/Currency/CurrencyIdentificationShard")]
        public static readonly string ScrollFragment;

        [ItemMetadata("Metadata/Items/Currency/CurrencyUpgradeToMagicShard")]
        public static readonly string TransmutationShard;

        [ItemMetadata("Metadata/Items/Currency/CurrencyRerollMagicShard")]
        public static readonly string AlterationShard;

        [ItemMetadata("Metadata/Items/Currency/CurrencyUpgradeToRareShard")]
        public static readonly string AlchemyShard;

        [ItemMetadata("Metadata/Items/Currency/CurrencyRemoveModShard")]
        public static readonly string AnnulmentShard;

        [ItemMetadata("Metadata/Items/Currency/CurrencyUpgradeToRareAndSetSocketsShard")]
        public static readonly string BindingShard;

        [ItemMetadata("Metadata/Items/Currency/CurrencyRerollMapTypeShard")]
        public static readonly string HorizonShard;

        [ItemMetadata("Metadata/Items/Currency/CurrencyUpgradeMapTierShard")]
        public static readonly string HarbingerShard;

        [ItemMetadata("Metadata/Items/Currency/CurrencyStrongboxQualityShard")]
        public static readonly string EngineerShard;

        [ItemMetadata("Metadata/Items/Currency/CurrencyRerollUniqueShard")]
        public static readonly string AncientShard;

        [ItemMetadata("Metadata/Items/Currency/CurrencyUpgradeMagicToRareShard")]
        public static readonly string RegalShard;

        [ItemMetadata("Metadata/Items/Currency/CurrencyRerollRareShard")]
        public static readonly string ChaosShard;

        [ItemMetadata("Metadata/Items/Currency/CurrencyAddModToRareShard")]
        public static readonly string ExaltedShard;

        [ItemMetadata("Metadata/Items/Currency/CurrencyDuplicateShard")]
        public static readonly string MirrorShard;

        internal static Dictionary<string, string> ShardToCurrencyDict;

        private static readonly log4net.ILog Log = Logger.GetLoggerInstanceForType();

        static CurrencyNames()
        {
            var fieldDict = new Dictionary<string, FieldInfo>();
            foreach (var field in typeof(CurrencyNames).GetFields())
            {
                var metadataAttribute = field.GetCustomAttribute<ItemMetadata>();
                if (metadataAttribute != null)
                {
                    fieldDict.Add(metadataAttribute.Metadata, field);
                }
            }

            int total = fieldDict.Count;
            int processed = 0;

            foreach (var item in Dat.BaseItemTypes)
            {
                if (processed >= total) break;
                if (fieldDict.TryGetValue(item.Metadata, out FieldInfo field))
                {
                    field.SetValue(null, item.Name);
                    ++processed;
                }
            }

            if (processed < total)
            {
                Log.Error("[CurrencyNames] Update is required. Not all fields were initialized.");
                BotManager.Stop();
            }

            // Updated Shard to Currency Dictionary
            ShardToCurrencyDict = new Dictionary<string, string>
            {
                [ScrollFragment] = Wisdom,
                [TransmutationShard] = Transmutation,
                [AlterationShard] = Alteration,
                [AlchemyShard] = Alchemy,
                [AnnulmentShard] = Annulment,
                [BindingShard] = Binding,
                [HorizonShard] = Horizon,
                [HarbingerShard] = Harbinger,
                [EngineerShard] = Engineer,
                [AncientShard] = Ancient,
                [RegalShard] = Regal,
                [ChaosShard] = Chaos,
                [ExaltedShard] = Exalted,
                [MirrorShard] = Mirror,
                // Breach splinters
                [SplinterXoph] = BlessingXoph,
                [SplinterTul] = BlessingTul,
                [SplinterEsh] = BlessingEsh,
                [SplinterUulNetol] = BlessingUulNetol,
                [SplinterChayula] = BlessingChayula
            };
        }

        /// <summary>
        /// Gets the full currency name from a shard name
        /// </summary>
        /// <param name="shardName">The name of the shard</param>
        /// <returns>The name of the full currency, or null if not found</returns>
        public static string GetFullCurrencyFromShard(string shardName)
        {
            return ShardToCurrencyDict.TryGetValue(shardName, out string fullCurrency) ? fullCurrency : null;
        }

        /// <summary>
        /// Checks if a currency item is a shard
        /// </summary>
        /// <param name="currencyName">The name of the currency</param>
        /// <returns>True if the currency is a shard</returns>
        public static bool IsShard(string currencyName)
        {
            return ShardToCurrencyDict.ContainsKey(currencyName);
        }

        /// <summary>
        /// Gets all available currency names
        /// </summary>
        /// <returns>List of all currency names</returns>
        public static List<string> GetAllCurrencyNames()
        {
            var currencies = new List<string>();
            foreach (var field in typeof(CurrencyNames).GetFields())
            {
                if (field.IsStatic && field.FieldType == typeof(string))
                {
                    var value = (string)field.GetValue(null);
                    if (!string.IsNullOrEmpty(value))
                    {
                        currencies.Add(value);
                    }
                }
            }
            return currencies;
        }

        [AttributeUsage(AttributeTargets.Field)]
        public class ItemMetadata : Attribute
        {
            public ItemMetadata(string metadata)
            {
                Metadata = metadata;
            }

            public string Metadata { get; set; }
        }
    }
}