using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Bot.Pathfinding;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Coroutine;
using DreamPoeBot.Loki.Game;
using Lboto.Helpers;
using Lboto.Helpers.Global;
using Lboto.Helpers.Tasks;
using log4net;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using static DreamPoeBot.Loki.Game.LokiPoe;

namespace Lboto
{
    public class Lboto : IBot
    {
        private LbotoGui _gui;
        private readonly TaskManager _taskManager = new TaskManager();
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();
        private static int _lastBoundMoveSkillSlot = -1;
        internal static int LastBoundMoveSkillSlot
        {
            get
            {
                if (_lastBoundMoveSkillSlot == -1)
                    _lastBoundMoveSkillSlot = LokiPoe.InGameState.SkillBarHud.LastBoundMoveSkill.Slot;
                return _lastBoundMoveSkillSlot;
            }
        }
        private Coroutine _coroutine;


        public void Deinitialize()
        {
            
        }

        public void Initialize()
        {
            
            
            if (!LbotoSettings.Instance.MapTabs.Contains("~price 333 divine"))
            {
                LbotoSettings.Instance.MapTabs.Add("~price 333 divine");
            }
            if (!LbotoSettings.Instance.MapTabs.Contains("Coin"))
            {
                LbotoSettings.Instance.CurrencyTabs.Add("Coin");
            }
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return await _taskManager.ProvideLogic(TaskGroup.Enabled, RunBehavior.UntilHandled, logic);
        }

        public MessageResult Message(Message message)
        {
            var handled = false;
            var id = message.Id;
            switch (message.Id)
            {
                case "GetTaskManager":
                    message.AddOutput<TaskManager>((IMessageHandler)(object)this, _taskManager, "");
                    handled = true;
                    break;
            }
                    return handled ? MessageResult.Processed : MessageResult.Unprocessed;
        }

        public void Start()
        {
            Explorer.CurrentDelegate = user => CombatAreaCache.Current.Explorer.BasicExplorer;

            //ComplexExplorer.ResetSettingsProviders();
            //ComplexExplorer.AddSettingsProvider("FollowBot", MapBotExploration, ProviderPriority.Low); Need to configure

            // Cache all bound keys.
            LokiPoe.Input.Binding.Update();

            // Reset the default MsBetweenTicks on start.
            BotManager.MsBetweenTicks = 40;
            Log.Debug($"[Start] MsBetweenTicks: {BotManager.MsBetweenTicks}.");
            Log.Debug($"[Start] PlayerMover.Instance: {PlayerMoverManager.Current.GetType()}.");
            Log.Debug($"[Start] NetworkingMode: {ConfigManager.NetworkingMode}.");
            Log.Debug($"[Start] KeyPickup: {ConfigManager.KeyPickup}.");


            // Since this bot will be performing client actions, we need to enable the process hook manager.
            LokiPoe.ProcessHookManager.Enable();

            _coroutine = null;

            ExilePather.BlockLockedDoors = FeatureEnum.Disabled;
            ExilePather.BlockLockedTempleDoors = FeatureEnum.Disabled;
            ExilePather.BlockTrialOfAscendancy = FeatureEnum.Disabled;

            ExilePather.Reload();

            _taskManager.Reset();

            AddTasks();

            PluginManager.Start();
            RoutineManager.Start();
            _taskManager.Start();

            foreach (var plugin in PluginManager.EnabledPlugins)
            {
                Log.Debug($"[Start] The plugin {plugin.Name} is enabled.");
            }

            Log.Debug($"[Start] PlayerMover.Instance: {PlayerMoverManager.Current.GetType()}.");
        }

        public void Stop()
        {
            _taskManager.Stop();
            PluginManager.Stop();
            RoutineManager.Stop();

            // When the bot is stopped, we want to remove the process hook manager.
            LokiPoe.ProcessHookManager.Disable();

            // Cleanup the coroutine.
            if (_coroutine != null)
            {
                _coroutine.Dispose();
                _coroutine = null;
            }
        }

        public void Tick()
        {
            Events.Tick();
            CombatAreaCache.Tick();
            foreach (var _task in _taskManager.TaskList)
            {
                Log.Debug(_task.Name);
            }
            _taskManager.Tick();
            PluginManager.Tick();
            RoutineManager.Tick();
        }

        public TaskManager GetTaskManager()
        {
            return _taskManager;
        }
        private void AddTasks()
        {

            _taskManager.Add(new ClearCursorTask());
            //_taskManager.Add(new DefenseAndFlaskTask());
            //_taskManager.Add(new LootItemTask());
            //_taskManager.Add(new PreCombatFollowTask());
            _taskManager.Add(new TakeMapTask());
            _taskManager.Add(new CombatTask(50));
            _taskManager.Add(new PostCombatHookTask());
            //_taskManager.Add(new LevelGemsTask());
            //_taskManager.Add(new CombatTask(-1));
            //_taskManager.Add(new CastAuraTask());
            //_taskManager.Add(new TravelToPartyZoneTask());
            //_taskManager.Add(new FollowTask());
            //_taskManager.Add(new OpenWaypointTask());
            //_taskManager.Add(new JoinPartyTask());
            _taskManager.Add(new FallbackTask());
        }
        public string Author => "Lenson";

        public string Description => "Base Bot";

        public string Name => "Lbot";

        public string Version => "0.0.0.1";

        public UserControl Control => _gui ?? (_gui = new LbotoGui());

        public JsonSettings Settings => LbotoSettings.Instance;

        public override string ToString() => $"{Name}: {Description}";
    }
}
