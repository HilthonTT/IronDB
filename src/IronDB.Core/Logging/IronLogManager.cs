namespace IronDB.Core.Logging;

public sealed class IronLogManager
{
    internal static bool GlobalIsAuditEnabled = false;
    public static readonly IronLogManager Instance = new();

    private static IIronLogManager _logManager = IronNullLogManager.Instance;

    public bool IsAuditEnabled;

    private IronLogManager()
    {
        Refresh();
    }

    internal void Refresh()
    {
        IIronLogger? innerLogger = _logManager.GetLogger("Audit");
        if (innerLogger is null)
        {
            return;
        }

        IsAuditEnabled = innerLogger.IsInfoEnabled;
        _logManager.ConfigurationChanged += (_, _) => IsAuditEnabled = GlobalIsAuditEnabled && innerLogger.IsInfoEnabled;
    }

    internal static void SetAudit(bool isEnabled)
    {
        GlobalIsAuditEnabled = isEnabled;
    }

    public static void Set(IIronLogManager logManager)
    {
        ArgumentNullException.ThrowIfNull(logManager, nameof(logManager));

        _logManager = logManager;
        Instance.Refresh();
    }
}
