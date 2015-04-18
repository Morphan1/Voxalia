using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.EntitySystem;

namespace ShadowOperations.ServerGame.PlayerCommandSystem
{
    public class PlayerCommandEntry
    {
        public PlayerEntity Player;

        public AbstractPlayerCommand Command;

        public List<string> InputArguments;
    }
}
