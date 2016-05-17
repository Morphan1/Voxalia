using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class SpawnPrimitiveEntityPacketOut: AbstractPacketOut
    {
        public SpawnPrimitiveEntityPacketOut(PrimitiveEntity pe)
        {
            ID = ServerToClientPacket.SPAWN_PRIMITIVE_ENTITY;
            Data = new byte[12 + 12 + 16 + 12 + 12 + 8];
            pe.GetPosition().ToBytes().CopyTo(Data, 0);
            pe.GetVelocity().ToBytes().CopyTo(Data, 12);
            Utilities.QuaternionToBytes(pe.Angles).CopyTo(Data, 12 + 12);
            pe.Scale.ToBytes().CopyTo(Data, 12 + 12 + 16);
            pe.Gravity.ToBytes().CopyTo(Data, 12 + 12 + 16 + 12);
            Utilities.LongToBytes(pe.EID).CopyTo(Data, 12 + 12 + 16 + 12 + 12);
        }
    }
}
