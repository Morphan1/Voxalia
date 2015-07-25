using Voxalia.ClientGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class SpawnBulletPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 8 + 4 + 12 + 12)
            {
                return false;
            }
            BasicPrimitiveEntity bpe = new BasicPrimitiveEntity(TheClient.TheWorld, false);
            bpe.EID = Utilities.BytesToLong(Utilities.BytesPartial(data, 0, 8));
            bpe.Scale = new Location(Utilities.BytesToFloat(Utilities.BytesPartial(data, 8, 4)));
            bpe.SetPosition(Location.FromBytes(data, 8 + 4));
            bpe.SetVelocity(Location.FromBytes(data, 8 + 4 + 12));
            TheClient.TheWorld.SpawnEntity(bpe);
            return true;
        }
    }
}
