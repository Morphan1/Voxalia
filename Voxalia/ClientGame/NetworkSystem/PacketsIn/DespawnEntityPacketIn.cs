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
                SysConsole.Output(OutputType.WARNING, "Cannot find entity: " + eid);
                return false;
            }
            TheClient.TheRegion.Despawn(e);
            return true;
        }
    }
}
