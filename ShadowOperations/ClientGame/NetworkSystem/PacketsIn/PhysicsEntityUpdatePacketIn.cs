using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.EntitySystem;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsIn
{
    public class PhysicsEntityUpdatePacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 12 + 12 + 12 + 12 + 1 + 8)
            {
                return false;
            }
            Location pos = Location.FromBytes(data, 0);
            Location vel = Location.FromBytes(data, 12);
            Location ang = Location.FromBytes(data, 12 + 12);
            Location angvel = Location.FromBytes(data, 12 + 12 + 12);
            bool active = (data[12 + 12 + 12 + 12] & 1) == 1;
            long eID = Utilities.BytesToLong(Utilities.BytesPartial(data, 12 + 12 + 12 + 12 + 1, 8));
            for (int i = 0; i < TheClient.Entities.Count; i++)
            {
                if (TheClient.Entities[i] is PhysicsEntity)
                {
                    PhysicsEntity e = (PhysicsEntity)TheClient.Entities[i];
                    if (e.EID == eID)
                    {
                        e.SetPosition(pos);
                        e.SetVelocity(vel);
                        e.SetAngles(ang);
                        e.SetAngularVelocity(angvel);
                        if (e.Body.ActivityInformation.IsActive && !active)
                        {
                            e.Body.ActivityInformation.SimulationIsland.IsActive = false;
                        }
                        else if (!e.Body.ActivityInformation.IsActive && active)
                        {
                            e.Body.ActivityInformation.Activate();
                        }
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
