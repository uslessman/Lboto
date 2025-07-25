using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Game.Objects;
using System.Linq;
using System.Threading.Tasks;

namespace Lboto.Helpers
{

    public static class PlayerAction
    {
        private static readonly log4net.ILog Log = Logger.GetLoggerInstanceForType();

        public static async Task<Portal> CreateTownPortal()
        {
            var portalSkill = LokiPoe.InGameState.SkillBarHud.Skills.FirstOrDefault(s => s.Name == "Portal" && s.IsOnSkillBar);
            if (portalSkill != null)
            {
                await Coroutines.FinishCurrentAction();
                await Wait.SleepSafe(100);
                var err = LokiPoe.InGameState.SkillBarHud.Use(portalSkill.Slot, false);
                if (err != LokiPoe.InGameState.UseResult.None)
                {
                    Log.Error($"[CreateTownPortal] Fail to cast portal skill. Error: \"{err}\".");
                    return null;
                }
                await Coroutines.FinishCurrentAction();
                await Wait.SleepSafe(100);
            }
            else
            {
                var portalScroll = Inventories.InventoryItems
                    .Where(i => i.Name == CurrencyNames.Portal)
                    .OrderBy(i => i.StackCount)
                    .FirstOrDefault();

                if (portalScroll == null)
                {
                    Log.Error("[CreateTownPortal] Out of portal scrolls.");
                    return null;
                }

                int itemId = portalScroll.LocalId;

                if (!await Inventories.OpenInventory())
                    return null;

                await Coroutines.FinishCurrentAction();
                await Wait.SleepSafe(100);

                var err = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.UseItem(itemId);
                if (err != UseItemResult.None)
                {
                    Log.Error($"[CreateTownPortal] Fail to use a Portal Scroll. Error: \"{err}\".");
                    return null;
                }

                await Wait.ArtificialDelay();

                await Coroutines.CloseBlockingWindows();
            }

            Portal portal = null;
            await Wait.For(() => (portal = PortalInRangeOf(40)) != null, "portal spawning");
            return portal;
        }

        private static Portal PortalInRangeOf(int range)
        {
            return LokiPoe.ObjectManager.Objects
                .Closest<Portal>(p => p.IsPlayerPortal() && p.Distance <= range && p.PathDistance() <= range + 3);
        }
        public static async Task<bool> OpenWaypoint()
        {
            var wp = LokiPoe.ObjectManager.Waypoint;
            if (wp == null)
            {
                Log.Error("[OpenWaypoint] No Waypoint nearby.");
                return false;
            }

            await EnableAlwaysHighlight();
            var pos = wp.WalkablePosition();
            await pos.ComeAtOnce();
            await Interact(wp, () => LokiPoe.InGameState.WorldUi.IsOpened || LokiPoe.InGameState.GlobalWarningDialog.IsBetrayalLeaveZoneWarningOverlayOpen, "Waypoint opening");

            if (LokiPoe.InGameState.GlobalWarningDialog.IsBetrayalLeaveZoneWarningOverlayOpen)
            {
                Log.Info("[OpenWaypoint] Betrayal overlay is open. Confirming dialog.");
                LokiPoe.InGameState.GlobalWarningDialog.ConfirmDialog();
                await Wait.LatencySleep();
            }

            await Wait.SleepSafe(200);
            return LokiPoe.InGameState.WorldUi.IsOpened;
        }
        public static async Task<bool> TakeWaypoint(AreaInfo area, bool newInstance = false)
        {
            if (!LokiPoe.InGameState.WorldUi.IsOpened)
            {
                if (!await OpenWaypoint())
                {
                    Log.Error("[TakeWaypoint] Fail to open a waypoint.");
                    return false;
                }
            }

            Log.Debug($"[TakeWaypoint] Now going to take a waypoint to {area}");

            var areaHash = LokiPoe.LocalData.AreaHash;

            var err = LokiPoe.InGameState.WorldUi.TakeWaypoint(area.Id, newInstance);
            if (err != LokiPoe.InGameState.TakeWaypointResult.None)
            {
                Log.Error($"[TakeWaypoint] Fail to take a waypoint to {area}. Error: \"{err}\".");
                return false;
            }
            return await Wait.ForAreaChange(areaHash);
        }
        public static async Task<bool> Interact(NetworkObject obj, System.Func<bool> success, string desc, int timeout = 3000, int attempts = 3)
        {
            if (obj == null)
            {
                Log.Error("[Interact] Object is null.");
                return false;
            }

            for (int i = 1; i <= attempts; i++)
            {
                Log.Debug($"[Interact] Attempt {i}/{attempts} with \"{obj.Name}\".");
                await Coroutines.CloseBlockingWindows();
                await Coroutines.FinishCurrentAction();
                await Wait.LatencySleep();

                if (await Coroutines.InteractWith(obj))
                {
                    return await Wait.For(success, desc, 100, timeout);
                }

                Log.Warn($"[Interact] Failed attempt {i} with \"{obj.Name}\".");
                await Wait.SleepSafe(200, 400);
            }

            Log.Error($"[Interact] All attempts failed with \"{obj.Name}\".");
            return false;
        }

        public static async Task<bool> Interact(NetworkObject obj, int attempts = 3)
        {
            if (obj == null)
            {
                Log.Error("[Interact] Object is null.");
                return false;
            }

            for (int i = 1; i <= attempts; i++)
            {
                Log.Debug($"[Interact] Attempt {i}/{attempts} with \"{obj.Name}\".");
                await Coroutines.CloseBlockingWindows();
                await Coroutines.FinishCurrentAction();
                await Wait.LatencySleep();

                if (await Coroutines.InteractWith(obj))
                    return true;

                Log.Warn($"[Interact] Failed attempt {i} with \"{obj.Name}\".");
                await Wait.SleepSafe(100, 200);
            }

            Log.Error($"[Interact] All attempts failed with \"{obj.Name}\".");
            return false;
        }

        public static async Task<bool> Logout()
        {
            Log.Debug("[Logout] Logging out.");
            var err = LokiPoe.EscapeState.LogoutToTitleScreen();
            if (err != LokiPoe.EscapeState.LogoutError.None)
            {
                Log.Error($"[Logout] Logout failed. Error: {err}");
                return false;
            }
            return await Wait.For(() => LokiPoe.IsInLoginScreen, "Logout", 500, 5000);
        }

        public static async Task EnableAlwaysHighlight()
        {
            if (LokiPoe.ConfigManager.IsAlwaysHighlightEnabled)
                return;

            Log.Info("[EnableAlwaysHighlight] Enabling always highlight.");
            LokiPoe.Input.SimulateKeyEvent(LokiPoe.Input.Binding.highlight_toggle, true, false, false);
            await Wait.For(() => LokiPoe.ConfigManager.IsAlwaysHighlightEnabled, "EnableAlwaysHighlight", 10, 100);
        }

        public static async Task DisableAlwaysHighlight()
        {
            if (!LokiPoe.ConfigManager.IsAlwaysHighlightEnabled)
                return;

            Log.Info("[DisableAlwaysHighlight] Disabling always highlight.");
            LokiPoe.Input.SimulateKeyEvent(LokiPoe.Input.Binding.highlight_toggle, true, false, false);
            await Wait.For(() => !LokiPoe.ConfigManager.IsAlwaysHighlightEnabled, "DisableAlwaysHighlight", 10, 100);
        }

        public static async Task<bool> TakeTransition(AreaTransition transition, bool newInstance = false)
        {
            if (transition == null)
            {
                Log.Error("[TakeTransition] Transition is null.");
                return false;
            }

            await DisableAlwaysHighlight();
            var pos = transition.WalkablePosition();
            Log.Debug($"[TakeTransition] Moving to \"{pos.Name}\".");

            await pos.ComeAtOnce();
            await Coroutines.FinishCurrentAction();
            await Wait.SleepSafe(100);

            var areaHash = LokiPoe.LocalData.AreaHash;

            if (!await Interact(transition))
                return false;

            if (transition.TransitionType == TransitionTypes.Local)
            {
                if (!await Wait.For(() => LokiPoe.Me.HasAura("Grace Period"), "Grace Period after transition", 500, 5000))
                {
                    Log.Error("[TakeTransition] Grace Period did not trigger after transition.");
                    return false;
                }
            }
            else
            {
                if (!await Wait.ForAreaChange(areaHash))
                    return false;
            }

            Log.Debug($"[TakeTransition] Successfully entered \"{pos.Name}\".");
            return true;
        }

        public static async Task<bool> TpToTown(bool forceNewPortal = false, bool repeatUntilInTown = true)
        {
            //if (ErrorManager.GetErrorCount("TpToTown") > 5)
            //{
            //    Log.Debug("[TpToTown] We failed to take a portal to town more than 5 times. Now going to log out.");
            //    return await Logout();
            //}
            Log.Debug("[TpToTown] Now going to open and take a portal to town.");

            var area = World.CurrentArea;

            if (area.IsTown || area.IsHideoutArea)
            {
                Log.Error("[TpToTown] We are already in town/hideout.");
                return false;
            }
            if (!area.IsOverworldArea && !area.IsMap && !area.IsCorruptedArea && !area.IsMapRoom && !area.IsTempleOfAtzoatl && area.Name != "Syndicate Hideout")
            {
                Log.Warn($"[TpToTown] Cannot create portals in this area ({area.Name}). Now going to log out.");
                return await Logout();
            }

            Portal portal;

            if (forceNewPortal || (portal = PortalInRangeOf(70)) == null)
            {
                portal = await CreateTownPortal();
                if (portal == null)
                {
                    Log.Error("[TpToTown] Fail to create a new town portal. Now going to log out.");
                    return await Logout();
                }
            }
            else
            {
                Log.Debug($"[TpToTown] There is a ready-to-use portal at a distance of {portal.Distance}. Now going to take it.");
            }

            if (!await TakePortal(portal))
            {
                Log.Error("[TpToTown] Failed taking portal.");
                return false;
            }

            var newArea = World.CurrentArea;
            if (repeatUntilInTown && newArea.IsCombatArea)
            {
                Log.Debug($"[TpToTown] After taking a portal we appeared in another combat area ({newArea.Name}). Now calling TpToTown again.");
                return await TpToTown(forceNewPortal);
            }
            Log.Debug($"[TpToTown] We have been successfully teleported from \"{area.Name}\" to \"{newArea.Name}\".");
            return true;

            //while (!LokiPoe.InGameState.IsRightPanelShown)
            //{
            //    LokiPoe.Input.SimulateKeyEvent(Keys.I, true, false, false);
            //    await Coroutine.Coroutine.Sleep(16);
            //}
            //return true;
        }

        public static async Task<bool> TakePortal(Portal portal)
        {
            if (portal == null)
            {
                Log.Error("[TakePortal] Portal object is null.");
                return false;
            }

            var pos = portal.WalkablePosition();
            await DisableAlwaysHighlight();
            await pos.ComeAtOnce();
            await Wait.SleepSafe(200);

            Log.Debug($"[TakePortal] Now going to take portal to \"{pos.Name}\".");

            var hash = LokiPoe.LocalData.AreaHash;

            if (!LokiPoe.Input.HighlightObject(portal))
                return false;

            if (!await Interact(portal,
            () => !LokiPoe.IsInGame, "loading screen"))
                return false;

            if (!await Wait.ForAreaChange(hash))
                return false;

            Log.Debug($"[TakePortal] Portal to \"{pos.Name}\" has been successfully taken.");
            return true;
        }
    }
}
