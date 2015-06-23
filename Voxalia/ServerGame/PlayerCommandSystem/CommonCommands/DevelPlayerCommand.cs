using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.PlayerCommandSystem.CommonCommands
{
    class DevelPlayerCommand : AbstractPlayerCommand
    {
        public DevelPlayerCommand()
        {
            Name = "devel";
            Silent = false;
        }

        public override void Execute(PlayerCommandEntry entry)
        {
            if (entry.InputArguments.Count <= 0)
            {
                entry.Player.Network.SendMessage("/devel <subcommand> [ values ... ]");
                return;
            }
            string arg0 = entry.InputArguments[0];
            if (arg0 == "spawnVehicle")
            {
                VehicleEntity ve = new VehicleEntity("failmobile", entry.Player.TheWorld);
                ve.SetPosition(entry.Player.GetPosition());
                entry.Player.TheWorld.SpawnEntity(ve);
            }
            else
            {
                entry.Player.Network.SendMessage("/devel <subcommand> [ values ... ]");
                return;
            }
        }
    }
}
