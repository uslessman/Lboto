
namespace Lboto.Helpers
{
    public enum TaskPosition
    {
        Before,
        After,
        Replace
    }

    public enum TransitionType
    {
        Regular,
        Local,
        Vaal,
        Master,
        Trial,
        Incursion,
        Synthesis,
        Syndicate,
        Conqueror
    }

    public enum WithdrawResult
    {
        Success,
        Error,
        Unavailable,
    }
    public enum PauseTypeEnum
    {
        StashPauseProbability,
        TownMovePause,
        OtherPauseType // Добавь другие типы пауз, если они есть
    }

}
