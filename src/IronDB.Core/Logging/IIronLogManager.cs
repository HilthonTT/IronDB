namespace IronDB.Core.Logging;

public interface IIronLogManager
{
    IIronLogger GetLogger(string name);

    event EventHandler<IronLoggingConfigurationChangedEventArgs> ConfigurationChanged;

    void Shutdown();
}
