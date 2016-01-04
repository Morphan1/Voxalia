using Voxalia.Shared;
using Voxalia.ClientGame.EntitySystem;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class PhysicsEntityUpdatePacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 12 + 12 + 16 + 12 + 1 + 8)
            {
                SysConsole.Output(OutputType.WARNING, "Invalid physentupdtpacket: invalid length!");
                return false;
            }
            Location pos = Location.FromBytes(data, 0);
            Location vel = Location.FromBytes(data, 12);
            BEPUutilities.Quaternion ang = Utilities.BytesToQuaternion(data, 12 + 12);
            Location angvel = Location.FromBytes(data, 12 + 12 + 16);
            bool active = (data[12 + 12 + 16 + 12] & 1) == 1;
            long eID = Utilities.BytesToLong(Utilities.BytesPartial(data, 12 + 12 + 16 + 12 + 1, 8));
            for (int i = 0; i < TheClient.TheRegion.Entities.Count; i++)
            {
                if (TheClient.TheRegion.Entities[i] is PhysicsEntity)
                {
                    PhysicsEntity e = (PhysicsEntity)TheClient.TheRegion.Entities[i];
                    if (e.EID == eID)
                    {
                        e.SetPosition(pos);
                        e.SetVelocity(vel);
                        e.SetOrientation(ang);
                        e.SetAngularVelocity(angvel);
                        if (e.Body != null && e.Body.ActivityInformation != null && e.Body.ActivityInformation.IsActive && !active) // TODO: Why are the first two checks needed?
                        {
                            if (e.Body.ActivityInformation.SimulationIsland != null) // TODO: Why is this needed?
                            {
                                e.Body.ActivityInformation.SimulationIsland.IsActive = false;
                            }
                        }
                        else if (e.Body != null && e.Body.ActivityInformation != null && !e.Body.ActivityInformation.IsActive && active) // TODO: Why are the first two checks needed?
                        {
                            e.Body.ActivityInformation.Activate();
                        }
                        return true;
                    }
                }
            }
            SysConsole.Output(OutputType.WARNING, "Invalid physentupdtpacket: invalid EID: " + eID);
            return false;
        }
    }
}
