//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.Shared;
using FreneticScript;
using Voxalia.Shared.Files;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class CVarSetPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            DataReader dr = new DataReader(new DataStream(data));
            int cvarname_id = dr.ReadInt();
            string cvarvalue = dr.ReadFullString();
            string cvarname = TheClient.Network.Strings.StringForIndex(cvarname_id);
            CVar cvar = TheClient.CVars.system.Get(cvarname);
            if (cvar == null || !cvar.Flags.HasFlag(CVarFlag.ServerControl))
            {
                SysConsole.Output(OutputType.WARNING, "Invalid CVar " + cvarname);
                return false;
            }
            cvar.Set(cvarvalue, true);
            return true;
        }
    }
}
