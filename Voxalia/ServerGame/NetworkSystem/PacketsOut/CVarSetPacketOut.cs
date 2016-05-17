using FreneticScript;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.Shared;
using Voxalia.Shared.Files;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class CVarSetPacketOut: AbstractPacketOut
    {
        public CVarSetPacketOut(CVar var, Server tserver)
        {
            UsageType = NetUsageType.GENERAL;
            ID = ServerToClientPacket.CVAR_SET;
            DataStream ds = new DataStream();
            DataWriter dw = new DataWriter(ds);
            dw.WriteInt(tserver.Networking.Strings.IndexForString(var.Name.ToLowerFast()));
            dw.WriteFullString(var.Value);
            Data = ds.ToArray();
        }
    }
}
