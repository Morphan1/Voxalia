using Voxalia.ClientGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    class SpawnPrimitiveEntityPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 12 + 12 + 16 + 12 + 12 + 8)
            {
                return false;
            }
            BasicPrimitiveEntity bpe = new BasicPrimitiveEntity(TheClient.TheRegion, false);
            bpe.model = TheClient.Models.GetModel("projectiles/arrow");
            bpe.Position = Location.FromBytes(data, 0);
            bpe.Velocity = Location.FromBytes(data, 12);
            bpe.Angles = Utilities.BytesToQuaternion(data, 12 + 12);
            bpe.Scale = Location.FromBytes(data, 12 + 12 + 16);
            bpe.Gravity = Location.FromBytes(data, 12 + 12 + 16 + 12);
            bpe.EID = Utilities.BytesToLong(Utilities.BytesPartial(data, 12 + 12 + 16 + 12 + 12, 8));
            TheClient.TheRegion.SpawnEntity(bpe);
            return true;
        }
    }
}
