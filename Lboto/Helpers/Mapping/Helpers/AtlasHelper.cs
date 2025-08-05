using DreamPoeBot.Common;
using DreamPoeBot.Loki;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Game.Objects;
using log4net;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DreamPoeBot.Loki.Game.LokiPoe;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState.AtlasUi;
using static DreamPoeBot.Loki.Game.LokiPoe.InstanceInfo;


namespace Lboto.Helpers.Mapping
{
    public static class AtlasHelper
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();

        // <summary>
        /// Список оставшихся использований секстантов на каждом воидстоуне
        /// </summary>
        //private static readonly List<int> sextantUsesRemaining;

        /// <summary>
        /// Кэшированное значение информации о регионах атласа
        /// </summary>
        //public static PerFramesCachedValue<List<RegionInfo>> RegionCache;

        /// <summary>
        /// Проверяет, присутствует ли босс атласа на текущей карте
        /// </summary>
        public static bool IsAtlasBossPresent =>
            LocalData.MapMods.ContainsKey((StatTypeGGG)13845) ||
            LocalData.MapMods.ContainsKey((StatTypeGGG)6548);

        /// <summary>
        /// Проверяет, была ли карта засвидетельствована Мейвен
        /// </summary>
        /// <param name="mapName">Название карты</param>
        /// <returns>True, если карта была засвидетельствована</returns>
        public static bool HasMavenWitnessedMap(string mapName)
        {
            return Atlas.AreasWhereMavenHoldARecreation.Any(mapWrapper => mapWrapper.Name == mapName);
        }

        /// <summary>
        /// Открывает интерфейс атласа
        /// </summary>
        /// <returns>True, если интерфейс успешно открыт</returns>
        public static async Task<bool> OpenAtlasUi()
        {
            if (!AtlasUi.IsOpened)
            {
                Log.Info("[OpenAtlasUi] Открываем интерфейс Атласа.");
                Input.SimulateKeyEvent(Input.Binding.open_atlas_screen, true, false, false, Keys.None);

                if (!(await Wait.For(() => AtlasUi.IsOpened, "atlas ui opening")))
                {
                    Log.Error("Не удалось открыть интерфейс Атласа.");
                    return false;
                }

                await Wait.Sleep(20);
                return true;
            }
            return true;
        }

        /// <summary>
        /// Закрывает интерфейс атласа
        /// </summary>
        /// <returns>True, если интерфейс успешно закрыт</returns>
        public static async Task<bool> CloseAtlasUi()
        {
            if (AtlasUi.IsOpened)
            {
                Log.Info("[CloseAtlasUi] Закрываем интерфейс Атласа.");
                Input.SimulateKeyEvent(Keys.Escape, true, false, false, Keys.None);

                if (!(await Wait.For(() => !AtlasUi.IsOpened, "atlas ui closing")))
                {
                    Log.Error("Не удалось закрыть интерфейс Атласа.");
                    return false;
                }
                return true;
            }
            return true;
        }

        /// <summary>
        /// Накладывает секстанты на все воидстоуны
        /// </summary>
        /// <returns>True, если операция прошла успешно</returns>
        //public static async Task<bool> RollSextantsOnVoidstones()
        //{
        //    bool foundElevatedSextant = false;
        //    int errorRetryCount = 0;

        //    // Проверяем, включено ли использование секстантов в настройках
        //    if (!GeneralSettings.Instance.UseAwakenedSextants && !GeneralSettings.Instance.UseElevatedSextants)
        //    {
        //        return false;
        //    }

        //    // Уменьшаем счетчик использований для всех воидстоунов
        //    for (int i = 0; i < sextantUsesRemaining.Count; i++)
        //    {
        //        if (sextantUsesRemaining[i] > 0)
        //        {
        //            sextantUsesRemaining[i]--;
        //        }
        //    }

        //    // Проверяем, есть ли воидстоуны без секстантов
        //    bool hasEmptyVoidstone = false;
        //    foreach (int usesLeft in sextantUsesRemaining)
        //    {
        //        if (usesLeft == 0)
        //        {
        //            hasEmptyVoidstone = true;
        //            break;
        //        }
        //    }

        //    // Если на всех воидстоунах есть секстанты, выходим
        //    if (sextantUsesRemaining.Any() && !hasEmptyVoidstone)
        //    {
        //        return true;
        //    }

        //    // Основной цикл наложения секстантов
        //    while (true)
        //    {
        //        errorRetryCount++;
        //        if (errorRetryCount > 5)
        //        {
        //            return false;
        //        }

        //        // Берем секстанты из хранилища
        //        if (GeneralSettings.Instance.UseElevatedSextants && await TakeSextantFromStash(false))
        //        {
        //            foundElevatedSextant = true;
        //        }

        //        if (GeneralSettings.Instance.UseAwakenedSextants && !foundElevatedSextant &&
        //            !(await TakeSextantFromStash(true)))
        //        {
        //            Log.Info("[RollSextantsOnVoidstones] Секстанты не найдены в хранилище.");
        //            return false;
        //        }

        //        await OpenAtlasUi();

        //        // Обрабатываем каждый воидстоун
        //        bool rollResult = true;

        //        // Grasping Voidstone
        //        bool hasGraspingVoidstone = (RemoteMemoryObject)(object)AtlasUi.GraspingVoidstone.Item != (RemoteMemoryObject)null;
        //        if (hasGraspingVoidstone)
        //        {
        //            rollResult = await RollVoidstone("Grasping Voidstone");
        //            if (!rollResult) continue;
        //        }

        //        // Decayed Voidstone
        //        bool hasDecayedVoidstone = (RemoteMemoryObject)(object)AtlasUi.DecayedVoidstone.Item != (RemoteMemoryObject)null;
        //        if (hasDecayedVoidstone)
        //        {
        //            rollResult = await RollVoidstone("Decayed Voidstone");
        //            if (!rollResult) continue;
        //        }

        //        // Omniscient Voidstone
        //        bool hasOmniscientVoidstone = (RemoteMemoryObject)(object)AtlasUi.OmniscientVoidstone.Item != (RemoteMemoryObject)null;
        //        if (hasOmniscientVoidstone)
        //        {
        //            rollResult = await RollVoidstone("Omniscient Voidstone");
        //            if (!rollResult) continue;
        //        }

        //        // Ceremonial Voidstone
        //        bool hasCeremonialVoidstone = (RemoteMemoryObject)(object)AtlasUi.CerimonialVoidstone.Item != (RemoteMemoryObject)null;
        //        if (hasCeremonialVoidstone)
        //        {
        //            rollResult = await RollVoidstone("Ceremonial Voidstone");
        //            if (!rollResult) continue;
        //        }

        //        // Если все воидстоуны успешно обработаны, выходим из цикла
        //        break;
        //    }

        //    // Очищаем список и заново запоминаем количество использований
        //    sextantUsesRemaining.Clear();

        //    if ((RemoteMemoryObject)(object)AtlasUi.GraspingVoidstone.Item != (RemoteMemoryObject)null)
        //    {
        //        RememberSextantUsesAmount("Grasping Voidstone");
        //    }
        //    if ((RemoteMemoryObject)(object)AtlasUi.DecayedVoidstone.Item != (RemoteMemoryObject)null)
        //    {
        //        RememberSextantUsesAmount("Decayed Voidstone");
        //    }
        //    if ((RemoteMemoryObject)(object)AtlasUi.OmniscientVoidstone.Item != (RemoteMemoryObject)null)
        //    {
        //        RememberSextantUsesAmount("Omniscient Voidstone");
        //    }
        //    if ((RemoteMemoryObject)(object)AtlasUi.CerimonialVoidstone.Item != (RemoteMemoryObject)null)
        //    {
        //        RememberSextantUsesAmount("Ceremonial Voidstone");
        //    }

        //    // Возвращаем предметы в хранилище
        //    ITask stashTask = ((TaskManagerBase<ITask>)(object)MapBotEx.BotTaskManager).GetTaskByName("StashTask");
        //    await Coroutines.CloseBlockingWindows();
        //    await stashTask.Run();

        //    return true;
        //}

        /// <summary>
        /// Накладывает секстант на конкретный воидстоун
        /// </summary>
        /// <param name="voidstoneName">Название воидстоуна</param>
        /// <returns>True, если секстант успешно наложен</returns>
        //private static async Task<bool> RollVoidstone(string voidstoneName)
        //{
        //    int retryCount = 0;
        //    Voidstone targetVoidstone = GetVoidstoneByName(voidstoneName);

        //    // Открываем необходимые интерфейсы
        //    while (true)
        //    {
        //        if (retryCount > 3)
        //        {
        //            return false;
        //        }

        //        if (!(await OpenAtlasUi()))
        //        {
        //            await Coroutines.LatencyWait();
        //            retryCount++;
        //            continue;
        //        }

        //        if (await Inventories.OpenInventory())
        //        {
        //            break;
        //        }

        //        await Coroutines.LatencyWait();
        //        retryCount++;
        //    }

        //    // Основной цикл наложения секстанта
        //    while (true)
        //    {
        //        // Проверяем, нужно ли сохранить текущий мод
        //        if (ShouldKeepCurrentSextantMod(targetVoidstone))
        //        {
        //            Log.Info($"[RollVoidstone] {voidstoneName} успешно прокачан.");
        //            return true;
        //        }

        //        // Ищем секстант в инвентаре
        //        Item sextantItem = InventoryUi.InventoryControl_Main.Inventory.Items
        //            .FirstOrDefault(item => item.Name == "Awakened Sextant" || item.Name == "Elevated Sextant");

        //        if ((RemoteMemoryObject)(object)sextantItem == (RemoteMemoryObject)null)
        //        {
        //            break;
        //        }

        //        // Используем секстант на воидстоуне
        //        await InventoryUi.InventoryControl_Main.PickItemToCursor(sextantItem.LocationTopLeft, rightClick: true);
        //        await Coroutines.ReactionWait();
        //        await Coroutines.LatencyWait();

        //        targetVoidstone.LeftClick();
        //        await Coroutines.ReactionWait();
        //        await Coroutines.LatencyWait();
        //    }

        //    Log.Info("[RollVoidstone] Секстанты в инвентаре закончились. Идем пополнять запас.");
        //    return false;
        //}

        /// <summary>
        /// Получает объект воидстоуна по его названию
        /// </summary>
        /// <param name="voidstoneName">Название воидстоуна</param>
        /// <returns>Объект воидстоуна</returns>
        private static Voidstone GetVoidstoneByName(string voidstoneName)
        {
            switch (voidstoneName)
            {
                case "Grasping Voidstone":
                    return AtlasUi.GraspingVoidstone;
                case "Omniscient Voidstone":
                    return AtlasUi.OmniscientVoidstone;
                case "Ceremonial Voidstone":
                    return AtlasUi.CerimonialVoidstone;
                case "Decayed Voidstone":
                    return AtlasUi.DecayedVoidstone;
                default:
                    return AtlasUi.GraspingVoidstone;
            }
        }

        /// <summary>
        /// Проверяет, нужно ли сохранить текущий мод секстанта
        /// </summary>
        /// <param name="voidstone">Воидстоун для проверки</param>
        /// <returns>True, если мод нужно сохранить</returns>
        //private static bool ShouldKeepCurrentSextantMod(Voidstone voidstone)
        //{
        //    // Если список модов для сохранения пуст, сохраняем любой мод
        //    if (GeneralSettings.Instance.SextantModsToSave == null ||
        //        GeneralSettings.Instance.SextantModsToSave.Count < 1)
        //    {
        //        return voidstone.Item.Stats.Any();
        //    }

        //    // Проверяем, есть ли желаемые моды
        //    foreach (NameEntry desiredMod in GeneralSettings.Instance.SextantModsToSave)
        //    {
        //        StatTypeGGG matchingStatKey = voidstone.Item.Stats.FirstOrDefault(statPair =>
        //        {
        //            StatTypeGGG statKey = statPair.Key;
        //            return ((object)(StatTypeGGG)(ref statKey)).ToString() == desiredMod.Name;
        //        }).Key;

        //        if (((object)(StatTypeGGG)(ref matchingStatKey)).ToString() == desiredMod.Name)
        //        {
        //            Log.Info($"[RollVoidstone] Найден желаемый мод: {desiredMod.Name}");
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        /// <summary>
        /// Берет секстанты из хранилища в инвентарь
        /// </summary>
        /// <param name="useAwakenedSextant">True для Awakened Sextant, false для Elevated Sextant</param>
        /// <returns>True, если секстанты успешно взяты</returns>
        //private static async Task<bool> TakeSextantFromStash(bool useAwakenedSextant = false)
        //{
        //    string sextantName = useAwakenedSextant ? "Awakened Sextant" : "Elevated Sextant";

        //    // Проверяем, есть ли уже секстанты в инвентаре
        //    if (InventoryUi.InventoryControl_Main.Inventory.Items.All(item => item.Name != sextantName))
        //    {
        //        int retryCount = 0;

        //        while (retryCount <= 3)
        //        {
        //            if (await Inventories.OpenStash())
        //            {
        //                if (await Inventories.FindTabWithCurrency(sextantName))
        //                {
        //                    // Обработка валютной вкладки
        //                    if ((int)StashUi.StashTabInfo.TabType == 3)
        //                    {
        //                        InventoryControlWrapper currencyControl = (from control in Inventories.GetControlsWithCurrency(sextantName)
        //                                                                   orderby control.CustomTabItem.StackCount descending
        //                                                                   select control).FirstOrDefault();

        //                        if ((RemoteMemoryObject)(object)currencyControl == (RemoteMemoryObject)null)
        //                        {
        //                            Log.Info($"Секстанты {sextantName} не найдены в хранилище");
        //                            return false;
        //                        }

        //                        if (!(await Inventories.FastMoveCustomTabItem(currencyControl)))
        //                        {
        //                            //ErrorManager.ReportError();
        //                            await Coroutines.LatencyWait();
        //                            retryCount++;
        //                            continue;
        //                        }
        //                    }
        //                    // Обработка обычной вкладки
        //                    else
        //                    {
        //                        Item sextantInStash = StashUi.InventoryControl.Inventory.Items
        //                            .FirstOrDefault(item => item.Name == sextantName);

        //                        if ((RemoteMemoryObject)(object)sextantInStash == (RemoteMemoryObject)null)
        //                        {
        //                            Log.Info($"Секстанты {sextantName} не найдены в хранилище");
        //                            return false;
        //                        }

        //                        if (!(await Inventories.FastMoveFromStashTab(sextantInStash.LocationTopLeft)))
        //                        {
        //                            await Coroutines.LatencyWait();
        //                            retryCount++;
        //                            continue;
        //                        }
        //                    }
        //                    return true;
        //                }
        //                return false;
        //            }

        //            await Coroutines.LatencyWait();
        //            retryCount++;
        //        }
        //        return false;
        //    }
        //    return true;
        //}

        /// <summary>
        /// Запоминает количество оставшихся использований секстанта на воидстоуне
        /// </summary>
        /// <param name="voidstoneName">Название воидстоуна</param>
        //private static void RememberSextantUsesAmount(string voidstoneName)
        //{
        //    Voidstone targetVoidstone = GetVoidstoneByName(voidstoneName);

        //    int remainingUses = targetVoidstone.Item.Stats.FirstOrDefault(statPair =>
        //    {
        //        StatTypeGGG statKey = statPair.Key;
        //        return ((object)(StatTypeGGG)(ref statKey)).ToString() == "SextantUsesRemaining";
        //    }).Value;

        //    sextantUsesRemaining.Add(remainingUses);
        //}

        /// <summary>
        /// Вставляет воидстоуны из инвентаря в соответствующие слоты атласа
        /// </summary>
        /// <returns>True, если операция прошла успешно</returns>
        public static async Task<bool> SocketVoidstones()
        {
            //if (!GeneralSettings.Instance.SocketVoidstonesFromInventory)
            //{
            //    return false;
            //}

            // Проверяем, есть ли воидстоуны в инвентаре
            if (InventoryUi.InventoryControl_Main.Inventory.Items
                .Any(item => item.Metadata.Contains("Metadata/Items/AtlasUpgrades")))
            {
                int retryCount = 0;

                // Открываем необходимые интерфейсы
                while (true)
                {
                    if (retryCount > 3)
                    {
                        return false;
                    }

                    if (!(await OpenAtlasUi()))
                    {
                        await Coroutines.LatencyWait();
                        retryCount++;
                        continue;
                    }

                    if (await Inventories.OpenInventory())
                    {
                        break;
                    }

                    await Coroutines.LatencyWait();
                    retryCount++;
                }

                // Вставляем каждый тип воидстоуна
                Item graspingVoidstone = InventoryUi.InventoryControl_Main.Inventory.Items
                    .FirstOrDefault(item => item.Name == "Grasping Voidstone");
                await ActuallySocketVoidstone(graspingVoidstone);

                Item decayedVoidstone = InventoryUi.InventoryControl_Main.Inventory.Items
                    .FirstOrDefault(item => item.Name == "Decayed Voidstone");
                await ActuallySocketVoidstone(decayedVoidstone);

                Item omniscientVoidstone = InventoryUi.InventoryControl_Main.Inventory.Items
                    .FirstOrDefault(item => item.Name == "Omniscient Voidstone");
                await ActuallySocketVoidstone(omniscientVoidstone);

                Item ceremonialVoidstone = InventoryUi.InventoryControl_Main.Inventory.Items
                    .FirstOrDefault(item => item.Name == "Ceremonial Voidstone");
                await ActuallySocketVoidstone(ceremonialVoidstone);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Выполняет фактическую вставку воидстоуна в слот
        /// </summary>
        /// <param name="voidstone">Воидстоун для вставки</param>
        private static async Task ActuallySocketVoidstone(Item voidstone)
        {
            if ((RemoteMemoryObject)(object)voidstone != (RemoteMemoryObject)null)
            {
                Log.Info($"[SocketVoidstones] Берем предмет в позиции {voidstone.LocationTopLeft.X}, {voidstone.LocationTopLeft.Y} из инвентаря");

                await Inventories.PickItemToCursor(
                InventoryUi.InventoryControl_Main,
                new Vector2i(voidstone.LocationTopLeft.X, voidstone.LocationTopLeft.Y));
                await Inventories.WaitForCursorToHaveItem();

                // Вставляем в любой доступный слот (используем CerimonialVoidstone как универсальный слот)
                AtlasUi.CerimonialVoidstone.LeftClick();
                await Inventories.WaitForCursorToBeEmpty();
            }
        }

        /// <summary>
        /// Статический конструктор для инициализации статических полей
        /// </summary>
        //static AtlasHelper()
        //{
        //    sextantUsesRemaining = new List<int>();
        //    RegionCache = new PerFramesCachedValue<List<RegionInfo>>(
        //        () => Atlas.RegionsInfo,
        //        60 // Кэшируем на 60 кадров
        //    );
        //}
    }
}
