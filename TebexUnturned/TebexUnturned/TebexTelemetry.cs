namespace Tebex.Triage
{
    /// <summary>
    /// TebexTelemetry is a container class for storing information about the current runtime software and version.
    /// </summary>
    public class TebexTelemetry
    {
        private string _serverSoftware;
        private string _serverVersion;
        private string _runtimeVersion;

        public TebexTelemetry(string serverSoftware, string serverVersion, string runtimeVersion)
        {
            _serverSoftware = serverSoftware;
            _serverVersion = serverVersion;
            _runtimeVersion = runtimeVersion;
        }
    
        public string GetServerSoftware()
        {
            return _serverSoftware;
        }

        public string GetRuntimeVersion()
        {
            return _runtimeVersion;
        }

        public string GetServerVersion()
        {
            return _serverVersion;
        }
    }   
}