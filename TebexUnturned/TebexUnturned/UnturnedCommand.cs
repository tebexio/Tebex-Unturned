using System.Collections.Generic;
using Rocket.API;
using Tebex.Adapters;
using Tebex.Shared.Components;

namespace TebexUnturned
{
    public abstract class UnturnedCommand : IRocketCommand, ICommand
    {
        public abstract void Execute(IRocketPlayer caller, string[] command);
        public AllowedCaller AllowedCaller { get; }
        public string Name { get; }
        public string Help { get; }
        public string Syntax { get; }
        public List<string> Aliases { get; }
        public List<string> Permissions { get; }
        public BaseTebexAdapter _adapter { get; }
    }
}