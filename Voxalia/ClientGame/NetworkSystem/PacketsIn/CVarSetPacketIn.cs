using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Frenetic;

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
