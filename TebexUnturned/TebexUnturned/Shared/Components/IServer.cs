namespace Tebex.Shared.Components
{
    public interface IServer
    {
        string Command(string value);
        string Address { get; }
        string Version { get; }
        string Protocol { get; }
    }
}