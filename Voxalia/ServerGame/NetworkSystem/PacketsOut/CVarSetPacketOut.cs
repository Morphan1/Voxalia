using FreneticScript;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.Shared.Files;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class CVarSetPacketOut: AbstractPacketOut
    {
        public CVarSetPacketOut(CVar var, Server tserver)
        {
            ID = 22;
            DataStream ds = new DataStream();
            DataWriter dw = new DataWriter(ds);
            dw.WriteInt(tserver.Networking.Strings.IndexForString(var.Name.ToLowerInvariant()));
            dw.WriteFullString(var.Value);
            Data = ds.ToArray();
        }
    }
}
