using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    class DespawnEntityPacketOut: AbstractPacketOut
    {
        public DespawnEntityPacketOut(long EID)
        {
            ID = ServerToClientPacket.DESPAWN_ENTITY;
            Data = Utilities.LongToBytes(EID);
        }
    }
}
