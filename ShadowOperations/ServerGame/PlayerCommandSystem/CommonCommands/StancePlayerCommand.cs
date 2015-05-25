using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.ServerGame.EntitySystem;

namespace ShadowOperations.ServerGame.PlayerCommandSystem.CommonCommands
{
    public class StancePlayerCommand: AbstractPlayerCommand
    {
        public StancePlayerCommand()
        {
            Name = "stance";
        }

        public override void Execute(PlayerCommandEntry entry)
        {
            if (entry.InputArguments.Count < 1)
            {
                entry.Player.Network.SendMessage("^r^1/stance <stance>"); // TODO: ShowUsage
                return;
            }
            string stance = entry.InputArguments[0].ToLower();
            if (stance == "stand")
            {
                entry.Player.Stance = PlayerStance.STAND;
            }
            else if (stance == "crouch")
            {
                entry.Player.Stance = PlayerStance.CROUCH;
            }
            else if (stance == "crawl")
            {
                entry.Player.Stance = PlayerStance.CRAWL;
            }
            else
            {
                entry.Player.Network.SendMessage("^r^1Unknown stance input."); // TODO: Languaging
            }
        }
    }
}
