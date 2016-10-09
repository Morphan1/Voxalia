//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System.Collections.Generic;
using System.Linq;
using Voxalia.Shared.Files;
using FreneticScript;

namespace Voxalia.ServerGame.NetworkSystem.PacketsIn
{
    public class CommandPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            Player.NoteDidAction();
            string[] datums = FileHandler.encoding.GetString(data).SplitFast('\n');
            List<string> args =  datums.ToList();
            string cmd = args[0];
            args.RemoveAt(0);
            Player.TheServer.PCEngine.Execute(Player, args, cmd);
            return true;
        }
    }
}
