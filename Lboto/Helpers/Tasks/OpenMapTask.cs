using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Game.Objects;
using Lboto.Helpers;
using Lboto.Helpers.Mapping;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DreamPoeBot.Loki.Game.LokiPoe;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState.MasterDeviceUi;
using static DreamPoeBot.Loki.Game.LokiPoe.InstanceInfo;
using Utility = DreamPoeBot.Loki.Bot.Utility;

namespace Lboto.Helpers.Tasks
{
    /// <summary>
    /// Task responsible for opening maps and various invitations through the Map Device
    /// Handles different types of content: regular maps, Maven invitations, Simulacrums, etc.
    /// </summary>
    public class OpenMapTask : ITask, IAuthored, ILogicProvider, IMessageHandler, IStartStopEvents, ITickEvents
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();
        /// <summary>
        /// Static helper class to manage Map Device operations
        /// Handles both hideout and Karui Shores map devices
        /// </summary>
        public static class MapDevice
        {
            /// <summary>
            /// Checks if the map device UI is currently open
            /// </summary>
            public static bool IsOpen => (World.CurrentArea.IsMyHideoutArea || World.CurrentArea.Name == "Karui Shores")
                ? MasterDeviceUi.IsOpened
                : MapDeviceUi.IsOpened;

            /// <summary>
            /// Gets the inventory control for the current map device
            /// </summary>
            public static InventoryControlWrapper InventoryControl => (World.CurrentArea.IsMyHideoutArea || World.CurrentArea.Name == "Karui Shores")
                ? MasterDeviceUi.InventoryControl
                : MapDeviceUi.InventoryControl;
        }

        // Task state control
        internal static bool Enabled;

        // Task metadata
        public string Name => "OpenMapTask";
        public string Description => "Task for opening maps via Map Device.";
        public string Author => "ExVault";
        public string Version => "1.0";

        /// <summary>
        /// Main execution method for the OpenMapTask
        /// Handles the complete flow of opening different types of content
        /// </summary>
        public async Task<bool> Run()
        {
            if (!Enabled)
            {
                return false;
            }

            if (!LokiPoe.IsInGame)
            {
                return false;
            }

            if (World.CurrentArea.IsTown)
            {
                if (!TpToHideoutTask.GoToHideout().Result)
                {
                    return false;
                }
            }

            // Don't run in combat areas
            if (World.CurrentArea.IsCombatArea)
            {
                Log.Error("[OpenMapTask] The bot is in a combat area. Return false to continue.");
                return false;
            }

            // Skip if skills UI is open
            if (SkillsUi.IsOpened || AtlasSkillsUi.IsOpened)
            {
                return false;
            }

            // Ensure voidstones are socketed in atlas
            await AtlasHelper.SocketVoidstones();

            //// Search for various invitation types in inventory. Not needed now.
            //Item mavenInvitation = FindInvitationInInventory("Maven's Invitation");
            //Item writhingInvitation = FindInvitationInInventory("Writhing Invitation");
            //Item screamingInvitation = FindInvitationInInventory("Screaming Invitation");
            //Item polaricInvitation = FindInvitationInInventory("Polaric Invitation");
            //Item incandescentInvitation = FindInvitationInInventory("Incandescent Invitation");
            //Item simulacrum = FindSimulacrumInInventory();

            // Priority order: Simulacrum > Maven > Writhing > Screaming > Polaric > Incandescent > Regular Maps
            //if (HasValidItem(simulacrum) && GeneralSettings.SimulacrumsEnabled)
            //{
            //    return await OpenQuestItemInMapDevice(simulacrum);
            //}

            //if (HasValidItem(mavenInvitation) && GeneralSettings.Instance.OpenMavenIvitations)
            //{
            //    return await OpenQuestItemInMapDevice(mavenInvitation);
            //}

            //if (HasValidItem(writhingInvitation) && GeneralSettings.Instance.OpenMiniBossQuestInvitations)
            //{
            //    return await OpenQuestItemInMapDevice(writhingInvitation);
            //}

            //if (HasValidItem(screamingInvitation) && GeneralSettings.Instance.OpenBossQuestInvitations)
            //{
            //    return await OpenQuestItemInMapDevice(screamingInvitation);
            //}

            //if (HasValidItem(polaricInvitation) && GeneralSettings.Instance.OpenMiniBossQuestInvitations)
            //{
            //    return await OpenQuestItemInMapDevice(polaricInvitation);
            //}

            //if (HasValidItem(incandescentInvitation) && GeneralSettings.Instance.OpenBossQuestInvitations)
            //{
            //    return await OpenQuestItemInMapDevice(incandescentInvitation);
            //}

            // Handle regular maps if no invitations are available
            return await ProcessRegularMap();
        }

        /// <summary>
        /// Processes regular map opening with fragments and scarabs
        /// </summary>
        private async Task<bool> ProcessRegularMap()
        {
            // Find a regular map in inventory
            Item selectedMap = Inventories.InventoryItems.Find(item => item.IsMap());
            if (!HasValidItem(selectedMap))
            {
                Log.Error("[OpenMapTask] There is no map in inventory.");
                Enabled = false;
                return true;
            }

            // Open the map device
            if (!await PlayerAction.TryTo(OpenDevice, "Open Map Device", 3, 2000))
            {
                //ErrorManager.ReportError();
                if (!World.CurrentArea.IsCombatArea)
                {
                    return true;
                }
                Log.Error("[OpenMapTask] The bot is in a combat area. Return false to continue.");
                return false;
            }

            // Handle five-slot device settings
            await ValidateMapDeviceSettings();

            // Handle atlas warning dialog
            if (AtlasWarningDialog.IsOpened)
            {
                AtlasWarningDialog.ConfirmDialog(true);
            }

            // Close additional modifier panel if open
            if (MasterDeviceUi.IsAdditionalModifierPannelOpen)
            {
                MasterDeviceUi.ToggleAdditionalModifierPannel();
            }

            // Clean the device before placing items
            if (!await CleanDevice())
            {
                //ErrorManager.ReportError();
                return true;
            }

            // Place the map into the device
            if (!await PlayerAction.TryTo(() => PlaceIntoDevice(selectedMap), "Place map into device", 3))
            {
                //ErrorManager.ReportError();
                return true;
            }

            // Handle influence settings (Searing Exarch, Maven, Eater of Worlds)
           // await HandleInfluenceSettings();

            // Add fragments and scarabs to the device
            await AddFragmentsAndScarabs();

            // Handle portal logic
            return await HandlePortalLogic();
        }

        /// <summary>
        /// Handles influence settings for maps (Exarch, Maven, Eater)
        /// </summary>
        //private async Task HandleInfluenceSettings()
        //{
        //    if (GeneralSettings.Instance.SearingExarchInfluence && MasterDeviceUi.IsTheSearingExarchVisible
        //        && !EnableEye(ChooseExplorationInfluence("Exarch")))
        //    {
        //        ErrorManager.ReportError();
        //    }

        //    if (GeneralSettings.Instance.MavenInfluence && MasterDeviceUi.IsTheMavenInvitationVisible
        //        && !EnableEye(ChooseExplorationInfluence("Maven")))
        //    {
        //        ErrorManager.ReportError();
        //    }

        //    if (GeneralSettings.Instance.EaterOfWorldsInfluence && MasterDeviceUi.IsTheEaterOfWorldsVisible
        //        && !EnableEye(ChooseExplorationInfluence("Eater")))
        //    {
        //        ErrorManager.ReportError();
        //    }
        //}

        /// <summary>
        /// Adds fragments and scarabs to the map device based on settings and priorities
        /// </summary>
        private async Task AddFragmentsAndScarabs()
        {
            // Get all sacrifice fragments and scarabs from inventory
            List<Item> availableFragments = Inventories.InventoryItems
                .Where(item => item.Metadata.Contains("CurrencyVaalFragment")|| item.Name.Contains("Scarab"))
                .ToList();

            // Sort fragments by priority (prioritized scarabs first)
            List<Item> prioritizedFragments = availableFragments
                .OrderByDescending(item => GeneralSettings.Instance.ScarabsToPrioritize
                    .Any(prioritizedScarab => item.FullName.ContainsIgnorecase(prioritizedScarab.Name)))
                .ToList();

            // Place each valid fragment into the device
            foreach (Item currentFragment in prioritizedFragments)
            {
                if (ShouldAddFragment(currentFragment))
                {
                    await PlayerAction.TryTo(() => PlaceIntoDevice(currentFragment),
                        $"Place {currentFragment.FullName} into device", 3);
                }
            }
        }

        /// <summary>
        /// Determines if a fragment should be added to the map device
        /// </summary>
        private bool ShouldAddFragment(Item fragment)
        {
            // Check if fragment is in ignore list (unless it's prioritized)
            bool isIgnored = GeneralSettings.Instance.ScarabsToIgnore
                .Any(ignoredScarab => fragment.FullName.ContainsIgnorecase(ignoredScarab.Name));
            bool isPrioritized = GeneralSettings.Instance.ScarabsToPrioritize
                .Any(prioritizedScarab => fragment.FullName.ContainsIgnorecase(prioritizedScarab.Name));

            if (isIgnored && !isPrioritized)
            {
                return false;
            }

            var deviceItems = MapDevice.InventoryControl.Inventory.Items;

            // Don't mix Shaper and Elder fragments
            if (deviceItems.Any(item => item.FullName.Contains("Shaper")) && fragment.FullName.Contains("Elder"))
            {
                return false;
            }
            if (deviceItems.Any(item => item.FullName.Contains("Elder")) && fragment.FullName.Contains("Shaper"))
            {
                return false;
            }

            // Don't add blight scarab if map already has blight
            if (deviceItems.Any(item => item.Stats.ContainsKey((StatTypeGGG)10055)) && fragment.FullName.Contains("Blight"))
            {
                return false;
            }

            // Don't add scarabs to unique maps (unless it's a sacrifice fragment)
            if (deviceItems.Any(item => item.Stats.ContainsKey((StatTypeGGG)10342) || (int)item.Rarity == 3)
                && !fragment.Metadata.Contains("CurrencyVaalFragment"))
            {
                return false;
            }

            // Don't add duplicate scarab types Need rework
            if (!ScarabRegistry.CanAdd(fragment, deviceItems))
                return false;

            return true;
        }

        /// <summary>
        /// Handles portal creation and entry logic
        /// </summary>
        private async Task<bool> HandlePortalLogic()
        {
            NetworkObject mapDevice = ObjectManager.MapDevice;
            Portal existingPortal = FindNearbyPortal(mapDevice);

            // Apply map device mods if enabled
            //if (GeneralSettings.Instance.UseMapDeviceMods && !ChoseMapDeviceMod(GeneralSettings.Instance.MapDeviceModName))
            //{
            //    Log.Error("[OpenMapTask] Error Choosing Map Device Mod");
            //}

            if (!HasValidPortal(existingPortal))
            {
                // No existing portal - create new one
                return await CreateNewPortalAndEnter(mapDevice);
            }
            else
            {
                // Existing portal found - refresh it
                return await RefreshExistingPortalAndEnter(existingPortal, mapDevice);
            }
        }

        /// <summary>
        /// Creates a new portal and enters it
        /// </summary>
        private async Task<bool> CreateNewPortalAndEnter(NetworkObject mapDevice)
        {
            Log.Warn("[OpenMapTask] Starting fresh map. No portals present");

            await PlayerAction.TryTo(ActivateDevice, "Activate Map Device", 3);

            // Wait for new portals to spawn
            if (!await WaitForPortalSpawn(mapDevice))
            {
                //ErrorManager.ReportError();
                return true;
            }

            await Wait.SleepSafe(500);
            Portal newPortal = FindNearbyPortal(mapDevice);

            if (!await TakeMapPortal(newPortal))
            {
                //ErrorManager.ReportError();
            }
            return true;
        }

        /// <summary>
        /// Refreshes existing portal and enters it
        /// </summary>
        private async Task<bool> RefreshExistingPortalAndEnter(Portal existingPortal, NetworkObject mapDevice)
        {
            bool wasTargetable = existingPortal.IsTargetable;

            if (await PlayerAction.TryTo(ActivateDevice, "Activate Map Device", 3))
            {
                // Wait for old portals to despawn if they were targetable
                if (wasTargetable && !await WaitForPortalDespawn(existingPortal))
                {
                    //ErrorManager.ReportError();
                    return true;
                }

                // Wait for new portals to spawn
                if (!await WaitForPortalSpawn(mapDevice))
                {
                    //ErrorManager.ReportError();
                    return true;
                }

                await Wait.SleepSafe(500);
                Portal refreshedPortal = FindNearbyPortal(mapDevice);

                if (await TakeMapPortal(refreshedPortal))
                {
                    DreamPoeBot.Loki.Bot.Utility.BroadcastMessage(null, "MB_map_portal_entered_event", Array.Empty<object>());
                }
                else
                {
                    //ErrorManager.ReportError();
                }
                return true;
            }

            //ErrorManager.ReportError();
            return true;
        }

        /// <summary>
        /// Waits for new portals to spawn near the map device
        /// </summary>
        private async Task<bool> WaitForPortalSpawn(NetworkObject mapDevice)
        {
            return await Wait.For(delegate {
                Portal spawnedPortal = FindTargetablePortalNear(mapDevice);
                return HasValidPortal(spawnedPortal) &&
                       spawnedPortal.LeadsTo(area => area.IsMap || area.Id == "MavenHub" || area.Id.Contains("Affliction"));
            }, "new map portals spawning", 500, 15000);
        }

        /// <summary>
        /// Waits for old portals to despawn
        /// </summary>
        private async Task<bool> WaitForPortalDespawn(Portal oldPortal)
        {
            return await Wait.For(() => !oldPortal.Fresh<Portal>().IsTargetable,
                "old map portals despawning", 200, 10000);
        }

        /// <summary>
        /// Finds a portal near the specified map device
        /// </summary>
        private Portal FindNearbyPortal(NetworkObject mapDevice)
        {
            return ObjectManager.Objects
                .Where(obj => obj.Position.Distance(mapDevice.Position) < 20)
                .Closest<Portal>();
        }

        /// <summary>
        /// Finds a targetable portal near the specified map device
        /// </summary>
        private Portal FindTargetablePortalNear(NetworkObject mapDevice)
        {
            return ObjectManager.Objects
                .Where(obj => obj.Position.Distance(mapDevice.Position) < 20 && obj.IsTargetable)
                .Closest<Portal>();
        }

        /// <summary>
        /// Validates map device settings and adjusts them if necessary
        /// </summary>
        private async Task ValidateMapDeviceSettings()
        {
            // Проверка на шестислотовое устройство
            if (MasterDeviceUi.IsSixSlotDevice)
            {
                if (!GeneralSettings.Instance.UseSixSlotMapDevice)
                {
                    Log.Warn("[OpenMapTask] Device is six-slot! Setting UseSixSlotMapDevice to true!");
                    GeneralSettings.Instance.UseSixSlotMapDevice = true;
                }

                if (GeneralSettings.Instance.UseFiveSlotMapDevice)
                {
                    Log.Warn("[OpenMapTask] Device is six-slot! Setting UseFiveSlotMapDevice to false!");
                    GeneralSettings.Instance.UseFiveSlotMapDevice = false;
                }

                return;
            }

            // Проверка на пятислотовое устройство
            if (MasterDeviceUi.IsFiveSlotDevice)
            {
                if (!GeneralSettings.Instance.UseFiveSlotMapDevice)
                {
                    Log.Warn("[OpenMapTask] Device is five-slot! Setting UseFiveSlotMapDevice to true!");
                    GeneralSettings.Instance.UseFiveSlotMapDevice = true;
                }

                if (GeneralSettings.Instance.UseSixSlotMapDevice)
                {
                    Log.Warn("[OpenMapTask] Device is five-slot! Setting UseSixSlotMapDevice to false!");
                    GeneralSettings.Instance.UseSixSlotMapDevice = false;
                }

                return;
            }

            // Устройство не пяти- и не шестислотовое
            if (GeneralSettings.Instance.UseFiveSlotMapDevice || GeneralSettings.Instance.UseSixSlotMapDevice)
            {
                Log.Error("[OpenMapTask] Device is not five- or six-slot! Disabling both slot settings!");
                GeneralSettings.Instance.UseFiveSlotMapDevice = false;
                GeneralSettings.Instance.UseSixSlotMapDevice = false;
            }
        }


        /// <summary>
        /// Finds an invitation item in inventory by name
        /// </summary>
        private Item FindInvitationInInventory(string invitationName)
        {
            return Inventories.InventoryItems.Find(item =>
                item.Name.Contains(invitationName) && item.Class == "QuestItem");
        }

        /// <summary>
        /// Finds a Simulacrum item in inventory
        /// </summary>
        private Item FindSimulacrumInInventory()
        {
            return Inventories.InventoryItems.Find(item =>
                item.Metadata.Contains("CurrencyAfflictionFragment") && item.Class == "MapFragment");
        }

        /// <summary>
        /// Checks if an item reference is valid (not null)
        /// </summary>
        private bool HasValidItem(Item item)
        {
            return item != null;
        }

        /// <summary>
        /// Checks if a portal reference is valid (not null)
        /// </summary>
        private bool HasValidPortal(Portal portal)
        {
            return portal != null;
        }

        /// <summary>
        /// Handles messages sent to this task
        /// </summary>
        public MessageResult Message(Message message)
        {
            if (message.Id == "MB_new_map_entered_event")
            {
                Enabled = false;
                return MessageResult.Processed;
            }
            return MessageResult.Unprocessed;
        }

        /// <summary>
        /// Opens the map device UI
        /// </summary>
        public static async Task<bool> OpenDevice()
        {
            if (MasterDeviceUi.IsOpened)
            {
                return true;
            }

            int attemptCount = 0;
            NetworkObject mapDevice;

            // Try to find the map device with retries for hideout
            while (true)
            {
                mapDevice = ObjectManager.GetObjectsByName("Map Device").FirstOrDefault(x => x.IsHighlightable);
                if (mapDevice != null)
                {
                    break;
                }

                if (!World.CurrentArea.IsCombatArea)
                {
                    if (World.CurrentArea.IsMyHideoutArea || World.CurrentArea.Name == "Karui Shores")
                    {
                        if (attemptCount < 3)
                        {
                            attemptCount++;
                            //Log.Error($"[OpenMapTask] Fail to find Map Device in hideout, trying to load hideout template [{attemptCount}/3].");
                            //await PlayerAction.LoadHideoutTemplate();
                            await Wait.SleepSafe(1000);
                            continue;
                        }
                        Log.Error("[OpenMapTask] Fail to find Map Device in hideout.");
                    }
                    else
                    {
                        Log.Error("[OpenMapTask] Unknown error. Fail to find Map Device in Templar Laboratory.");
                    }

                    if (World.CurrentArea.IsMyHideoutArea || World.CurrentArea.Name == "Karui Shores" || World.CurrentArea.IsMapRoom)
                    {
                        Log.Error("[OpenMapTask] Now stopping the bot because it cannot continue.");
                        BotManager.Stop(new StopReasonData("no_map_device", "Unable to find map device in " + World.CurrentArea.Name, null), false);
                        return false;
                    }
                    return true;
                }

                Log.Error("[OpenMapTask] The bot probably clicked on a open portal and is now in a combat area. Return false to continue.");
                return false;
            }

            Log.Debug("[OpenMapTask] Now going to open Map Device.");

            // Move closer to map device if needed
            if (mapDevice.Distance > 20f)
            {
                await mapDevice.WalkablePosition().ComeAtOnce();
            }

            // Interact with map device to open UI
            if (!await PlayerAction.Interact(mapDevice, () => MasterDeviceUi.IsOpened, "Map Device opening", 6000))
            {
                Log.Debug("[OpenMapTask] Fail to open Map Device.");
                return false;
            }

            Log.Debug("[OpenMapTask] Map Device has been successfully opened.");
            return true;
        }

        /// <summary>
        /// Opens a quest item (invitation, simulacrum, etc.) in the map device
        /// </summary>
        private static async Task<bool> OpenQuestItemInMapDevice(Item questItem)
        {
            if (await PlayerAction.TryTo(OpenDevice, "Open Map Device", 3, 2000))
            {
                if (AtlasWarningDialog.IsOpened)
                {
                    AtlasWarningDialog.ConfirmDialog(true);
                }

                if (!await CleanDevice())
                {
                    //ErrorManager.ReportError();
                    return true;
                }

                if (!await PlayerAction.TryTo(() => PlaceIntoDevice(questItem),
                    $"Place {questItem.FullName} into device", 3))
                {
                    //ErrorManager.ReportError();
                    return true;
                }
                return false;
            }

            //ErrorManager.ReportError();
            return true;
        }

        /// <summary>
        /// Cleans all items from the map device
        /// </summary>
        internal static async Task<bool> CleanDevice()
        {
            List<Item> deviceItems = MapDevice.InventoryControl.Inventory.Items.ToList();
            if (deviceItems.Count == 0)
            {
                return true;
            }

            Log.Error("[OpenMapTask] Map Device is not empty. Now going to clean it.");

            foreach (Item itemToRemove in deviceItems)
            {
                Log.Debug($"[OpenMapTask] Taking out {itemToRemove.FullName} from device");
                if (!await PlayerAction.TryTo(() => FastMoveFromDevice(itemToRemove.LocationTopLeft), null, 2))
                {
                    Log.Error("[OpenMapTask] Error cleaning map device!");
                    return false;
                }
            }

            Log.Debug("[OpenMapTask] Map Device has been successfully cleaned.");
            return true;
        }

        /// <summary>
        /// Places an item into the map device
        /// </summary>
        internal static async Task<bool> PlaceIntoDevice(Item itemToPlace)
        {
            int currentItemCount = GetCurrentDeviceItemCount();

            // Check if device is full
            if (IsDeviceFull(currentItemCount))
            {
                return true;
            }

            Vector2i itemPosition = itemToPlace.LocationTopLeft;

            // Handle stacked items - split if needed
            //if (itemToPlace.StackCount > 1)
            //{
            //    await SplitStackedItem(itemToPlace);
            //    itemPosition = GetSplitItemPosition(itemToPlace);
            //}

            // Move item to device
            if (await Inventories.FastMoveFromInventory(itemPosition))
            {
                return await WaitForItemPlacement(currentItemCount);
            }

            return false;
        }

        /// <summary>
        /// Gets current number of items in the device
        /// </summary>
        private static int GetCurrentDeviceItemCount()
        {
            var v = MapDevice.InventoryControl.Inventory.Items.Count;    
            Log.Debug($"[OpenMapTask] Current item count in Map Device is {v}");
            return v;
        }

        /// <summary>
        /// Checks if the device is full
        /// </summary>
        private static bool IsDeviceFull(int currentItemCount)
        {
            int maxCapacity = MasterDeviceUi.IsSixSlotDevice ? 6 :
                MasterDeviceUi.IsFiveSlotDevice ? 5 : 4;
            return currentItemCount >= maxCapacity;
        }

        /// <summary>
        /// Splits a stacked item for placement
        /// </summary>
        private static async Task SplitStackedItem(Item stackedItem)
        {
            await Inventories.SplitAndPlaceItemInMainInventory(
                InventoryUi.InventoryControl_Main, stackedItem, 1);
        }

        /// <summary>
        /// Gets position of split item
        /// </summary>
        private static Vector2i GetSplitItemPosition(Item originalItem)
        {
            Item splitItem = Inventories.InventoryItems
                .Where(item => item.FullName == originalItem.FullName)
                .OrderBy(item => item.StackCount)
                .FirstOrDefault();

            return splitItem?.LocationTopLeft ?? originalItem.LocationTopLeft;
        }

        /// <summary>
        /// Waits for item to be placed in device
        /// </summary>
        private static async Task<bool> WaitForItemPlacement(int previousCount)
        {        
            return await Wait.For(() =>
                    MapDevice.InventoryControl.Inventory.Items.Count == previousCount + 1,
                    "item amount change in Map Device");
        }

        /// <summary>
        /// Determines which master mission to use based on map and available missions
        /// </summary>
        private static MasterMissionEnum GetMasterMission(Item mapItem)
        {
            bool isSpecialContent = IsSpecialContent(mapItem);
            bool isBlightedMap = mapItem.ImplicitStats.ContainsKey((StatTypeGGG)10342);
            int mapTier = mapItem.MapTier;
            bool hasScarabMission = HasScarabMission();

            // Don't use master missions for special content
            if (isSpecialContent || isBlightedMap || (int)mapItem.Rarity == 3 || hasScarabMission)
            {
                return MasterMissionEnum.None;
            }

            // Try Kirac missions first if available
            if (ShouldUseKiracMissions())
            {
                return MasterMissionEnum.Kirac;
            }

            // Select appropriate master mission based on tier and availability
            return SelectMasterMissionByTier(mapTier);
        }

        /// <summary>
        /// Checks if the item is special content (simulacrum, invitations)
        /// </summary>
        private static bool IsSpecialContent(Item item)
        {
            return item.Metadata.Contains("CurrencyAfflictionFragment") ||
                   item.Name.Contains(" Invitation");
        }

        /// <summary>
        /// Checks if there are scarabs that provide missions in the device
        /// </summary>
        private static bool HasScarabMission()
        {
            return MapDevice.InventoryControl.Inventory.Items.Any(item =>
                    item.FullName.Contains("Bestiary") ||
                    item.FullName.Contains("Sulphite"));
        }

        /// <summary>
        /// Determines if Kirac missions should be used
        /// </summary>
        private static bool ShouldUseKiracMissions()
        {
            return GeneralSettings.Instance.RunKiracMissions &&
                   World.CurrentArea.IsMyHideoutArea &&
                   (Atlas.KiracNormalMissionsLeft > 0 ||
                    Atlas.KiracYellowMissionsLeft > 0 ||
                    Atlas.KiracRedMissionsLeft > 0);
        }

        /// <summary>
        /// Selects master mission based on map tier
        /// </summary>
        private static MasterMissionEnum SelectMasterMissionByTier(int mapTier)
        {
            if (mapTier <= 5)
            {
                return GetNormalTierMission();
            }
            else if (mapTier <= 10)
            {
                return GetYellowTierMission();
            }
            else
            {
                return GetRedTierMission();
            }
        }

        /// <summary>
        /// Gets available master mission for normal tier maps (T1-T5)
        /// </summary>
        private static MasterMissionEnum GetNormalTierMission()
        {
            if (GeneralSettings.Instance.RunAlvaMissions && Atlas.AlvaNormalMissionsLeft > 0)
                return MasterMissionEnum.Alva;
            if (GeneralSettings.Instance.RunEinhardMissions && Atlas.EinharNormalMissionsLeft > 0)
                return MasterMissionEnum.Einhar;
            if (GeneralSettings.Instance.RunNikoMissions && Atlas.NikoNormalMissionsLeft > 0)
                return MasterMissionEnum.Niko;
            if (GeneralSettings.Instance.RunJunMissions && Atlas.JunNormalMissionsLeft > 0)
                return MasterMissionEnum.Jun;

            return MasterMissionEnum.None;
        }

        /// <summary>
        /// Gets available master mission for yellow tier maps (T6-T10)
        /// </summary>
        private static MasterMissionEnum GetYellowTierMission()
        {
            if (GeneralSettings.Instance.RunAlvaMissions && Atlas.AlvaYellowMissionsLeft > 0)
                return MasterMissionEnum.Alva;
            if (GeneralSettings.Instance.RunEinhardMissions && Atlas.EinharYellowMissionsLeft > 0)
                return MasterMissionEnum.Einhar;
            if (GeneralSettings.Instance.RunNikoMissions && Atlas.NikoYellowMissionsLeft > 0)
                return MasterMissionEnum.Niko;
            if (GeneralSettings.Instance.RunJunMissions && Atlas.JunYellowMissionsLeft > 0)
                return MasterMissionEnum.Jun;

            return MasterMissionEnum.None;
        }

        /// <summary>
        /// Gets available master mission for red tier maps (T11+)
        /// </summary>
        private static MasterMissionEnum GetRedTierMission()
        {
            if (GeneralSettings.Instance.RunAlvaMissions && Atlas.AlvaRedMissionsLeft > 0)
                return MasterMissionEnum.Alva;
            if (GeneralSettings.Instance.RunEinhardMissions && Atlas.EinharRedMissionsLeft > 0)
                return MasterMissionEnum.Einhar;
            if (GeneralSettings.Instance.RunNikoMissions && Atlas.NikoRedMissionsLeft > 0)
                return MasterMissionEnum.Niko;
            if (GeneralSettings.Instance.RunJunMissions && Atlas.JunRedMissionsLeft > 0)
                return MasterMissionEnum.Jun;

            return MasterMissionEnum.None;
        }

        /// <summary>
        /// Activates the map device to create portals
        /// </summary>
        private static async Task<bool> ActivateDevice()
        {
            Log.Debug("[OpenMapTask] Now going to activate the Map Device.");
            await Wait.SleepSafe(500);

            Item mapInDevice = FindMapInDevice();
            if (mapInDevice == null)
            {
                Log.Error("[OpenMapTask] Unexpected error. There is no map in the Map Device.");
                return false;
            }

            // Close additional modifier panel if open
            if (MasterDeviceUi.IsAdditionalModifierPannelOpen)
            {
                Log.Debug("[OpenMapTask] Closing additional modifier panel");
                if (!MasterDeviceUi.ToggleAdditionalModifierPannel())
                {
                    return false;
                }
            }

            // Handle special content (invitations, simulacrum)
            if (IsSpecialContentMap(mapInDevice))
            {
                Log.Debug($"[OpenMapTask] {mapInDevice.Name} is in the device. No additional actions needed. Activating device.");
                return await PressButtonAndWait();
            }

            // Handle master missions for regular maps in hideout
            if (World.CurrentArea.IsMyHideoutArea || World.CurrentArea.Name == "Karui Shores")
            {
                return await HandleMasterMissions(mapInDevice);
            }

            return await PressButtonAndWait(isMaster: false);
        }

        /// <summary>
        /// Finds the map item currently in the device
        /// </summary>
        private static Item FindMapInDevice()
        {
            return MapDevice.InventoryControl.Inventory.Items.Find(item =>
                item.Class == "Map" ||
                item.Metadata.Contains("CurrencyAfflictionFragment") ||
                item.Name.Contains("Maven's Invitation") ||
                item.Name.Contains("Writhing Invitation") ||
                item.Name.Contains("Screaming Invitation") ||
                item.Name.Contains("Polaric Invitation") ||
                item.Name.Contains("Incandescent Invitation"));
        }

        /// <summary>
        /// Checks if the map is special content that doesn't need master missions
        /// </summary>
        private static bool IsSpecialContentMap(Item map)
        {
            return map.Name.Contains("Maven's Invitation") ||
                   map.Name.Contains("Writhing Invitation") ||
                   map.Name.Contains("Screaming Invitation") ||
                   map.Name.Contains("Polaric Invitation") ||
                   map.Name.Contains("Incandescent Invitation") ||
                   map.Name.Contains("Simulacrum");
        }

        /// <summary>
        /// Handles master mission selection and activation
        /// </summary>
        private static async Task<bool> HandleMasterMissions(Item mapInDevice)
        {
            MasterMissionEnum selectedMasterMission = GetMasterMission(mapInDevice);

            if (selectedMasterMission == MasterMissionEnum.None)
            {
                return await PressButtonAndWait();
            }

            if (selectedMasterMission == MasterMissionEnum.Kirac)
            {
                return await HandleKiracMissions();
            }

            // Handle other master missions
            if (MasterDeviceUi.SelectedMasterMission != selectedMasterMission)
            {
                Log.Debug($"[OpenMapTask] Need to switch {MasterDeviceUi.SelectedMasterMission} to {selectedMasterMission}");
                SelectMasterMissionResult result = MasterDeviceUi.SelectMasterMission(selectedMasterMission);

                if (!await Wait.For(() => MasterDeviceUi.SelectedMasterMission == selectedMasterMission, "Select Master Mission"))
                {
                    Log.Error($"[OpenMapTask] SelectMasterMission error: {result}");
                    //ErrorManager.ReportError();
                    return false;
                }
            }

            return await PressButtonAndWait();
        }

        /// <summary>
        /// Handles Kirac mission selection and activation
        /// </summary>
        private static async Task<bool> HandleKiracMissions()
        {
            // Open Kirac missions UI
            if (!await OpenKiracMissionsUI())
            {
                return false;
            }

            // Select appropriate map
            Item chosenMap = await SelectBestKiracMap();
            if (chosenMap == null)
            {
                Log.Error("[OpenMapTask] Something is wrong. Map of choice is null. Kirac missions disabled until bot restart.");
                GeneralSettings.Instance.RunKiracMissions = false;
                return false;
            }

            // Ensure the correct map is selected
            if (!await EnsureKiracMapSelected(chosenMap))
            {
                return false;
            }

            // Activate the mission
            return await ActivateKiracMission();
        }

        /// <summary>
        /// Opens the Kirac missions UI
        /// </summary>
        private static async Task<bool> OpenKiracMissionsUI()
        {
            if (KiracMissionsUi.IsOpened)
            {
                return true;
            }

            OpenKiracMissionsResult result = MasterDeviceUi.OpenKiracMissions(true);
            if (result != OpenKiracMissionsResult.None)
            {
                Log.Error($"[OpenMapTask] Kirac mission UI failed to open {result}.");
                return false;
            }

            await Coroutines.LatencyWait();
            await Wait.For(() => KiracMissionsUi.IsOpened, "Kirac mission UI to open", 10, 1000);
            Log.Warn("[OpenMapTask] Kirac mission UI opened.");
            return true;
        }

        /// <summary>
        /// Selects the best available Kirac map based on priority
        /// </summary>
        private static async Task<Item> SelectBestKiracMap()
        {
            List<Item> unbannedMaps = KiracMissionsUi.AvailableMaps
                .Where(map => map.GetBannedAffix() == null)
                .ToList();

            Item bestMap = unbannedMaps
                .OrderByDescending(map => !map.Ignored())
                .ThenByDescending(map => map.Priority())
                .FirstOrDefault();

            if (bestMap != null)
            {
                Log.Warn($"[OpenMapTask] Map of choice: [{bestMap.FullName} {bestMap.Name} prio: {bestMap.Priority()}] tier: {bestMap.MapTier} rarity {bestMap.Rarity}");
            }

            return bestMap;
        }

        /// <summary>
        /// Ensures the correct Kirac map is selected in the UI
        /// </summary>
        private static async Task<bool> EnsureKiracMapSelected(Item targetMap)
        {
            Item currentlySelected = KiracMissionsUi.SelectedMap;

            if (currentlySelected != null)
            {
                Log.Debug($"[OpenMapTask] Selected map: {currentlySelected.FullName}");
            }

            // Check if we need to switch maps
            if (currentlySelected == null ||
                currentlySelected != targetMap ||
                string.IsNullOrEmpty(KiracMissionsUi.SelectedMapDescription))
            {

                if (currentlySelected != null)
                {
                    Log.Warn($"[OpenMapTask] Need to switch {currentlySelected.FullName} to {targetMap.FullName}");
                }

                SelectKiracMissionMapResult selectResult = KiracMissionsUi.SelectKiracMissionMap(targetMap);
                if (selectResult != SelectKiracMissionMapResult.None)
                {
                    Log.Error($"[OpenMapTask] Error switching map! {selectResult}");
                    return false;
                }

                await Coroutines.LatencyWait();
                await Wait.For(() => KiracMissionsUi.SelectedMap == targetMap, "Select map", 10, 1000);
                Log.Debug("[OpenMapTask] Successfully switched map!");
            }

            return true;
        }

        /// <summary>
        /// Activates the selected Kirac mission
        /// </summary>
        private static async Task<bool> ActivateKiracMission()
        {
            ActivateKiracMissionResult activationResult = KiracMissionsUi.ActivateKiracMission();
            if (activationResult != ActivateKiracMissionResult.None)
            {
                Log.Error($"[OpenMapTask] Fail to activate Kirac mission map {activationResult}.");
                return false;
            }

            await Coroutines.LatencyWait();
            if (!await Wait.For(() => !KiracMissionsUi.IsOpened, "Kirac mission UI closing"))
            {
                Log.Error("[OpenMapTask] Fail to activate Kirac mission map (device ui not closed).");
                return false;
            }

            Log.Debug("[OpenMapTask] Successfully activated Kirac mission map!");
            Utility.BroadcastMessage(null, "MB_kirac_mission_opened_event", Array.Empty<object>());
            return true;
        }

        /// <summary>
        /// Removes an item from the map device by position
        /// </summary>
        public static async Task<bool> FastMoveFromDevice(Vector2i itemPosition)
        {
            Item itemToRemove = MasterDeviceUi.InventoryControl.Inventory.FindItemByPos(itemPosition);
            if (itemToRemove == null)
            {
                Log.Error($"[FastMoveFromDevice] Fail to find item at {itemPosition} in Map Device.");
                return false;
            }

            string itemName = itemToRemove.FullName;
            Log.Debug($"[FastMoveFromDevice] Fast moving \"{itemName}\" at {itemPosition} from Map Device.");

            FastMoveResult moveResult = MasterDeviceUi.InventoryControl.FastMove(itemToRemove.LocalId, true, true);
            if (moveResult != FastMoveResult.None)
            {
                Log.Error($"[FastMoveFromDevice] Fast move error: \"{moveResult}\".");
                return false;
            }

            if (await Wait.For(() => MasterDeviceUi.InventoryControl.Inventory.FindItemByPos(itemPosition) == null, "fast move"))
            {
                Log.Debug($"[FastMoveFromDevice] \"{itemName}\" at {itemPosition} has been successfully fast moved from Map Device.");
                return true;
            }

            Log.Error($"[FastMoveFromDevice] Fast move timeout for \"{itemName}\" at {itemPosition} in Map Device.");
            return false;
        }

        /// <summary>
        /// Attempts to take a portal to enter the map
        /// </summary>
        public static async Task<bool> TakeMapPortal(Portal portalToTake, int maxAttempts = 3)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (!LokiPoe.IsInGame || World.CurrentArea.IsMap)
                {
                    return true;
                }

                await Coroutines.CloseBlockingWindows();
                Log.Debug($"[OpenMapTask] Take portal to map attempt: {attempt}/{maxAttempts}");

                if (await PlayerAction.TakePortal(portalToTake))
                {
                    return true;
                }

                await Wait.SleepSafe(1000);
            }

            return false;
        }

        /// <summary>
        /// Selects a map device modifier (Zana mod)
        /// </summary>
        private static bool ChoseMapDeviceMod(string modName)
        {
            // Check if the requested mod is available
            if (!MasterDeviceUi.ZanaMods.Any(mod => mod.Name.EqualsIgnorecase(modName)) &&
                !modName.EqualsIgnorecase("free"))
            {
                Log.Error("[OpenMapTask] Selected Zana mod is not yet unlocked. Skip");
                return true;
            }

            ZanaMod selectedMod;

            // Handle "free" mod selection
            if (modName.Equals("free"))
            {
                ZanaMod freeMod = MasterDeviceUi.ZanaMods
                    .OrderBy(_ => Guid.NewGuid())
                    .FirstOrDefault(mod => mod.ChaosCost == 0);

                if (freeMod == null)
                {
                    Log.Debug("[OpenMapTask] No free mods found.");
                    return true;
                }
                selectedMod = freeMod;
            }
            else
            {
                selectedMod = MasterDeviceUi.ZanaMods.First(mod => mod.Name.EqualsIgnorecase(modName));
            }

            string logMessage = selectedMod.ChaosCost != 0
                ? $"[OpenMapTask] Now going to chose mod {selectedMod.Name}"
                : $"[OpenMapTask] Now going to chose free mod {selectedMod.Name}";
            Log.Warn(logMessage);

            SelectZanaModResult result = MasterDeviceUi.SelectZanaMod(selectedMod);
            if (result != SelectZanaModResult.None)
            {
                Log.Error($"[OpenMapTask] Mod chosing error: {result}");
            }

            return result == SelectZanaModResult.None ;
        }

        /// <summary>
        /// Enables the specified influence eye (Maven, Exarch, Eater)
        /// </summary>
        public static bool EnableEye(string eyeType)
        {
            if (!MasterDeviceUi.IsTheMavenInvitationVisible &&
                !MasterDeviceUi.IsTheSearingExarchVisible &&
                !MasterDeviceUi.IsTheEaterOfWorldsVisible)
            {
                return true;
            }

            Log.Warn($"[OpenMapTask] Trying to enable {eyeType}'s Eye.");

            EnableTheMavenInvitationResult result = EnableSpecificEye(eyeType);

            if (result == EnableTheMavenInvitationResult.None)
            {
                return HandleEyeActivationSuccess(eyeType);
            }

            Log.Error($"[OpenMapTask] Error enabling {eyeType}'s Eye. Error: {result}");
            return false;
        }

        /// <summary>
        /// Enables the specific eye type
        /// </summary>
        private static EnableTheMavenInvitationResult EnableSpecificEye(string eyeType)
        {
            if (eyeType == "Eater" && MasterDeviceUi.IsTheEaterOfWorldsVisible)
            {
                return MasterDeviceUi.EnableTheEaterOfWorlds();
            }
            else if (eyeType == "Exarch" && MasterDeviceUi.IsTheSearingExarchVisible)
            {
                return MasterDeviceUi.EnableTheSearingExarch();
            }
            else if (eyeType == "Maven" && MasterDeviceUi.IsTheMavenInvitationVisible)
            {
                return MasterDeviceUi.EnableTheMavenInvitation();
            }
            else
            {
                return EnableTheMavenInvitationResult.None;
            }
        }

        /// <summary>
        /// Handles successful eye activation or tries alternatives
        /// </summary>
        private static bool HandleEyeActivationSuccess(string requestedEyeType)
        {
            if (requestedEyeType == "Exarch")
            {
                return HandleExarchEyeActivation();
            }
            else if (requestedEyeType == "Eater")
            {
                return HandleEaterEyeActivation();
            }
            else if (requestedEyeType == "Maven")
            {
                return HandleMavenEyeActivation();
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Handles Exarch eye activation with fallbacks
        /// </summary>
        private static bool HandleExarchEyeActivation()
        {
            if (MasterDeviceUi.IsTheSearingExarchClicked)
            {
                return true;
            }

            Log.Warn("[OpenMapTask] Exarch's Eye not clickable. Using another one");
            return TryAlternativeEyes("Exarch");
        }

        /// <summary>
        /// Handles Eater eye activation with fallbacks
        /// </summary>
        private static bool HandleEaterEyeActivation()
        {
            if (MasterDeviceUi.IsTheEaterOfWorldsClicked)
            {
                return true;
            }

            Log.Warn("[OpenMapTask] Eater's Eye not clickable. Using another one");
            return TryAlternativeEyes("Eater");
        }

        /// <summary>
        /// Handles Maven eye activation with fallbacks
        /// </summary>
        private static bool HandleMavenEyeActivation()
        {
            if (MasterDeviceUi.IsTheMavenInvitationClicked)
            {
                return true;
            }

            Log.Warn("[OpenMapTask] Maven's Eye not clickable. Using another one");
            return TryAlternativeEyes("Maven");
        }

        /// <summary>
        /// Tries alternative eyes when the preferred one is not available
        /// </summary>
        private static bool TryAlternativeEyes(string originalEyeType)
        {
            // Try other available eyes as fallbacks
            string[] alternativeOrder;
            if (originalEyeType == "Exarch")
            {
                alternativeOrder = new string[] { "Eater", "Maven" };
            }
            else if (originalEyeType == "Eater")
            {
                alternativeOrder = new string[] { "Exarch", "Maven" };
            }
            else if (originalEyeType == "Maven")
            {
                alternativeOrder = new string[] { "Exarch", "Eater" };
            }
            else
            {
                alternativeOrder = new string[0];
            }

            foreach (string alternativeEye in alternativeOrder)
            {
                if (TryEnableAlternativeEye(alternativeEye, originalEyeType))
                {
                    return true;
                }
            }

            return true;
        }

        /// <summary>
        /// Attempts to enable an alternative eye
        /// </summary>
        private static bool TryEnableAlternativeEye(string eyeToTry, string originalEye)
        {
            EnableTheMavenInvitationResult result = EnableSpecificEye(eyeToTry);

            if (result != EnableTheMavenInvitationResult.None)
            {
                return false;
            }

            bool isActivated;
            if (eyeToTry == "Exarch")
            {
                isActivated = MasterDeviceUi.IsTheSearingExarchClicked;
            }
            else if (eyeToTry == "Eater")
            {
                isActivated = MasterDeviceUi.IsTheEaterOfWorldsClicked;
            }
            else if (eyeToTry == "Maven")
            {
                isActivated = MasterDeviceUi.IsTheMavenInvitationClicked;
            }
            else
            {
                isActivated = false;
            }

            if (isActivated)
            {
                Log.Warn($"[OpenMapTask] Used {eyeToTry}'s Eye instead of {originalEye}'s");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Presses the activate button and waits for the device to activate
        /// </summary>
        private static async Task<bool> PressButtonAndWait(bool isMaster = true)
        {
            ActivateResult activationResult = isMaster
                ? MasterDeviceUi.Activate(true)
                : MapDeviceUi.Activate(true);

            await Coroutines.LatencyWait();

            if (activationResult != ActivateResult.None)
            {
                Log.Error($"[OpenMapTask] Fail to activate the Map Device {activationResult}. Is master = {isMaster}");
                
                return false;
            }

            if (!await Wait.For(() => !MapDeviceUi.IsOpened && !MasterDeviceUi.IsOpened, "Map Device closing"))
            {
                Log.Error("[OpenMapTask] Fail to activate the Map Device.");
                return false;
            }

            Log.Debug("[OpenMapTask] Map Device has been successfully activated.");
            return true;
        }

        /// <summary>
        /// Chooses the appropriate exploration influence based on atlas progression
        /// </summary>
        private static string ChooseExplorationInfluence(string preferredInfluence)
        {
            if (!GeneralSettings.Instance.AtlasExplorationEnabled)
            {
                return preferredInfluence;
            }

            // Check quest states for atlas progression
            DatQuestStateWrapper tangleQuest = InGameState.GetCurrentQuestStateAccurate("tangle");
            bool hasCompletedEaterPath = tangleQuest != null && tangleQuest.Id == 0;

            DatQuestStateWrapper cleansingFireQuest = InGameState.GetCurrentQuestStateAccurate("cleansing_fire");
            bool hasCompletedExarchPath = cleansingFireQuest != null && cleansingFireQuest.Id == 0;

            DatQuestStateWrapper mavenQuest = InGameState.GetCurrentQuestStateAccurate("maven_atlas");
            int? mavenQuestId = mavenQuest?.Id;
            bool hasAccessToMaven = mavenQuestId >= 5 || mavenQuestId == 1;

            // Determine influence based on completion status
            if (hasCompletedExarchPath && hasCompletedEaterPath)
            {
                return preferredInfluence; // Both paths complete, use preference
            }

            if (hasAccessToMaven)
            {
                if (!hasCompletedEaterPath)
                {
                    return "Exarch"; // Focus on Exarch to progress Eater path
                }
                if (!hasCompletedExarchPath)
                {
                    return "Eater"; // Focus on Eater to progress Exarch path
                }
                return preferredInfluence;
            }

            return "Maven"; // Focus on Maven if no access yet
        }

        // Interface implementations
        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public void Tick()
        {
            // Implementation for tick events if needed
        }

        public void Start()
        {
            // Implementation for start events if needed
        }

        public void Stop()
        {
            // Implementation for stop events if needed
        }
    }
}
