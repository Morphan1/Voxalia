using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.PlayerCommandSystem.RegionCommands
{
    class StructurePlayerCommand : AbstractPlayerCommand
    {
        public StructurePlayerCommand()
        {
            Name = "structure";
        }

        public override void Execute(PlayerCommandEntry entry)
        {
            if (entry.InputArguments.Count < 1)
            {
                entry.Player.Network.SendMessage("^1/structure create <name>");
                entry.Player.Network.SendMessage("^1/structure paste <name>");
                return;
            }
            string cmd = entry.InputArguments[0].ToLower();
            if (cmd == "create")
            {
                if (entry.InputArguments.Count < 2)
                {
                    entry.Player.Network.SendMessage("^1/structure create <name>");
                    return;
                }
                string name = entry.InputArguments[1].ToLower();
                Structure created = null;
                try
                {
                    created = new Structure(entry.Player.TheRegion, entry.Player.GetPosition().GetBlockLocation(), 20); // TODO: 20 -> variable capped by a CVar
                }
                catch (Exception ex)
                {
                    entry.Player.Network.SendMessage("^1Error creating structure: " + ex.Message);
                    return;
                }
                byte[] dat = created.ToBytes();
                Program.Files.WriteBytes("structures/" + name + ".str", dat);
                entry.Player.Network.SendMessage("^2Structure created and saved.");
            }
            else if (cmd == "paste")
            {
                if (entry.InputArguments.Count < 2)
                {
                    entry.Player.Network.SendMessage("^1/structure create <name>");
                    return;
                }
                string name = entry.InputArguments[1].ToLower();
                string fn = "structures/" + name + ".str";
                if (!Program.Files.Exists(fn))
                {
                    entry.Player.Network.SendMessage("^1Error pasting structure: unknown structure name!");
                    return;
                }
                byte[] dat = Program.Files.ReadBytes(fn);
                Structure topaste = new Structure(dat);
                topaste.Paste(entry.Player.TheRegion, entry.Player.GetPosition());
                entry.Player.Network.SendMessage("^2Structure pasted.");
            }
            else
            {
                entry.Player.Network.SendMessage("^1/structure create <name>");
                entry.Player.Network.SendMessage("^1/structure paste <name>");
            }
        }
    }
}
