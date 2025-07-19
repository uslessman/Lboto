
using System.Collections.Generic;
using System.Threading.Tasks;
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Game.Objects;
using DreamPoeBot.Loki.Common;
using Cursor = DreamPoeBot.Loki.Game.LokiPoe.InGameState.CursorItemOverlay;
using InventoryUi = DreamPoeBot.Loki.Game.LokiPoe.InGameState.InventoryUi;
using StashUi = DreamPoeBot.Loki.Game.LokiPoe.InGameState.StashUi;
using Lboto.Helpers.Positions;

namespace Lboto.Helpers
{
    public static class Inventories
    {
        private static readonly log4net.ILog Log = Logger.GetLoggerInstanceForType();

        public static List<Item> InventoryItems => LokiPoe.InstanceInfo.GetPlayerInventoryItemsBySlot(InventorySlot.Main);

        public static int AvailableInventorySquares => LokiPoe.InstanceInfo.GetPlayerInventoryBySlot(InventorySlot.Main).AvailableInventorySquares;

        public static async Task<bool> OpenStash()
        {
            if (StashUi.IsOpened)
                return true;

            WalkablePosition stashPos;

            if (LokiPoe.CurrentWorldArea?.IsTown == true)
            {
                stashPos = StaticPositions.GetStashPosByAct();
            }
            else
            {
                var stashObj = LokiPoe.ObjectManager.Stash;
                if (stashObj == null)
                {
                    Log.Error("[Inventories] Failed to find Stash nearby.");
                    return false;
                }
                stashPos = stashObj.WalkablePosition();
            }

            await PlayerAction.EnableAlwaysHighlight();
            await stashPos.ComeAtOnce(35);

            if (!await PlayerAction.Interact(LokiPoe.ObjectManager.Stash, () => StashUi.IsOpened && StashUi.StashTabInfo != null, "opening stash"))
                return false;

            await Wait.SleepSafe(LokiPoe.Random.Next(200, 400));
            return true;
        }

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
    }
}
