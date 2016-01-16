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
            long eid = Utilities.BytesToLong(data);
            Entity e = TheClient.TheRegion.GetEntity(eid);
            if (e == null)
            {
                // Who cares?
                return true;
            }
            TheClient.TheRegion.Despawn(e);
            return true;
        }
    }
}
