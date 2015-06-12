using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.ServerMainSystem;

namespace ShadowOperations.ServerGame.NetworkSystem.PacketsOut
{
    public class CVarSetPacketOut: AbstractPacketOut
    {
        public CVarSetPacketOut(CVar var, Server tserver)
        {
            ID = 22;
            DataStream ds = new DataStream();
            DataWriter dw = new DataWriter(ds);
            dw.WriteInt(tserver.Networking.Strings.IndexForString(var.Name.ToLower()));
            dw.WriteFullString(var.Value);
            Data = ds.ToArray();
        }
    }
}
