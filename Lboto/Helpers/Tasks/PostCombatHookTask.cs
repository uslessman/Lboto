using System.Threading.Tasks;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;

namespace Lboto.Helpers.Tasks
{
    public class PostCombatHookTask : ITask
    {
        public const string MessageId = "hook_post_combat";
        #region temp
        private bool TempShouldKillSetting = true;
        #endregion
        private static readonly log4net.ILog Log = Logger.GetLoggerInstanceForType();

        public async Task<bool> Run()
        {
            if (!TempShouldKillSetting) return false;
            foreach (var plugin in PluginManager.EnabledPlugins)
            {
                if (await plugin.Logic(new Logic(MessageId, this)) == LogicResult.Provided)
                {
                    Log.Info($"[PostCombatHookTask] \"{plugin.Name}\" returned true.");
                    return true;
                }
            }
            foreach (var content in ContentManager.Contents)
            {
                if (await content.Logic(new Logic(MessageId, this)) == LogicResult.Provided)
                {
                    Log.Info($"[PostCombatHookTask] \"{content.Name}\" returned true.");
                    return true;
                }
            }
            return false;
        }

        #region Unused interface methods

        public MessageResult Message(Message message)
        {
            return MessageResult.Unprocessed;
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public void Start()
        {
        }

        public void Tick()
        {
        }

        public void Stop()
        {
        }

        public string Name => "PostCombatHookTask";
        public string Description => "This task provides a coroutine hook for executing user logic after combat has completed.";
        public string Author => "NotYourFriend original from EXVault";
        public string Version => "1.0";

        #endregion
    }
}
