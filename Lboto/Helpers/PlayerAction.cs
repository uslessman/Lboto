using DreamPoeBot.Common;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.Objects;
using DreamPoeBot.Loki.Game.GameData;
using System.Threading.Tasks;

namespace Lboto.Helpers
{

    public static class PlayerAction
    {
        private static readonly log4net.ILog Log = Logger.GetLoggerInstanceForType();

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
    }
}
