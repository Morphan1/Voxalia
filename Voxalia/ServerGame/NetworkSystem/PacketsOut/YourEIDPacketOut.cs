using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class YourEIDPacketOut : AbstractPacketOut
    {
        public YourEIDPacketOut(long EID)
        {
            UsageType = NetUsageType.GENERAL;
            ID = ServerToClientPacket.YOUR_EID;
            Data = Utilities.LongToBytes(EID);
        }
    }
}
