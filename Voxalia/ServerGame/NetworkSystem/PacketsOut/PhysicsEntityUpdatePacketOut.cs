using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class PhysicsEntityUpdatePacketOut: AbstractPacketOut
    {
        public PhysicsEntityUpdatePacketOut(PhysicsEntity e)
        {
            UsageType = NetUsageType.ENTITIES;
            ID = ServerToClientPacket.PHYSICS_ENTITY_UPDATE;
            Data = new byte[12 + 12 + 16 + 12 + 1 + 8];
            e.GetPosition().ToBytes().CopyTo(Data, 0);
            e.GetVelocity().ToBytes().CopyTo(Data, 12);
            Utilities.QuaternionToBytes(e.GetOrientation()).CopyTo(Data, 12 + 12);
            e.GetAngularVelocity().ToBytes().CopyTo(Data, 12 + 12 + 16);
            Data[12 + 12 + 16 + 12] = (byte)((e.Body != null && e.Body.ActivityInformation.IsActive) ? 1 : 0);
            Utilities.LongToBytes(e.EID).CopyTo(Data, 12 + 12 + 16 + 12 + 1);
        }
    }
}
