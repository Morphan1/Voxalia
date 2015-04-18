using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShadowOperations.ServerGame.PlayerCommandSystem
{
    public abstract class AbstractPlayerCommand
    {
        public string Name;

        public abstract void Execute(PlayerCommandEntry entry);
    }
}
