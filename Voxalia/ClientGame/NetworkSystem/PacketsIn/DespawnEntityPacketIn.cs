using Voxalia.ClientGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class DespawnEntityPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 8)
            {
                return false;
            }
            Entity e = TheClient.TheRegion.GetEntity(Utilities.BytesToLong(data));
            if (e == null)
            {
                return false;
            }
            TheClient.TheRegion.Despawn(e);
            return true;
        }
    }
}
