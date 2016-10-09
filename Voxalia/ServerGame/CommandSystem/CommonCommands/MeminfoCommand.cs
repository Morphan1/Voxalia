//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.EntitySystem;
using FreneticScript;
using Voxalia.Shared;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers;

namespace Voxalia.ServerGame.CommandSystem.CommonCommands
{
    public class MeminfoCommand : AbstractCommand
    {
        Server TheServer;

        public MeminfoCommand(Server tserver)
        {
            TheServer = tserver;
            Name = "meminfo";
            Description = "Shows memory usage information.";
            Arguments = "";

        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            const string rn = "   Region Name Here   ";
            const string cr = "Chunk Exact RAM in MB";
            const string er = "Entity Est. RAM in MB";
            entry.Info(queue, "[<{text_color.emphasis}>" + rn + "<{text_color.base}>] [<{text_color.emphasis}>" + cr + "<{text_color.base}>] [<{text_color.emphasis}>" + er + "<{text_color.base}>]");
            long cht = 0;
            long entt = 0;
            int n = 0;
            foreach (World world in TheServer.LoadedWorlds)
            {
                n++;
                long chunk = Chunk.RAM_USAGE * world.MainRegion.LoadedChunks.Count;
                //string reg_cr = Utilities.Pad(Utilities.FormatNumber(chunk), ' ', cr.Length, false);
                long ent = 0;
                foreach (Entity e in world.MainRegion.Entities)
                {
                    ent += e.GetRAMUsage();
                }
                string reg_er = Utilities.Pad(Utilities.FormatNumber(ent), ' ', er.Length, false);
                entry.Info(queue, "[<{text_color.emphasis}>" + n + "<{text_color.base}>] [<{text_color.emphasis}>" + TagParser.Escape(world.Name) + "<{text_color.base}>] [<{text_color.emphasis}>" + reg_er + "<{text_color.base}>]");
                cht += chunk;
                entt += ent;
            }
            entry.Info(queue, "Totals -> Chunks (Semi-accurate): <{text_color.emphasis}>" + Utilities.FormatNumber(cht) + "<{text_color.base}>, Entities (Estimated): <{text_color.emphasis}>" + Utilities.FormatNumber(entt)
                + "<{text_color.base}>, actual usage: <{text_color.emphasis}>" + Utilities.FormatNumber(GC.GetTotalMemory(false)) + "<{text_color.base}>.");
        }
    }
}
