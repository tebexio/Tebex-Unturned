using Tebex.Adapters;

namespace Tebex.Shared.Components
{
    public interface ICommand
    {
         BaseTebexAdapter _adapter { get; }
    }
}