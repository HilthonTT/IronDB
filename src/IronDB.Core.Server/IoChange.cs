using IronDB.Core.Server.Meters;

namespace IronDB.Core.Server;

public sealed class IoChange
{
    public string Key => $"{MeterItem.Type}/{FileName}";

    public string FileName { get; set; } = string.Empty;

    public IoMeterBuffer.MeterItem MeterItem { get; set; } = default!;

    public sealed class IoChangesNotifications
    {
        public event Action<IoChange>? OnIoChange;
        public bool DisableToMetrics;

        public void RaiseNotifications(string fileName, IoMeterBuffer.MeterItem meterItem)
        {
            OnIoChange?.Invoke(new IoChange
            {
                FileName = fileName,
                MeterItem = meterItem
            });
        }
    }
}
