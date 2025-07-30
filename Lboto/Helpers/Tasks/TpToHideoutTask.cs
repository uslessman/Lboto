using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Coroutine;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.Objects;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lboto.Helpers.Tasks
{
    public static class TpToHideoutTask
    {

        private static readonly log4net.ILog Log = Logger.GetLoggerInstanceForType();

        public static async Task<bool> GoToHideout()
        {
            var area = LokiPoe.CurrentWorldArea;
            if (area.IsTown || area.IsMenagerieArea)
            {
                return await GoToHideoutViaCommand();
            }
            else
            {
                return await GoToHideoutViaWaypoint();
            }
        }

        private static async Task<bool> GoToHideoutViaCommand()
        {
            Log.Debug("[HideoutHelper] Trying /hideout command.");

            var areaHash = LokiPoe.LocalData.AreaHash;
            var result = LokiPoe.InGameState.ChatPanel.Commands.hideout();

            if (result != LokiPoe.InGameState.ChatResult.None)
            {
                Log.Error($"[HideoutHelper] /hideout failed with error: {result}");
                return false;
            }

            if (!await Wait.ForHOChange())
            {
                Log.Error("[HideoutHelper] Area change did not happen after /hideout.");
                return false;
            }

            Log.Debug("[HideoutHelper] Entered hideout via chat.");
            return true;
        }

        private static async Task<bool> GoToHideoutViaWaypoint()
        {
            Log.Debug("[HideoutHelper] Trying to open waypoint UI.");
            if (!LokiPoe.InGameState.WorldUi.IsOpened)
            {
                if (!await OpenWaypoint())
                {
                    Log.Error("[HideoutHelper] Failed to open waypoint.");
                    return false;
                }
            }

            var result = LokiPoe.InGameState.WorldUi.GoToHideout();
            if (result != LokiPoe.InGameState.TakeWaypointResult.None)
            {
                Log.Error($"[HideoutHelper] Waypoint to hideout failed with error: {result}");
                return false;
            }

            if (!await Wait.ForHOChange())
            {
                Log.Error("[HideoutHelper] Area change did not happen after waypoint.");
                return false;
            }

            Log.Debug("[HideoutHelper] Entered hideout via waypoint.");
            return true;
        }

        private static async Task<bool> OpenWaypoint()
        {
            var waypoint = LokiPoe.ObjectManager.GetObjectsByType<AreaTransition>()
                .FirstOrDefault(x => x.Metadata == "Metadata/MiscellaneousObject/Waypoint");

            if (waypoint == null)
            {
                Log.Error("[HideoutHelper] No waypoint found in area.");
                return false;
            }

            Log.Debug("[HideoutHelper] Found waypoint, interacting...");
            return await Coroutines.InteractWith(waypoint);
        }
    }
}
