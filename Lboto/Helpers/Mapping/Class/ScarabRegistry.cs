using DreamPoeBot.Loki.Game;
using System;
using System.Collections.Generic;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;
using DreamPoeBot.Loki.Game.Objects;

namespace Lboto.Helpers.Mapping
{
    public static class ScarabRegistry
    {
        private static readonly List<ScarabInfo> _scarabs = new List<ScarabInfo>
        {
            new ScarabInfo("Abyss Scarab", "Abyss", 2, () => StashUi.FragmentTab.Scarab.AbyssScarab),
            new ScarabInfo("Abyss Scarab of Edifice", "Abyss", 1, () => StashUi.FragmentTab.Scarab.AbyssScarabofEdifice),
            new ScarabInfo("Abyss Scarab of Multitudes", "Abyss", 2, () => StashUi.FragmentTab.Scarab.AbyssScarabofMultitudes),
            new ScarabInfo("Abyss Scarab of Profound Depth", "Abyss", 1, () => StashUi.FragmentTab.Scarab.AbyssScarabofProfoundDepth),
            new ScarabInfo("Ambush Scarab", "Ambush", 3, () => StashUi.FragmentTab.Scarab.AmbushScarab),
            new ScarabInfo("Ambush Scarab of Containment", "Ambush", 1, () => StashUi.FragmentTab.Scarab.AmbushScarabofContainment),
            new ScarabInfo("Ambush Scarab of Discernment", "Ambush", 1, () => StashUi.FragmentTab.Scarab.AmbushScarabofDiscernment),
            new ScarabInfo("Ambush Scarab of Hidden Compartments", "Ambush", 1, () => StashUi.FragmentTab.Scarab.AmbushScarabofHiddenCompartments),
            new ScarabInfo("Ambush Scarab of Potency", "Ambush", 1, () => StashUi.FragmentTab.Scarab.AmbushScarabofPotency),
            new ScarabInfo("Anarchy Scarab", "Anarchy", 5, () => StashUi.FragmentTab.Scarab.AnarchyScarab),
            new ScarabInfo("Anarchy Scarab of Gigantification", "Anarchy", 2, () => StashUi.FragmentTab.Scarab.AnarchyScarabofGigantification),
            new ScarabInfo("Anarchy Scarab of Partnership", "Anarchy", 1, () => StashUi.FragmentTab.Scarab.AnarchyScarabofPartnership),
            new ScarabInfo("Bestiary Scarab", "Bestiary", 1, () => StashUi.FragmentTab.Scarab.BestiaryScarab),
            new ScarabInfo("Bestiary Scarab of Duplicating", "Bestiary", 1, () => StashUi.FragmentTab.Scarab.BestiaryScarabofDuplicating),
            new ScarabInfo("Bestiary Scarab of the Herd", "Bestiary", 2, () => StashUi.FragmentTab.Scarab.BestiaryScaraboftheHerd),
            new ScarabInfo("Betrayal Scarab", "Betrayal", 1, () => StashUi.FragmentTab.Scarab.BetrayalScarab),
            new ScarabInfo("Betrayal Scarab of Reinforcements", "Betrayal", 1, () => StashUi.FragmentTab.Scarab.BetrayalScarabofReinforcements),
            new ScarabInfo("Betrayal Scarab of the Allflame", "Betrayal", 1, () => StashUi.FragmentTab.Scarab.BetrayalScaraboftheAllflame),
            new ScarabInfo("Beyond Scarab", "Beyond", 1, () => StashUi.FragmentTab.Scarab.BeyondScarab),
            new ScarabInfo("Beyond Scarab of Haemophilia", "Beyond", 2, () => StashUi.FragmentTab.Scarab.BeyondScarabofHaemophilia),
            new ScarabInfo("Beyond Scarab of Resurgence", "Beyond", 1, () => StashUi.FragmentTab.Scarab.BeyondScarabofResurgence),
            new ScarabInfo("Beyond Scarab of the Invasion", "Beyond", 1, () => StashUi.FragmentTab.Scarab.BeyondScaraboftheInvasion),
            new ScarabInfo("Blight Scarab", "Blight", 1, () => StashUi.FragmentTab.Scarab.BlightScarab),
            new ScarabInfo("Blight Scarab of Blooming", "Blight", 1, () => StashUi.FragmentTab.Scarab.BlightScarabofBlooming),
            new ScarabInfo("Blight Scarab of Invigoration", "Blight", 1, () => StashUi.FragmentTab.Scarab.BlightScarabofInvigoration),
            new ScarabInfo("Blight Scarab of the Blightheart", "Blight", 1, () => StashUi.FragmentTab.Scarab.BlightScaraboftheBlightheart),
            new ScarabInfo("Breach Scarab", "Breach", 5, () => StashUi.FragmentTab.Scarab.BreachScarab),
            new ScarabInfo("Breach Scarab of Lordship", "Breach", 1, () => StashUi.FragmentTab.Scarab.BreachScarabofLordship),
            new ScarabInfo("Breach Scarab of Resonant Cascade", "Breach", 1, () => StashUi.FragmentTab.Scarab.BreachScarabofResonantCascade),
            new ScarabInfo("Breach Scarab of Snares", "Breach", 1, () => StashUi.FragmentTab.Scarab.BreachScarabofSnares),
            new ScarabInfo("Breach Scarab of Splintering", "Breach", 2, () => StashUi.FragmentTab.Scarab.BreachScarabofSplintering),
            new ScarabInfo("Cartography Scarab of Corruption", "Cartography", 1, () => StashUi.FragmentTab.Scarab.CartographyScarabofCorruption),
            new ScarabInfo("Cartography Scarab of Escalation", "Cartography", 1, () => StashUi.FragmentTab.Scarab.CartographyScarabofEscalation),
            new ScarabInfo("Cartography Scarab of Risk", "Cartography", 5, () => StashUi.FragmentTab.Scarab.CartographyScarabofRisk),
            new ScarabInfo("Cartography Scarab of the Multitude", "Cartography", 3, () => StashUi.FragmentTab.Scarab.CartographyScaraboftheMultitude),
            new ScarabInfo("Delirium Scarab", "Delirium", 1, () => StashUi.FragmentTab.Scarab.DeliriumScarab),
            new ScarabInfo("Delirium Scarab of Delusions", "Delirium", 1, () => StashUi.FragmentTab.Scarab.DeliriumScarabofDelusions),
            new ScarabInfo("Delirium Scarab of Mania", "Delirium", 2, () => StashUi.FragmentTab.Scarab.DeliriumScarabofMania),
            new ScarabInfo("Delirium Scarab of Neuroses", "Delirium", 1, () => StashUi.FragmentTab.Scarab.DeliriumScarabofNeuroses),
            new ScarabInfo("Delirium Scarab of Paranoia", "Delirium", 5, () => StashUi.FragmentTab.Scarab.DeliriumScarabofParanoia),
            new ScarabInfo("Divination Scarab of Pilfering", "Divination", 1, () => StashUi.FragmentTab.Scarab.DivinationScarabofPilfering),
            new ScarabInfo("Divination Scarab of Plenty", "Divination", 5, () => StashUi.FragmentTab.Scarab.DivinationScarabofPlenty),
            new ScarabInfo("Divination Scarab of The Cloister", "Divination", 5, () => StashUi.FragmentTab.Scarab.DivinationScarabofTheCloister),
            new ScarabInfo("Domination Scarab", "Domination", 4, () => StashUi.FragmentTab.Scarab.DominationScarab),
            new ScarabInfo("Domination Scarab of Apparitions", "Domination", 1, () => StashUi.FragmentTab.Scarab.DominationScarabofApparitions),
            new ScarabInfo("Domination Scarab of Evolution", "Domination", 2, () => StashUi.FragmentTab.Scarab.DominationScarabofEvolution),
            new ScarabInfo("Domination Scarab of Terrors", "Domination", 1, () => StashUi.FragmentTab.Scarab.DominationScarabofTerrors),
            new ScarabInfo("Essence Scarab", "Essence", 5, () => StashUi.FragmentTab.Scarab.EssenceScarab),
            new ScarabInfo("Essence Scarab of Adaptation", "Essence", 1, () => StashUi.FragmentTab.Scarab.EssenceScarabofAdaptation),
            new ScarabInfo("Essence Scarab of Ascent", "Essence", 1, () => StashUi.FragmentTab.Scarab.EssenceScarabofAscent),
            new ScarabInfo("Essence Scarab of Calcification", "Essence", 1, () => StashUi.FragmentTab.Scarab.EssenceScarabofCalcification),
            new ScarabInfo("Essence Scarab of Stability", "Essence", 1, () => StashUi.FragmentTab.Scarab.EssenceScarabofStability),
            new ScarabInfo("Expedition Scarab", "Expedition", 1, () => StashUi.FragmentTab.Scarab.ExpeditionScarab),
            new ScarabInfo("Expedition Scarab of Archaeology", "Expedition", 1, () => StashUi.FragmentTab.Scarab.ExpeditionScarabofArchaeology),
            new ScarabInfo("Expedition Scarab of Runefinding", "Expedition", 2, () => StashUi.FragmentTab.Scarab.ExpeditionScarabofRunefinding),
            new ScarabInfo("Expedition Scarab of Verisium Powder", "Expedition", 1, () => StashUi.FragmentTab.Scarab.ExpeditionScarabofVerisiumPowder),
            new ScarabInfo("Harbinger Scarab", "Harbinger", 4, () => StashUi.FragmentTab.Scarab.HarbingerScarab),
            new ScarabInfo("Harbinger Scarab of Obelisks", "Harbinger", 1, () => StashUi.FragmentTab.Scarab.HarbingerScarabofObelisks),
            new ScarabInfo("Harbinger Scarab of Regency", "Harbinger", 1, () => StashUi.FragmentTab.Scarab.HarbingerScarabofRegency),
            new ScarabInfo("Harbinger Scarab of Warhoards", "Harbinger", 1, () => StashUi.FragmentTab.Scarab.HarbingerScarabofWarhoards),
            new ScarabInfo("Harvest Scarab", "Harvest", 1, () => StashUi.FragmentTab.Scarab.HarvestScarab),
            new ScarabInfo("Harvest Scarab of Cornucopia", "Harvest", 1, () => StashUi.FragmentTab.Scarab.HarvestScarabofCornucopia),
            new ScarabInfo("Harvest Scarab of Doubling", "Harvest", 1, () => StashUi.FragmentTab.Scarab.HarvestScarabofDoubling),
            new ScarabInfo("Horned Scarab of Awakening", "Horned", 1, () => StashUi.FragmentTab.Scarab.HornedScarabofAwakening),
            new ScarabInfo("Horned Scarab of Bloodlines", "Horned", 1, () => StashUi.FragmentTab.Scarab.HornedScarabofBloodlines),
            new ScarabInfo("Horned Scarab of Glittering", "Horned", 1, () => StashUi.FragmentTab.Scarab.HornedScarabofGlittering),
            new ScarabInfo("Horned Scarab of Nemeses", "Horned", 1, () => StashUi.FragmentTab.Scarab.HornedScarabofNemeses),
            new ScarabInfo("Horned Scarab of Pandemonium", "Horned", 1, () => StashUi.FragmentTab.Scarab.HornedScarabofPandemonium),
            new ScarabInfo("Horned Scarab of Preservation", "Horned", 1, () => StashUi.FragmentTab.Scarab.HornedScarabofPreservation),
            new ScarabInfo("Horned Scarab of Tradition", "Horned", 1, () => StashUi.FragmentTab.Scarab.HornedScarabofTradition),
            new ScarabInfo("Incursion Scarab", "Incursion", 1, () => StashUi.FragmentTab.Scarab.IncursionScarab),
            new ScarabInfo("Incursion Scarab of Champions", "Incursion", 2, () => StashUi.FragmentTab.Scarab.IncursionScarabofChampions),
            new ScarabInfo("Incursion Scarab of Invasion", "Incursion", 3, () => StashUi.FragmentTab.Scarab.IncursionScarabofInvasion),
            new ScarabInfo("Incursion Scarab of Timelines", "Incursion", 1, () => StashUi.FragmentTab.Scarab.IncursionScarabofTimelines),
            new ScarabInfo("Influencing Scarab of Conversion", "Influencing", 1, () => StashUi.FragmentTab.Scarab.InfluencingScarabofConversion),
            new ScarabInfo("Influencing Scarab of Hordes", "Influencing", 1, () => StashUi.FragmentTab.Scarab.InfluencingScarabofHordes),
            new ScarabInfo("Influencing Scarab of the Elder", "Influencing", 1, () => StashUi.FragmentTab.Scarab.InfluencingScaraboftheElder),
            new ScarabInfo("Influencing Scarab of the Shaper", "Influencing", 1, () => StashUi.FragmentTab.Scarab.InfluencingScaraboftheShaper),
            new ScarabInfo("Kalguuran Scarab", "Kalguuran", 2, () => StashUi.FragmentTab.Scarab.KalguuranScarab),
            new ScarabInfo("Kalguuran Scarab of Guarded Riches", "Kalguuran", 1, () => StashUi.FragmentTab.Scarab.KalguuranScarabofGuardedRiches),
            new ScarabInfo("Kalguuran Scarab of Refinement", "Kalguuran", 1, () => StashUi.FragmentTab.Scarab.KalguuranScarabofRefinement),
            new ScarabInfo("Legion Scarab", "Legion", 5, () => StashUi.FragmentTab.Scarab.LegionScarab),
            new ScarabInfo("Legion Scarab of Command", "Legion", 1, () => StashUi.FragmentTab.Scarab.LegionScarabofCommand),
            new ScarabInfo("Legion Scarab of Eternal Conflict", "Legion", 1, () => StashUi.FragmentTab.Scarab.LegionScarabofEternalConflict),
            new ScarabInfo("Legion Scarab of Officers", "Legion", 1, () => StashUi.FragmentTab.Scarab.LegionScarabofOfficers),
            new ScarabInfo("Ritual Scarab of Abundance", "Ritual", 2, () => StashUi.FragmentTab.Scarab.RitualScarabofAbundance),
            new ScarabInfo("Ritual Scarab of Selectiveness", "Ritual", 2, () => StashUi.FragmentTab.Scarab.RitualScarabofSelectiveness),
            new ScarabInfo("Ritual Scarab of Wisps", "Ritual", 1, () => StashUi.FragmentTab.Scarab.RitualScarabofWisps),
            new ScarabInfo("Scarab of Adversaries", "Adversaries", 2, () => StashUi.FragmentTab.Scarab.ScarabofAdversaries),
            new ScarabInfo("Scarab of Bisection", "Bisection", 1, () => StashUi.FragmentTab.Scarab.ScarabofBisection),
            new ScarabInfo("Scarab of Divinity", "Divinity", 2, () => StashUi.FragmentTab.Scarab.ScarabofDivinity),
            new ScarabInfo("Scarab of Hunted Traitors", "HuntedTraitors", 1, () => StashUi.FragmentTab.Scarab.ScarabofHuntedTraitors),
            new ScarabInfo("Scarab of Monstrous Lineage", "MonstrousLineage", 2, () => StashUi.FragmentTab.Scarab.ScarabofMonstrousLineage),
            new ScarabInfo("Scarab of Radiant Storms", "RadiantStorms", 1, () => StashUi.FragmentTab.Scarab.ScarabofRadiantStorms),
            new ScarabInfo("Scarab of Stability", "Stability", 1, () => StashUi.FragmentTab.Scarab.ScarabofStability),
            new ScarabInfo("Scarab of Wisps", "Wisps", 2, () => StashUi.FragmentTab.Scarab.ScarabofWisps),
            new ScarabInfo("Sulphite Scarab", "Sulphite", 1, () => StashUi.FragmentTab.Scarab.SulphiteScarab),
            new ScarabInfo("Sulphite Scarab of Fumes", "Sulphite", 1, () => StashUi.FragmentTab.Scarab.SulphiteScarabofFumes),
            new ScarabInfo("Titanic Scarab", "Titanic", 1, () => StashUi.FragmentTab.Scarab.TitanicScarab),
            new ScarabInfo("Titanic Scarab of Legend", "Titanic", 1, () => StashUi.FragmentTab.Scarab.TitanicScarabofLegend),
            new ScarabInfo("Titanic Scarab of Treasures", "Titanic", 3, () => StashUi.FragmentTab.Scarab.TitanicScarabofTreasures),
            new ScarabInfo("Torment Scarab", "Torment", 2, () => StashUi.FragmentTab.Scarab.TormentScarab),
            new ScarabInfo("Torment Scarab of Peculiarity", "Torment", 1, () => StashUi.FragmentTab.Scarab.TormentScarabofPeculiarity),
            new ScarabInfo("Torment Scarab of Possession", "Torment", 4, () => StashUi.FragmentTab.Scarab.TormentScarabofPossession),
            new ScarabInfo("Ultimatum Scarab", "Ultimatum", 1, () => StashUi.FragmentTab.Scarab.UltimatumScarab),
            new ScarabInfo("Ultimatum Scarab of Bribing", "Ultimatum", 2, () => StashUi.FragmentTab.Scarab.UltimatumScarabofBribing),
            new ScarabInfo("Ultimatum Scarab of Catalysing", "Ultimatum", 1, () => StashUi.FragmentTab.Scarab.UltimatumScarabofCatalysing),
            new ScarabInfo("Ultimatum Scarab of Dueling", "Ultimatum", 1, () => StashUi.FragmentTab.Scarab.UltimatumScarabofDueling),
            new ScarabInfo("Ultimatum Scarab of Inscription", "Ultimatum", 1, () => StashUi.FragmentTab.Scarab.UltimatumScarabofInscription),
        };

        public static IEnumerable<ScarabInfo> All
        {
            get { return _scarabs; }
        }

        public static ScarabInfo GetByName(string name)
        {
            for (int i = 0; i < _scarabs.Count; i++)
            {
                if (string.Equals(_scarabs[i].Name, name, StringComparison.OrdinalIgnoreCase))
                    return _scarabs[i];
            }
            return null;
        }

        public static string GetTypeByName(string name)
        {
            var scarab = GetByName(name);
            return scarab != null ? scarab.Type : "Unknown";
        }

        public static bool CanAdd(Item fragment, IEnumerable<Item> currentItems)
        {
            var scarab = GetByName(fragment.FullName);
            if (scarab == null)
                return true;

            int count = 0;
            foreach (var item in currentItems)
            {
                if (string.Equals(GetTypeByName(item.FullName), scarab.Name, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count < scarab.TypeLimit;
        }

        public static Func<InventoryControlWrapper> GetControl(string name)
        {
            var scarab = GetByName(name);
            return scarab != null ? scarab.Control : null;
        }
    }


}
