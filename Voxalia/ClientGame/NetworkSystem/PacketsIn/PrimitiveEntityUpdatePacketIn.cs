using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.EntitySystem;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsIn
{
    public class PrimitiveEntityUpdatePacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 12 + 12 + 16 + 12 + 8)
            {
                return false;
            }
            Location pos = Location.FromBytes(data, 0);
            Location vel = Location.FromBytes(data, 12);
            BEPUutilities.Quaternion ang = Utilities.BytesToQuaternion(data, 12 + 12);
            Location grav = Location.FromBytes(data, 12 + 12 + 16);
            long eID = Utilities.BytesToLong(Utilities.BytesPartial(data, 12 + 12 + 16 + 12, 8));
            for (int i = 0; i < TheClient.Entities.Count; i++)
            {
                if (TheClient.Entities[i] is PrimitiveEntity)
                {
                    PrimitiveEntity e = (PrimitiveEntity)TheClient.Entities[i];
                    if (e.EID == eID)
                    {
                        e.SetPosition(pos);
                        e.SetVelocity(vel);
                        e.Angles = ang;
                        e.Gravity = grav;
                        return true;
                    }
                }
            }
            SysConsole.Output(OutputType.WARNING, "Unknown EID " + eID);
            return false;
        }
    }
}
