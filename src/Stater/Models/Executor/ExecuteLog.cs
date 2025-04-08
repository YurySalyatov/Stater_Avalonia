namespace Stater.Models.Executor;



public record ExecuteLog(
    string Text,
    int Tab,
    ExecuteLog.ExecuteLogStatusEnum ExecuteLogStatus
)
{
    public enum ExecuteLogStatusEnum
    {
        Info, Warning, Error
    }
};