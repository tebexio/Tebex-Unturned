
namespace Tebex.Triage
{
    /// <summary>
    /// TebexPlatform is a container class for the current plugin version and telemetry information about the current runtime.
    /// </summary>
    public class TebexPlatform
    {
        private string _pluginVersion;
        private TebexTelemetry _telemetry;
        public TebexPlatform(string pluginVersion, TebexTelemetry _telemetry)
        {
            this._pluginVersion = pluginVersion;
            this._telemetry = _telemetry;

        }
    
        public TebexTelemetry GetTelemetry()
        {
            return _telemetry;
        }

        public string GetPluginVersion()
        {
            return _pluginVersion;
        }
    }    
}
