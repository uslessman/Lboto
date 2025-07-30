
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Game.Objects;
using DreamPoeBot.Loki.RemoteMemoryObjects;
using Lboto.Helpers;
using Lboto.Helpers.CachedObjects;
using Lboto.Helpers.Global;
using Lboto.Helpers.Positions;
using Lboto.Helpers.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DreamPoeBot.Loki.Game.LokiPoe;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState.GuildStashUi.FragmentTab;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState.StashUi;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState.StashUi.MapsTab;
using Cursor = DreamPoeBot.Loki.Game.LokiPoe.InGameState.CursorItemOverlay;
using InventoryUi = DreamPoeBot.Loki.Game.LokiPoe.InGameState.InventoryUi;
using StashUi = DreamPoeBot.Loki.Game.LokiPoe.InGameState.StashUi;
using Utility = DreamPoeBot.Loki.Bot.Utility;


namespace Lboto.Helpers
{
    public static class Inventories
    {
        private static readonly log4net.ILog Log = Logger.GetLoggerInstanceForType();

        public static List<Item> InventoryItems => LokiPoe.InstanceInfo.GetPlayerInventoryItemsBySlot(InventorySlot.Main);

        public static int AvailableInventorySquares => LokiPoe.InstanceInfo.GetPlayerInventoryBySlot(InventorySlot.Main).AvailableInventorySquares;

        private static Dictionary<string, bool> _availableCurrency = new Dictionary<string, bool>();

        public static Dictionary<string, bool> AvailableCurrency => _availableCurrency;


        //public static async Task<bool> OpenStash()
        //{
        //    if (StashUi.IsOpened)
        //        return true;

        //    WalkablePosition stashPos;

        //    if (LokiPoe.CurrentWorldArea?.IsTown == true)
        //    {
        //        stashPos = StaticPositions.GetStashPosByAct();
        //    }
        //    else
        //    {
        //        var stashObj = LokiPoe.ObjectManager.Stash;
        //        if (stashObj == null)
        //        {
        //            Log.Error("[Inventories] Failed to find Stash nearby.");
        //            return false;
        //        }
        //        stashPos = stashObj.WalkablePosition();
        //    }

        //    await PlayerAction.EnableAlwaysHighlight();
        //    await stashPos.ComeAtOnce(35);

        //    if (!await PlayerAction.Interact(LokiPoe.ObjectManager.Stash, () => StashUi.IsOpened && StashUi.StashTabInfo != null, "opening stash"))
        //        return false;

        //    await Wait.SleepSafe(LokiPoe.Random.Next(200, 400));
        //    return true;
        //}

        public static async Task<bool> OpenInventory()
        {
            if (InventoryUi.IsOpened && !LokiPoe.InGameState.PurchaseUi.IsOpened && !LokiPoe.InGameState.SellUi.IsOpened)
                return true;

            await Coroutines.CloseBlockingWindows();

            Log.Debug("[Inventories] Opening inventory panel.");
            LokiPoe.Input.SimulateKeyEvent(LokiPoe.Input.Binding.open_inventory_panel, true, false, false);

            if (!await Wait.For(() => InventoryUi.IsOpened, "opening inventory"))
            {
                Log.Error("[Inventories] Failed to open inventory panel.");
                return false;
            }

            await Wait.ArtificialDelay();
            return true;
        }

        #region Extension methods

        public static async Task<bool> PlaceItemFromCursor(this InventoryControlWrapper inventory, Vector2i pos)
        {
            var cursorItem = Cursor.Item;
            if (cursorItem == null)
            {
                Log.Error("[Inventories] Cursor item is null.");
                return false;
            }

            Log.Debug($"[Inventories] Placing \"{cursorItem.Name}\" from cursor into slot {pos}.");

            if (Cursor.Mode == LokiPoe.InGameState.CursorItemModes.VirtualUse)
            {
                var destItem = inventory.Inventory.FindItemByPos(pos);
                if (destItem == null)
                {
                    Log.Error("[Inventories] Cannot apply item — destination item is null.");
                    return false;
                }

                int destItemId = destItem.LocalId;
                var applied = inventory.ApplyCursorTo(destItem.LocalId);

                if (applied != ApplyCursorResult.None)
                {
                    Log.Error($"[Inventories] Failed to apply item from cursor. Error: {applied}.");
                    return false;
                }

                return await Wait.For(() =>
                {
                    var item = inventory.Inventory.FindItemByPos(pos);
                    return item != null && item.LocalId != destItemId;
                }, "destination item changed after apply");
            }

            // Place item or swap
            int cursorItemId = cursorItem.LocalId;
            var placed = inventory.PlaceCursorInto(pos.X, pos.Y, true);

            if (placed != PlaceCursorIntoResult.None)
            {
                Log.Error($"[Inventories] Failed to place item from cursor. Error: {placed}.");
                return false;
            }

            return await Wait.For(() =>
            {
                var item = Cursor.Item;
                return item == null || item.LocalId != cursorItemId;
            }, "cursor item change after place");
        }
        #endregion
        
        #region Properties

        public static List<Item> StashTabItems => StashUi.InventoryControl.Inventory.Items;

        #endregion

        #region Map Caching

        /// <summary>
        /// Caches map information from configured stash tabs
        /// </summary>
        /// <param name="fullScan">Whether to perform a full scan of all maps</param>
        /// <returns>Cached map tab information</returns>
        public static async Task<CachedMaps.MapTabInfo> CacheMapTabs(bool fullScan = false)
        {
            var mapTiers = Enumerable.Range(1, 18)
                .Where(tier => tier != 17) // Skip tier 17 (unique maps handled separately)
                .OrderBy(_ => Guid.NewGuid())
                .ToArray();

            var cachedMapTiers = new List<CachedMaps.MapTierInfo>();
            var allCachedMaps = new HashSet<CachedMapItem>();

            var mapTabs = GetAllMapTabNames();

            foreach (string tabName in mapTabs)
            {
                await OpenStashTab(tabName, "CacheMapTabs");

                if (!StashUi.StashTabInfo.IsPremiumMap)
                {
                    // Handle regular stash tabs
                    await ProcessRegularMapTab(mapTiers, cachedMapTiers, allCachedMaps);
                }
                else
                {
                    // Handle premium map tabs
                    await ProcessPremiumMapTab(mapTiers, cachedMapTiers, allCachedMaps, fullScan);
                }
            }

            return new CachedMaps.MapTabInfo(cachedMapTiers, allCachedMaps);
        }

        private static List<string> GetAllMapTabNames()
        {
            var mapTabs = LbotoSettings.Instance.MapTabs;
            var simulacrumTabs = LbotoSettings.Instance.SimulacrumTabs;
            var blightedMapTabs = LbotoSettings.Instance.BlightedMapTabs;
            var influencedMapTabs = LbotoSettings.Instance.InfluencedMapTabs;

            return mapTabs.Concat(simulacrumTabs)
                         .Concat(blightedMapTabs)
                         .Concat(influencedMapTabs)
                         .Distinct()
                         .ToList();
        }

        private static async Task ProcessRegularMapTab(int[] mapTiers, List<CachedMaps.MapTierInfo> cachedMapTiers, HashSet<CachedMapItem> allCachedMaps)
        {
            if (StashUi.StashTabInfo.IsPremiumSpecial)
            {
                string errorMessage = $"[CacheMapTabs] {StashUi.TabControl.CurrentTabName} [type:{StashUi.StashTabInfo.TabType}] is not supported for maps. Please remove it from stashing rules.";
                Log.Error(errorMessage);
                BotManager.Stop(new StopReasonData("unsupported_tab", errorMessage, null), false);
                return;
            }

            var mapsInTab = StashTabItems.Where(x => x.IsMap()).ToList();

            foreach (var map in mapsInTab.Select(m => new CachedMapItem(m)))
            {
                allCachedMaps.Add(map);
            }

            foreach (int tier in mapTiers)
            {
                var mapsForTier = mapsInTab.Where(m => m.MapTier == tier).Select(map => new MapInfo
                {
                    Name = map.Name,
                    IsWitnessedByTheMaven = false,
                    MapsCount = mapsInTab.Count(m => m.MapTier == tier)
                }).ToHashSet();

                if (mapsForTier.Any())
                {
                    cachedMapTiers.Add(new CachedMaps.MapTierInfo(tier, mapsForTier));
                }
            }
        }

        private static async Task ProcessPremiumMapTab(int[] mapTiers, List<CachedMaps.MapTierInfo> cachedMapTiers, HashSet<CachedMapItem> allCachedMaps, bool fullScan)
        {
            foreach (int tier in mapTiers)
            {
                int mapCount = MapsTab.GetMapsCountPerTier(tier);
                if (mapCount <= 0) continue;

                var selectTierResult = MapsTab.SelectTier(tier);
                if ((int)selectTierResult > 0) continue;

                var availableMaps = MapsTab.GetMapInfoForVisibleTier.Where(info => info.MapsCount > 0).ToHashSet();

                if (fullScan)
                {
                    await ProcessMapsForFullScan(availableMaps, allCachedMaps);
                }

                Log.Warn($"[CacheMapTabs] Adding info for T{tier} : [{mapCount}]");
                cachedMapTiers.Add(new CachedMaps.MapTierInfo(tier, availableMaps));
            }
        }

        private static async Task ProcessMapsForFullScan(HashSet<MapInfo> availableMaps, HashSet<CachedMapItem> allCachedMaps)
        {
            foreach (var mapInfo in availableMaps)
            {
                var selectMapResult = MapsTab.SelectMap(mapInfo.Name);
                if ((int)selectMapResult > 0)
                {
                    Log.Error($"[CacheMapTabs:{mapInfo.Name}] Error switching maps: {selectMapResult}");
                    await Wait.SleepSafe(100);
                    continue;
                }

                try
                {
                    bool mapsLoaded = await Wait.For(() =>
                    {
                        var inventoryControl = MapsTab.InventoryControl;
                        return inventoryControl?.Inventory?.Items != null;
                    }, "maps to load", 50, 5000);

                    if (!mapsLoaded)
                    {
                        await Wait.SleepSafe(100);
                        continue;
                    }

                    var inventoryMaps = MapsTab.InventoryControl.Inventory.Items;
                    if (inventoryMaps.Count == mapInfo.MapsCount)
                    {
                        foreach (var map in inventoryMaps.Select(item => new CachedMapItem(item)))
                        {
                            allCachedMaps.Add(map);
                        }
                    }
                    else
                    {
                        Log.Error($"Map count mismatch: {inventoryMaps.Count} != {mapInfo.MapsCount}");
                        await Wait.SleepSafe(100);
                    }
                }
                catch
                {
                    await Wait.SleepSafe(100);
                }
            }
        }

        #endregion

        #region Map Retrieval

        /// <summary>
        /// Takes a specific map from stash and places it in inventory
        /// </summary>
        /// <param name="cachedMap">The cached map item to retrieve</param>
        /// <returns>The map item in inventory, or null if failed</returns>
        public static async Task<Item> TakeMapFromStash(CachedMapItem cachedMap)
        {
            if (!await OpenStashTab(cachedMap.StashTab, "TakeMapFromStash"))
                return null;

            Log.Debug($"[TakeMapFromStash] Attempting to take T{cachedMap.MapTier} {cachedMap.Name} from tab: {cachedMap.StashTab}");

            var currentInventoryMapPositions = InventoryItems.Where(x => x.IsMap())
                                                            .Select(item => item.LocationTopLeft)
                                                            .ToList();

            try
            {
                Item retrievedMap;

                if (StashUi.StashTabInfo.IsPremiumMap)
                {
                    retrievedMap = await TakeMapFromPremiumTab(cachedMap);
                }
                else
                {
                    retrievedMap = await TakeMapFromRegularTab(cachedMap);
                }

                if (retrievedMap == null)
                {
                    RemoveMapFromCache(cachedMap);
                    return null;
                }

                // Wait for map to appear in inventory
                await Wait.For(() =>
                    InventoryItems.Count(x => x.IsMap()) > currentInventoryMapPositions.Count,
                    "map appear in inventory");

                var newMapInInventory = InventoryItems.FirstOrDefault(item =>
                    item.IsMap() &&
                    !currentInventoryMapPositions.Contains(item.LocationTopLeft));

                if (newMapInInventory != null)
                {
                    RemoveMapFromCache(cachedMap);
                }

                return newMapInInventory;
            }
            catch
            {
                Log.Error("[TakeMapFromStash] Error occurred. Map cache may be corrupted. Recaching required.");
                InvalidateMapCache();
                return null;
            }
        }

        private static async Task<Item> TakeMapFromPremiumTab(CachedMapItem cachedMap)
        {
            int targetTier = cachedMap.MapTier;
            string targetName = cachedMap.Name;

            // For unique maps, use tier 17 and full name
            if ((int)cachedMap.Rarity == 3)
            {
                targetTier = 17;
                targetName = cachedMap.FullName;
            }

            // For influenced maps, use tier 18 and influence name
            if (cachedMap.IsInfluencedMap())
            {
                targetTier = 18;
                targetName = Mapping.InfluenceHelper.GetInfluenceName(cachedMap);
            }

            var selectTierResult = MapsTab.SelectTier(targetTier);
            if ((int)selectTierResult > 0)
            {
                Log.Error($"[TakeMapFromStash] SelectTier failed: {selectTierResult}");
                return null;
            }

            await Wait.LatencySleep();

            var selectMapResult = MapsTab.SelectMap(targetName);
            if ((int)selectMapResult == 1)
            {
                // Try with full name if first attempt failed
                selectMapResult = MapsTab.SelectMap($"{targetName} {cachedMap.Name}");
                if ((int)selectMapResult > 0)
                {
                    Log.Error($"[TakeMapFromStash] SelectMap [{targetName}] failed: {selectMapResult}");
                    return null;
                }
            }

            await Wait.LatencySleep();

            bool mapInventoryLoaded = await Wait.For(() =>
                MapsTab.InventoryControl?.Inventory?.Items?.Any(m => m.FullName.Equals(cachedMap.FullName)) == true,
                "map inventory to load");

            if (!mapInventoryLoaded)
            {
                Log.Error($"[TakeMapFromStash] Cannot find T{cachedMap.MapTier} {cachedMap.Name} in tab: {cachedMap.StashTab}");
                return null;
            }

            var targetMapItem = MapsTab.InventoryControl.Inventory.Items.FirstOrDefault(item =>
                AreMapStatsEqual(item, cachedMap) && item.FullName.Equals(cachedMap.FullName));

            if (targetMapItem == null || (int)MapsTab.InventoryControl.FastMove(targetMapItem.LocalId, true, true) > 0)
            {
                Log.Error($"[TakeMapFromStash] Failed to retrieve T{cachedMap.MapTier} {cachedMap.Name} from tab: {cachedMap.StashTab}");
                return null;
            }

            return targetMapItem;
        }

        private static async Task<Item> TakeMapFromRegularTab(CachedMapItem cachedMap)
        {
            var targetMapItem = StashUi.InventoryControl.Inventory.Items.FirstOrDefault(item =>
                AreMapStatsEqual(item, cachedMap) && item.FullName.Equals(cachedMap.FullName));

            if (targetMapItem == null)
            {
                return null;
            }

            bool moveSuccessful = await FastMoveFromStashTab(targetMapItem.LocationTopLeft);
            return moveSuccessful ? targetMapItem : null;
        }

        private static void RemoveMapFromCache(CachedMapItem cachedMap)
        {
            CachedMaps.Instance.MapCache.Maps.Remove(cachedMap);
        }

        private static void InvalidateMapCache()
        {
            CachedMaps.Instance.MapCache = null;
            CachedMaps.Cached = false;
        }

        private static bool AreMapStatsEqual(Item item, CachedMapItem cachedMap)
        {
            var itemStats = item.Stats.OrderBy(s => s.Key)
                                     .Select(stat => $"{stat.Key}[{stat.Value}]")
                                     .ToList();
            string itemStatsString = string.Join("|", itemStats).Trim();

            var cachedMapStats = cachedMap.Stats.OrderBy(s => s.Key)
                                               .Select(stat => $"{stat.Key}[{stat.Value}]")
                                               .ToList();
            string cachedMapStatsString = string.Join("|", cachedMapStats).Trim();

            return itemStatsString.Equals(cachedMapStatsString);
        }

        #endregion

        #region Orb Application

        /// <summary>
        /// Applies an orb to a map at the specified position in inventory
        /// </summary>
        /// <param name="targetPosition">Position of the map in inventory</param>
        /// <param name="orbName">Name of the orb to apply</param>
        /// <returns>True if successful, false otherwise</returns>
        public static async Task<bool> ApplyOrb(Vector2i targetPosition, string orbName)
        {
            var orbInInventory = FindOrbInInventory(orbName);

            if (orbInInventory == null)
            {
                orbInInventory = await GetOrbFromStash(orbName);
                if (orbInInventory == null)
                {
                    Log.Warn($"[ApplyOrb] No '{orbName}' available in inventory or stash tabs. Marking as unavailable.");
                    _availableCurrency[orbName] = false;
                    return false;
                }
            }

            // Pick up the orb
            if (!await PickItemToCursor(InventoryUi.InventoryControl_Main, orbInInventory.LocationTopLeft, rightClick: true))
            {
                //ErrorManager.ReportError();
                return false;
            }

            // Apply orb to target map
            if (!await InventoryUi.InventoryControl_Main.PlaceItemFromCursor(targetPosition))
            {
                //ErrorManager.ReportError();
                return false;
            }

            await WaitForCursorToBeEmpty();
            return true;
        }

        private static Item FindOrbInInventory(string orbName)
        {
            // Handle Alchemy/Binding orb preference for white maps
            if (orbName.Equals(CurrencyNames.Alchemy) || orbName.Equals(CurrencyNames.Binding))
            {
                var availableOrbs = InventoryUi.InventoryControl_Main.Inventory.Items
                    .Where(item => item.Name.Equals(CurrencyNames.Alchemy) || item.Name.Equals(CurrencyNames.Binding))
                    .OrderByDescending(item => item.StackCount)
                    .FirstOrDefault();

                return availableOrbs;
            }

            return InventoryUi.InventoryControl_Main.Inventory.Items
                .FirstOrDefault(item => item.Name.Equals(orbName));
        }

        private static async Task<Item> GetOrbFromStash(string orbName)
        {
            if (!await FindTabWithCurrency(orbName))
            {
                return null;
            }

            // Handle currency tab
            if ((int)StashUi.StashTabInfo.TabType == 3)
            {
                return await GetOrbFromCurrencyTab(orbName);
            }

            // Handle regular tab
            return await GetOrbFromRegularTab(orbName);
        }

        private static async Task<Item> GetOrbFromCurrencyTab(string orbName)
        {
            var currencyControl = GetControlWithCurrency(orbName);

            // Handle Alchemy/Binding preference
            if (orbName.Equals(CurrencyNames.Alchemy))
            {
                var bindingAmount = GetControlsWithCurrency(CurrencyNames.Binding)
                    .Sum(control => control.Inventory.Items.Sum(item => item.StackCount));

                if (bindingAmount > 20)
                {
                    orbName = CurrencyNames.Binding;
                    currencyControl = GetControlWithCurrency(orbName);
                }
            }

            if (StashUi.StashTabInfo.IsPublicFlagged)
            {
                if (!await FastMoveCustomTabItem(currencyControl))
                {
                    return null;
                }

                return InventoryUi.InventoryControl_Main.Inventory.Items
                    .FirstOrDefault(item => item.Name.Equals(orbName));
            }
            else
            {
                // Use the currency control's UseItem method with rightClick=true to pick to cursor
                var useResult = currencyControl.UseItem(true); // actuallyUse = true
                if (useResult != UseItemResult.None)
                {
                    Log.Error($"[GetOrbFromCurrencyTab] Failed to pick {orbName} to cursor. Error: {useResult}");
                    return null;
                }

                // Wait for item to appear on cursor
                if (!await Wait.For(() => CursorItemOverlay.Item != null, "item appear under cursor"))
                {
                    return null;
                }

                return currencyControl.CustomTabItem;
            }
        }

        private static async Task<Item> GetOrbFromRegularTab(string orbName)
        {
            var orbItem = StashTabItems.FirstOrDefault(item => item.Name == orbName);
            if (orbItem == null) return null;

            if (!await PickItemToCursor(StashUi.InventoryControl, orbItem.LocationTopLeft, rightClick: true))
            {
                return null;
            }

            return orbItem;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Opens a specific stash tab
        /// </summary>
        private static async Task<bool> OpenStashTab(string tabName, string caller = "")
        {
            if (!await OpenStash()) return false;

            if (tabName.Contains("Remove-only"))
            {
                Log.Debug($"[OpenStashTab] Ignoring remove-only tab [{tabName}]");
                return true;
            }

            var ignoredTabs = LbotoSettings.Instance.IgnoredTabs;
                //.Find(rule => rule.Name == "Tabs to ignore")?.TabList ?? new List<string>();

            if (ignoredTabs.Contains(tabName))
            {
                Log.Warn($"[OpenStashTab] Ignoring tab [{tabName}] - marked as ignored");
                return true;
            }

            if (StashUi.TabControl.CurrentTabName == tabName)
            {
                return true;
            }

            Log.Debug($"[OpenStashTab] Switching to tab \"{tabName}\". [{caller}]");

            int currentInventoryId = StashUi.StashTabInfo.InventoryId;
            bool isSpecialTab = StashUi.StashTabInfo.IsUniqueCollection ||
                               StashUi.StashTabInfo.IsPremiumDivination ||
                               StashUi.StashTabInfo.IsFolder ||
                               StashUi.StashTabInfo.IsPremiumGem ||
                               StashUi.StashTabInfo.IsPremiumFlask;

            try
            {
                var currentTabIndex = StashUi.TabControl.CurrentTabIndex;
                var targetTabIndex = StashUi.TabControl.TabNames.FindIndex(name => name.Equals(tabName));
                bool useKeyboard = Math.Abs(targetTabIndex - currentTabIndex) <= 1 || LbotoSettings.Instance.ForceKeyboardSwitch;

                var switchResult = useKeyboard ?
                    StashUi.TabControl.SwitchToTabKeyboard(tabName) :
                    StashUi.TabControl.SwitchToTabMouse(tabName);

                if ((int)switchResult > 0)
                {
                    Log.Error($"[OpenStashTab] Failed to switch to tab \"{tabName}\". Error: {switchResult}");
                    LbotoSettings.Instance.ForceKeyboardSwitch = true;
                    return false;
                }
            }
            catch (Exception)
            {
                Log.Error("Exception while using mouse. Forcing keyboard switch for this session");
                LbotoSettings.Instance.ForceKeyboardSwitch = true;
            }

            bool tabSwitched = await Wait.For(() =>
                isSpecialTab || StashUi.StashTabInfo.InventoryId != currentInventoryId,
                "stash tab switching");

            if (!tabSwitched) return false;

            bool tabLoaded = await Wait.For(() =>
                isSpecialTab || StashUi.StashTabInfo != null,
                "stash tab loading");

            if (!tabLoaded) return false;

            if (StashUi.StashTabInfo.IsPremiumSpecial)
            {
                return true;
            }

            return await Wait.For(() =>
                StashUi.InventoryControl != null,
                "items loading");
        }

        /// <summary>
        /// Opens the stash
        /// </summary>
        private static async Task<bool> OpenStash()
        {
            if (StashUi.IsOpened) return true;

            var currentArea = World.CurrentArea;
            var stashObject = CachedStash;
            var minimapStash = InstanceInfo.MinimapIcons.FirstOrDefault(icon =>
                icon.MinimapIcon.Name.Equals("StashPlayer"));

            var stashPosition = minimapStash?.LastSeenPosition ?? Vector2i.Zero;

            var walkableStashPosition = new WalkablePosition("EMPTY STASH POS", Vector2i.Zero);

            if (World.CurrentArea.IsTown && stashPosition == Vector2i.Zero)
            {
                Log.Warn("[OpenStash] In town, using default positions.");
                walkableStashPosition = StaticPositions.GetStashPosByAct();
            }

            if (stashPosition != Vector2i.Zero)
            {
                walkableStashPosition = new WalkablePosition("Stash minimap icon", stashPosition, 5);
            }

            if (stashObject != null)
            {
                walkableStashPosition = stashObject.Position;
            }

            if (walkableStashPosition.AsVector == Vector2i.Zero)
            {
                Log.Error("[OpenStash] Cannot find stash nearby.");
                if (currentArea.Id.Contains("Affliction"))
                {
                    Log.Warn("[OpenStash] In simulacrum, exploring.");
                    await CombatAreaCache.Current.Explorer.Execute();
                }
                return false;
            }

            await Coroutines.CloseBlockingWindows();

            if (walkableStashPosition.Distance > 35)
            {
                if (LokiPoe.CurrentWorldArea.IsTown && Wait.StashPauseProbability(30))
                {
                    var randomPosition = WorldPosition.FindRandomPositionForMove(walkableStashPosition);
                    if (randomPosition != null)
                    {
                        await Move.AtOnce(randomPosition, "To Random Location and pause.");
                        await Wait.TownMoveRandomDelay();
                    }
                }

                if (!await walkableStashPosition.TryComeAtOnce())
                {
                    return false;
                }

                await Coroutines.FinishCurrentAction(true);
                await Wait.LatencySleep();
            }

            await Coroutines.FinishCurrentAction(true);
            await PlayerAction.Interact(Stash);

            bool stashOpened = await Wait.For(() =>
                StashUi.IsOpened && StashUi.StashTabInfo != null,
                "stash ui to load", 50, 1500);

            if (!stashOpened)
            {
                if (((Player)LokiPoe.Me).Hideout != null && !currentArea.IsHideoutArea && !currentArea.Id.Contains("Affliction"))
                {
                    Log.Error("[OpenStash] Stash opening failed. Going to hideout!");
                    await TpToHideoutTask.GoToHideout();
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the stash object from cache or game world
        /// </summary>
        private static NetworkObject Stash => ObjectManager.Objects.FirstOrDefault<Stash>();

        /// <summary>
        /// Gets or sets the cached stash object
        /// </summary>
        private static CachedObject CachedStash
        {
            get => CombatAreaCache.Current.Storage["CachedStash"] as CachedObject;
            set => CombatAreaCache.Current.Storage["CachedStash"] = value;
        }

        /// <summary>
        /// Finds a tab containing the specified currency
        /// </summary>
        private static async Task<bool> FindTabWithCurrency(string currencyName)
        {
            var currencyTabs = new List<string>(LbotoSettings.Instance.CurrencyTabs);

            if (currencyTabs.Count > 1 && StashUi.IsOpened)
            {
                string currentTabName = StashUi.StashTabInfo.DisplayName;
                currencyTabs = currencyTabs.OrderByDescending(tab => tab.Contains(currentTabName)).ToList();
            }

            foreach (string tabName in currencyTabs)
            {
                if (!await OpenStashTab(tabName, "FindTabWithCurrency")) continue;

                if (StashUi.StashTabInfo.IsPublicFlagged && !StashUi.StashTabInfo.IsPremiumCurrency)
                {
                    Log.Error($"[FindTabWithCurrency] Tab \"{tabName}\" is public. Cannot use currency from it.");
                    continue;
                }

                int currencyAmount = GetCurrencyAmountInStashTab(currencyName);
                if (currencyAmount > 0)
                {
                    Log.Debug($"[FindTabWithCurrency] Found {currencyAmount} \"{currencyName}\" in \"{tabName}\" tab.");
                    return true;
                }

                Log.Debug($"[FindTabWithCurrency] No \"{currencyName}\" found in \"{tabName}\" tab.");
            }

            return false;
        }

        /// <summary>
        /// Gets the amount of specified currency in current stash tab
        /// </summary>
        private static int GetCurrencyAmountInStashTab(string currencyName)
        {
            int totalAmount = 0;
            var tabType = StashUi.StashTabInfo.TabType;

            switch ((int)tabType)
            {
                case 3: // Currency tab
                    var currencyControl = GetCurrencyControl(currencyName);
                    if (currencyControl?.CustomTabItem != null)
                    {
                        totalAmount += currencyControl.CustomTabItem.StackCount;
                    }

                    totalAmount += CurrencyTab.NonCurrency
                        .Select(control => control.CustomTabItem)
                        .Where(item => item != null && item.Name == currencyName)
                        .Sum(item => item.StackCount);
                    break;

                case 8: // Essence tab
                    totalAmount += EssenceTab.NonEssences
                        .Select(control => control.CustomTabItem)
                        .Where(item => item != null && item.Name == currencyName)
                        .Sum(item => item.StackCount);
                    break;

                case 6: // Divination tab
                case 5: // Map tab
                case 9: // Fragment tab
                    return 0;

                default: // Regular tab
                    totalAmount += StashTabItems
                        .Where(item => item.Name == currencyName)
                        .Sum(item => item.StackCount);
                    break;
            }

            return totalAmount;
        }

        /// <summary>
        /// Gets the first control containing the specified currency
        /// </summary>
        private static InventoryControlWrapper GetControlWithCurrency(string currencyName)
        {
            return GetControlsWithCurrency(currencyName).FirstOrDefault();
        }

        /// <summary>
        /// Gets all controls containing the specified currency
        /// </summary>
        private static List<InventoryControlWrapper> GetControlsWithCurrency(string currencyName)
        {
            var controls = new List<InventoryControlWrapper>();
            var tabType = StashUi.StashTabInfo.TabType;

            switch ((int)tabType)
            {
                case 3: // Currency tab
                    var currencyControl = GetCurrencyControl(currencyName);
                    if (currencyControl?.CustomTabItem != null)
                    {
                        controls.Add(currencyControl);
                    }

                    var nonCurrencyControls = CurrencyTab.NonCurrency
                        .Where(control => control.CustomTabItem?.Name.Equals(currencyName) == true)
                        .ToList();

                    if (nonCurrencyControls.Any())
                    {
                        return nonCurrencyControls;
                    }
                    break;

                case 8: // Essence tab
                    return EssenceTab.All
                        .Where(control => control.CustomTabItem?.Name.Equals(currencyName) == true)
                        .ToList();

                case 9: // Fragment tab
                    var fragmentControls = new List<InventoryControlWrapper>();

                    if (currencyName.ContainsIgnorecase("invitation"))
                    {
                        var targetInventory = EldrichMaven.All.FirstOrDefault(i =>
                            currencyName.ContainsIgnorecase("maven")
                                ? i.HasMavenTabOverride
                                : i.HasEldrichTabOverride);

                        if (targetInventory != null)
                        {
                            fragmentControls.Add(targetInventory);
                        }
                        else
                        {
                            Log.Error("[GetControlsWithCurrency]Cant find inventory for: " + currencyName);
                        }
                    }
                    else if (currencyName.ContainsIgnorecase("scarab"))
                    {
                        fragmentControls.AddRange(FragmentTab.AllScarab
                            .Where(control => control.CustomTabItem?.Name.Equals(currencyName) == true));
                    }
                    else
                    {
                        var fragmentControl = FragmentTab.All.FirstOrDefault(control =>
                            control.CustomTabItem?.Name.Equals(currencyName) == true);
                        if (fragmentControl != null)
                        {
                            fragmentControls.Add(fragmentControl);
                        }
                    }
                    return fragmentControls;
            }

            return controls.OrderBy(control => control.CustomTabItem.StackCount).ToList();
        }

        /// <summary>
        /// Gets currency control from predefined dictionaries
        /// </summary>
        private static InventoryControlWrapper GetCurrencyControl(string currencyName)
        {
            // These dictionaries should be initialized elsewhere in the codebase
            // For now, returning null as we don't have access to the dictionary definitions
            return null;
        }

        /// <summary>
        /// Fast moves an item from stash tab to inventory
        /// </summary>
        private static async Task<bool> FastMoveFromStashTab(Vector2i itemPosition)
        {
            string tabName = StashUi.TabControl.CurrentTabName;
            var item = StashUi.InventoryControl.Inventory.FindItemByPos(itemPosition);

            if (item == null)
            {
                Log.Error($"[FastMoveFromStashTab] Cannot find item at {itemPosition} in \"{tabName}\" tab.");
                return false;
            }

            string itemName = item.FullName;
            int originalStackCount = item.StackCount;
            var cachedItem = new CachedItem(item);

            Log.Debug($"[FastMoveFromStashTab] Fast moving \"{itemName}\" at {itemPosition} from \"{tabName}\" tab.");

            var moveResult = StashUi.InventoryControl.FastMove(item.LocalId, true, true);
            if ((int)moveResult > 0)
            {
                Log.Error($"[FastMoveFromStashTab] Fast move error: \"{moveResult}\".");
                return false;
            }

            bool itemMoved = await Wait.For(() =>
            {
                var currentItem = StashUi.InventoryControl.Inventory.FindItemByPos(itemPosition);
                return currentItem == null || currentItem.StackCount < originalStackCount;
            }, "fast move from stash tab", 30);

            if (!itemMoved)
            {
                Log.Error($"[FastMoveFromStashTab] Fast move timeout for \"{itemName}\" at {itemPosition} in \"{tabName}\" tab.");
                return false;
            }

            Log.Info($"[Events] Item withdrawn ({itemName})");
            Utility.BroadcastMessage(null, "item_withdrawn_event", new object[] { cachedItem });
            return true;
        }

        /// <summary>
        /// Fast moves an item from custom tab to inventory
        /// </summary>
        private static async Task<bool> FastMoveCustomTabItem(InventoryControlWrapper control)
        {
            if (control == null)
            {
                Log.Error("[FastMoveCustomTabItem] Inventory control is null.");
                return false;
            }

            var item = control.CustomTabItem;
            if (item == null)
            {
                Log.Error("[FastMoveCustomTabItem] Inventory control has no item.");
                return false;
            }

            string itemName = item.Name;
            int originalStackCount = item.StackCount;
            string tabName = StashUi.TabControl.CurrentTabName;

            Log.Debug($"[FastMoveCustomTabItem] Fast moving \"{itemName}\" from \"{tabName}\" tab.");

            var moveResult = control.FastMove(true, true, false);
            if ((int)moveResult > 0)
            {
                Log.Error($"[FastMoveCustomTabItem] Fast move error: \"{moveResult}\".");
                return false;
            }

            bool itemMoved = await Wait.For(() =>
            {
                var currentItem = control.CustomTabItem;
                return currentItem == null || currentItem.StackCount < originalStackCount;
            }, "fast move from premium stash tab", 30);

            if (!itemMoved)
            {
                Log.Error($"[FastMoveCustomTabItem] Fast move timeout for \"{itemName}\" in \"{tabName}\" tab.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Picks an item to cursor from inventory control
        /// </summary>
        private static async Task<bool> PickItemToCursor(InventoryControlWrapper inventory, Vector2i itemPosition, bool rightClick = false)
        {
            var item = inventory.Inventory.FindItemByPos(itemPosition);
            if (item == null)
            {
                Log.Error($"[PickItemToCursor] Cannot find item at {itemPosition}");
                return false;
            }

            int itemId = item.LocalId;

            if (rightClick)
            {
                var useResult = inventory.UseItem(itemId, true);
                if (useResult != UseItemResult.None)
                {
                    Log.Error($"[PickItemToCursor] Failed to pick item to cursor. Error: \"{useResult}\".");
                    return false;
                }
            }
            else
            {
                var pickupResult = inventory.Pickup(itemId, true);
                if (pickupResult != PickupResult.None)
                {
                    Log.Error($"[PickItemToCursor] Failed to pick item to cursor. Error: \"{pickupResult}\".");
                    return false;
                }
            }

            return await Wait.For(() => CursorItemOverlay.Item != null, "item appear under cursor");
        }

        /// <summary>
        /// Waits for cursor to be empty
        /// </summary>
        private static async Task<bool> WaitForCursorToBeEmpty(int timeout = 5000)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            while (InstanceInfo.GetPlayerInventoryItemsBySlot((InventorySlot)12).Any())
            {
                Log.Info("[WaitForCursorToBeEmpty] Waiting for cursor to become empty.");
                await Coroutines.LatencyWait();

                if (stopwatch.ElapsedMilliseconds > timeout)
                {
                    Log.Info("[WaitForCursorToBeEmpty] Timeout while waiting for cursor to become empty.");
                    return false;
                }
            }

            return true;
        }

    #endregion
    }

    #region Helper Classes and Extensions

    #endregion
}


