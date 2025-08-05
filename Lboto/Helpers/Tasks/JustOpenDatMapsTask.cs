using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Game.Objects;
using Lboto.Helpers.Mapping;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DreamPoeBot.Loki.Game.LokiPoe;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState.MasterDeviceUi;
using static Lboto.Helpers.Tasks.OpenMapTask;

namespace Lboto.Helpers.Tasks
{
    public class JustOpenDatMapsTask : ITask, IAuthored, ILogicProvider, IMessageHandler, IStartStopEvents, ITickEvents
    {
        #region basic things
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();
        // Task state control
        //internal static bool Enabled;
        private static bool _needRefil = false;


        // Task metadata
        public string Name => "OpenMapTask";
        public string Description => "Task for opening maps via Map Device.";
        public string Author => "ExVault";
        public string Version => "1.0";
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

        public MessageResult Message(Message message)
        {
            if (message.Id == "MB_new_map_entered_event")
            {
                OpenMapTask.Enabled = false;
                return MessageResult.Processed;
            }
            return MessageResult.Unprocessed;
        }
        #endregion

        #region future settings

        bool _settingVesselsLogic = true;

        public static List<ScarabInfo> ScarabsToPrioritizeVessel = new List<ScarabInfo>
        {
            new ScarabInfo(
                "Anarchy Scarab",
                "Anarchy",
                2,
                ScarabRegistry.GetControl("Anarchy Scarab")
            ),
            ScarabRegistry.GetByName("Anarchy Scarab of Gigantification"),
            ScarabRegistry.GetByName("Anarchy Scarab of Partnership"),
        };


        #endregion

        private bool HasValidItem(Item item)
        {
            return item != null;
        }

        public async Task<bool> Run()
        {
            if (!OpenMapTask.Enabled)
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
            if (World.CurrentArea.IsCombatArea)
            {
                Log.Error("[OpenMapTask] The bot is in a combat area. Return false to continue.");
                return false;
            }
            if (SkillsUi.IsOpened || AtlasSkillsUi.IsOpened)
            {
                return false;
            }
            await AtlasHelper.SocketVoidstones();

            return await ProcessRegularMap();

        }
        private async Task<bool> ProcessRegularMap()
        {
            // Find a regular map in inventory
            Item selectedMap = Inventories.InventoryItems.Find(item => item.IsMap());
            if (!HasValidItem(selectedMap) && !_needRefil)
            {
                Log.Error("[OpenMapTask] There is no map in inventory.");
                OpenMapTask.Enabled = false;
                return true;
            }

            // Open the map device
            if (!await PlayerAction.TryTo(OpenMapTask.OpenDevice, "Open Map Device", 3, 2000))
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

            if (!MapDevice.InventoryControl.Inventory.Items.Any(x => x.IsMap()))
            { 
            // Place the map into the device
                if (!await PlayerAction.TryTo(() => OpenMapTask.PlaceIntoDevice(selectedMap), "Place map into device", 3))
                {
                    //ErrorManager.ReportError();
                    return true;
                }
            }

            // Handle influence settings (Searing Exarch, Maven, Eater of Worlds)
            // await HandleInfluenceSettings();

            // Add fragments and scarabs to the device
            await AddFragmentsAndScarabs();
            if (_needRefil)
            {
                Log.Debug("[OpenMapTask] Need to refill the device with fragments/scarabs.");
                return true; // If we need to refill, exit early
            }
            //check device mod
            ChoseMapDeviceMod("Anarchy");

            // Handle portal logic
            return await HandlePortalLogic();
        }

        private async Task AddFragmentsAndScarabs()
        {
            //place for fragments count!

            // Check if the device should add fragments and scarabs
            if (!_needRefil)
                return;
            else if (!TakeFragmentsTask.Executed)
            {
                TakeFragmentsTask.ShouldRefill = true;
                return;
            }
            // Get all sacrifice fragments and scarabs from inventory
            // Process each prioritized scarab type individually
            // Debug: Log all fragments in the collection first
            Log.Debug($"[PlaceFragmentTask] ScarabsToPrioritizeVessel contains {JustOpenDatMapsTask.ScarabsToPrioritizeVessel.Count} items:");
            foreach (var debugFragment in JustOpenDatMapsTask.ScarabsToPrioritizeVessel)
            {
                Log.Debug($"[PlaceFragmentTask] - {debugFragment?.Name} (Type: {debugFragment?.Type}, TypeLimit: {debugFragment?.TypeLimit})");
            }


            Log.Debug($"[PlaceFragmentTask] Grouped into {ScarabsToPrioritizeVessel.Count} types:");

            // Process each scarab TYPE (not individual scarab)
            foreach (var typeGroup in ScarabsToPrioritizeVessel)
            {
                string scarabType = typeGroup.Name;
                int maxTypeLimit = typeGroup.TypeLimit; // Use the highest TypeLimit for this type

                Log.Debug($"[PlaceFragmentTask] Processing scarab type: {scarabType} with max type limit {maxTypeLimit}");

                // Check how many of this TYPE are already in the device
                var deviceItems = MapDevice.InventoryControl.Inventory.Items;
                int alreadyInDevice = 0;

                foreach (var item in deviceItems)
                {
                    var itemScarab = ScarabRegistry.GetByName(item.FullName);
                    if (itemScarab != null && string.Equals(itemScarab.Type, scarabType, StringComparison.OrdinalIgnoreCase))
                    {
                        alreadyInDevice++;
                    }
                }

                Log.Debug($"[PlaceFragmentTask] {scarabType}: {alreadyInDevice} already in device, want {maxTypeLimit} total");

                if (alreadyInDevice >= maxTypeLimit)
                {
                    Log.Debug($"[PlaceFragmentTask] {scarabType} already has enough stacks in device ({alreadyInDevice}/{maxTypeLimit}), skipping");
                    continue;
                }

                int needToPlace = maxTypeLimit - alreadyInDevice;
                Log.Debug($"[PlaceFragmentTask] Need to place {needToPlace} more stacks of {scarabType}");

                int placedCount = 0;
                int maxAttempts = needToPlace * 3;
                int attempts = 0;

                // Try to place scarabs of this type until we reach the limit
                while (placedCount < needToPlace && attempts < maxAttempts)
                {
                    attempts++;

                    // Find available stacks of this scarab TYPE in inventory (any variant of this type)
                    List<Item> availableStacks = new List<Item>();

                    foreach (var scarabVariant in ScarabsToPrioritizeVessel)
                    {
                        var stacks = Inventories.InventoryItems
                            .Where(item => item.FullName.ContainsIgnorecase(scarabVariant.Name))
                            .ToList();
                        availableStacks.AddRange(stacks);
                    }

                    Log.Debug($"[PlaceFragmentTask] Found {availableStacks.Count} stacks of {scarabType} type in inventory");

                    if (!availableStacks.Any())
                    {
                        Log.Warn($"[PlaceFragmentTask] No more {scarabType} available in inventory. Placed {placedCount}/{needToPlace}");
                        break;
                    }

                    // Take the first available stack
                    Item currentFragment = availableStacks.First();

                    // Check if we can actually add this fragment
                    if (!ShouldAddFragment(currentFragment))
                    {
                        Log.Debug($"[PlaceFragmentTask] Cannot add more {scarabType} to device (ShouldAddFragment returned false). Placed {placedCount}/{needToPlace}");
                        break;
                    }

                    Log.Debug($"[PlaceFragmentTask] Attempting to place {currentFragment.FullName} (stack size: {currentFragment.StackCount})");

                    // Try to place the fragment
                    bool success = await PlayerAction.TryTo(() => PlaceIntoDevice(currentFragment),
                        $"Place {currentFragment.FullName} into device", 2);

                    if (success)
                    {
                        placedCount++;
                        Log.Debug($"[PlaceFragmentTask] Successfully placed {currentFragment.FullName}. Progress: {placedCount}/{needToPlace}");
                    }
                    else
                    {
                        Log.Error($"[PlaceFragmentTask] Failed to place {currentFragment.FullName} after attempts");
                    }

                    // Small delay between attempts
                    await Wait.SleepSafe(200);

                    // Safety check - if device is full, stop completely
                    if (deviceItems.Count() >= 6) // Assuming 6 is max device slots
                    {
                        Log.Warn($"[PlaceFragmentTask] Map device appears full ({deviceItems.Count()} items), stopping placement");
                        return;
                    }
                }

                if (attempts >= maxAttempts)
                {
                    Log.Error($"[PlaceFragmentTask] Hit max attempts ({maxAttempts}) for {scarabType}. Placed {placedCount}/{needToPlace}");
                }
                else if (placedCount < needToPlace)
                {
                    Log.Warn($"[PlaceFragmentTask] Could not place all desired {scarabType} stacks. Placed {placedCount}/{needToPlace}");
                }

                Log.Debug($"[PlaceFragmentTask] Finished processing {scarabType}. Placed {placedCount}/{needToPlace} stacks in {attempts} attempts.");
            }

            Log.Debug($"[PlaceFragmentTask] Fragment placement completed.");
            _needRefil = false; // Set to false after processing all fragments
        }

        private bool ShouldAddFragment(Item fragment)
        {
            // Check if fragment is in ignore list (unless it's prioritized)
            //bool isIgnored = GeneralSettings.Instance.ScarabsToIgnore
            //    .Any(ignoredScarab => fragment.FullName.ContainsIgnorecase(ignoredScarab.Name));
            //bool isPrioritized = GeneralSettings.Instance.ScarabsToPrioritize
            //    .Any(prioritizedScarab => fragment.FullName.ContainsIgnorecase(prioritizedScarab.Name));

            //if (isIgnored && !isPrioritized)
            //{
            //    return false;
            //}

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

            // Don't add duplicate scarab types
            if (!ScarabRegistry.CanAdd(fragment, deviceItems))
                return false;

            return true;
        }

        private static async Task<bool> CleanDevice()
        {
            _needRefil = true;
            List<Item> deviceItems = MapDevice.InventoryControl.Inventory.Items.ToList();
            if (deviceItems.Count == 0)
            {
                return true;
            }
            var _counter = 0;
            foreach (ScarabInfo scarab in ScarabsToPrioritizeVessel)
            {
                var _scarabs = deviceItems.FindAll(x => x.Name.ContainsIgnorecase(scarab.Name));
                var _scarabCount = _scarabs.Count();
                if (_scarabCount == scarab.TypeLimit)
                {
                    _counter++;
                }
                else
                {
                    break;
                }
                if (_counter == ScarabsToPrioritizeVessel.Count)
                {
                    Log.Debug("[OpenMapTask] Map Device is already filled with prioritized scarabs. No need to clean it.");
                    _needRefil = false;
                    return true;
                }
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
        #region open portal stuff
        private bool HasValidPortal(Portal portal)
        {
            return portal != null;
        }

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

        private async Task<bool> WaitForPortalSpawn(NetworkObject mapDevice)
        {
            return await Wait.For(delegate
            {
                Log.Debug("[PortalScan] Wait delegate tick");

                Portal spawnedPortal = FindTargetablePortalNear(mapDevice);
                Log.Debug($"[PortalScan] Found portal: {spawnedPortal?.ToString() ?? "null"}");

                if (spawnedPortal == null)
                    return false;

                bool isValid = HasValidPortal(spawnedPortal);
                Log.Debug($"[PortalScan] HasValidPortal: {isValid}");

                bool leadsToMap = spawnedPortal.LeadsTo(area => area.IsMap);
                bool leadsToMaven = spawnedPortal.LeadsTo(area => area.Id == "MavenHub");
                bool leadsToAffliction = spawnedPortal.LeadsTo(area => area.Id.Contains("Affliction"));
                bool leadsToTarget = leadsToMap || leadsToMaven || leadsToAffliction;

                Log.Debug($"[PortalScan] LeadsTo - IsMap: {leadsToMap}, MavenHub: {leadsToMaven}, Affliction: {leadsToAffliction}");

                return isValid && leadsToTarget;
            }, "new map portals spawning", 500, 15000);

        }

        private Portal FindTargetablePortalNear(NetworkObject mapDevice)
        {
            return ObjectManager.Objects
                .Where(obj => obj.Position.Distance(mapDevice.Position) < 50 && obj.IsTargetable)
                .Closest<Portal>(x => x.Metadata.ContainsIgnorecase("Metadata/MiscellaneousObjects/MultiplexPortal") );
        }

        private async Task<bool> CreateNewPortalAndEnter(NetworkObject mapDevice)
        {
            Log.Warn("[OpenMapTask] Starting fresh map. No portals present");

            await PlayerAction.TryTo(ActivateDevice, "Activate Map Device", 3);

            // Wait for new portals to spawn
            if (!await WaitForPortalSpawn(mapDevice))
            {
                //ErrorManager.ReportError();
                Log.Error("[OpenMapTask] Failed to spawn new portal.");
                return false;
            }

            await Wait.SleepSafe(500);
            Portal newPortal = FindNearbyPortal(mapDevice);

            if (!await TakeMapPortal(newPortal))
            {
                //ErrorManager.ReportError();
                Log.Error("[OpenMapTask] Failed to take map portal.");
                return false;
            }
            return true;
        }

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
                    Log.Error("[OpenMapTask] Failed to take map portal.");
                }
                return true;
            }

            //ErrorManager.ReportError();
            return true;
        }

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
                selectedMod = MasterDeviceUi.ZanaMods.First(mod => mod.Name.ContainsIgnorecase(modName));
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

            return result == SelectZanaModResult.None;
        }

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

            return await PressButtonAndWait(isMaster: true);
        }

        private static async Task<bool> PressButtonAndWait(bool isMaster = true)
        {
            ActivateResult activationResult = isMaster
                ? MasterDeviceUi.Activate(true)
                : MapDeviceUi.Activate(true);

            await Coroutines.LatencyWait();

            if (activationResult != ActivateResult.None)
            {
                Log.Error($"[OpenMapTask] Fail to activate the Map Device {activationResult}");
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

        private Portal FindNearbyPortal(NetworkObject mapDevice)
        {
            var nearbyObjects = ObjectManager.Objects
                .Where(obj => obj.Position.Distance(mapDevice.Position) < 40)
                .ToList();
            Log.Debug($"Number of nearby objects: {nearbyObjects.Count}");
            if (nearbyObjects.Count == 0)
            {
                Log.Error($"[OpenMapTask] mapDevice.Position: {mapDevice.Position}, my position: {LokiPoe.Me.Position}");
                nearbyObjects = ObjectManager.Objects
                    .Where(obj => obj.Position.Distance(mapDevice.Position) < 40)
                    .ToList();
                foreach (var obj in nearbyObjects)
                {
                    Console.WriteLine($"Type: {obj.GetType().Name}, Metadata: {obj.Metadata}");
                }

                Wait.Sleep(1000);
            }
            var nearbyPortals = ObjectManager.Objects
                .OfType<Portal>()
                .Where(p => p.Position.Distance(mapDevice.Position) < 40)
                .ToList();
            foreach (var portal in nearbyPortals)
            {
                Log.Debug($"Portal at {portal.Position}, Metadata: {portal.Metadata}");
            }
            Log.Debug($"Number of nearby portals: {nearbyPortals.Count}");



            return ObjectManager.Objects
                .Where(obj => obj.Position.Distance(mapDevice.Position) < 40)
                .Closest<Portal>(x => x.Metadata.ContainsIgnorecase("Metadata/MiscellaneousObjects/MultiplexPortal"));
        }
        private async Task<bool> WaitForPortalDespawn(Portal oldPortal)
        {
            return await Wait.For(() => !oldPortal.Fresh<Portal>().IsTargetable,
                "old map portals despawning", 200, 10000);
        }
        public static async Task<bool> TakeMapPortal(Portal portal, int attempts = 3)
        {
            if (portal == null || !portal.IsTargetable)
            {
                Log.Error($"[OpenMapTask] Portal is null or not targetable. Obj null?{portal==null}");
                return false;
            }
            int i = 1;
            while (i <= attempts)
            {
                if (!LokiPoe.IsInGame || World.CurrentArea.IsMap)
                {
                    return true;
                }
                await Coroutines.CloseBlockingWindows();
                Log.Debug($"[OpenMapTask] Take portal to map attempt: {i}/{attempts}");
                if (await PlayerAction.TakePortal(portal))
                {
                    if(TakeMapTask.Executed)
                    { TakeMapTask.Executed = false; }
                    return true;
                }
                await Wait.SleepSafe(1000);
                int num = i + 1;
                i = num;
            }
            return false;
        }
        #endregion
    }
}
