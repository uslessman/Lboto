using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Common;

namespace Lboto.Helpers.Tasks
{
    public class ClearCursorTask : ITask
    {
        private static readonly log4net.ILog Log = Logger.GetLoggerInstanceForType();

        public async Task<bool> Run()
        {
            var mode = LokiPoe.InGameState.CursorItemOverlay.Mode;

            if (mode == LokiPoe.InGameState.CursorItemModes.VirtualMove ||
                mode == LokiPoe.InGameState.CursorItemModes.VirtualUse)
            {
                Log.Error("[ClearCursorTask] Virtual item detected on cursor. Pressing Escape to clear it.");
                LokiPoe.Input.SimulateKeyEvent(Keys.Escape, true, false, false);
                await Wait.LatencySleep();
                await Wait.ArtificialDelay();
                return true;
            }

            if (mode == LokiPoe.InGameState.CursorItemModes.None)
                return false;

            var cursorItem = LokiPoe.InGameState.CursorItemOverlay.Item;
            if (cursorItem == null)
            {
                Log.Error($"[ClearCursorTask] Cursor mode = \"{mode}\", but no item is present.");
                return true;
            }

            Log.Error($"[ClearCursorTask] \"{cursorItem.Name}\" is on cursor. Trying to place it into inventory.");

            if (!LokiPoe.InGameState.InventoryUi.IsOpened)
            {
                LokiPoe.Input.SimulateKeyEvent(LokiPoe.Input.Binding.open_inventory_panel, true, false, false);
                await Wait.LatencySleep();
            }

            var inventory = LokiPoe.InGameState.InventoryUi.InventoryControl_Main;
            var itemSize = LokiPoe.InGameState.CursorItemOverlay.ItemSize;

            if (!inventory.Inventory.CanFitItem(itemSize, out int col, out int row))
            {
                Log.Error("[ClearCursorTask] No space in inventory. Stopping the bot.");
                BotManager.Stop();
                return true;
            }

            if (!await inventory.PlaceItemFromCursor(new Vector2i(col, row)))
            {
                Log.Error("[ClearCursorTask] Failed to place item from cursor.");
            }

            await Wait.LatencySleep();
            await Wait.ArtificialDelay();

            return true;
        }

        public MessageResult Message(DreamPoeBot.Loki.Bot.Message message) => MessageResult.Unprocessed;

        public async Task<LogicResult> Logic(Logic logic) => LogicResult.Unprovided;

        public void Start() { }

        public void Tick() { }

        public void Stop() { }

        public string Name => "ClearCursorTask";
        public string Description => "Places any item left on the cursor into the inventory.";
        public string Author => "by OpenAI";
        public string Version => "1.0";
    }
}
