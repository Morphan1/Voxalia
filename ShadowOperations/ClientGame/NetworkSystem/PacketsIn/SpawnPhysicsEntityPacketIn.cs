using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.ClientMainSystem;
using ShadowOperations.ClientGame.EntitySystem;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsIn
{
    public class SpawnPhysicsEntityPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 4 + 12 + 12 + 12 + 12 + 8 + 4 + 12 + 1)
            {
                return false;
            }
            byte type = data[4 + 12 + 12 + 12 + 12 + 8 + 4 + 12];
            float mass = Utilities.BytesToFloat(Utilities.BytesPartial(data, 0, 4));
            Location pos = Location.FromBytes(data, 4);
            Location vel = Location.FromBytes(data, 4 + 12);
            Location ang = Location.FromBytes(data, 4 + 12 + 12);
            Location angvel = Location.FromBytes(data, 4 + 12 + 12 + 12);
            long eID = Utilities.BytesToLong(Utilities.BytesPartial(data, 4 + 12 + 12 + 12 + 12, 8));
            float fric = Utilities.BytesToFloat(Utilities.BytesPartial(data, 4 + 12 + 12 + 12 + 12 + 8, 4));
            Location halfsize = Location.FromBytes(data, 4 + 12 + 12 + 12 + 12 + 8 + 4);
            PhysicsEntity ce;
            if (type == 0)
            {
                ce = new CubeEntity(TheClient, halfsize);
            }
            else if (type == 1)
            {
                ce = new OtherPlayerEntity(TheClient, halfsize);
            }
            else
            {
                return false;
            }
            ce.SetPosition(pos);
            ce.SetVelocity(vel);
            ce.SetAngles(ang);
            ce.SetAngularVelocity(angvel);
            ce.EID = eID;
            ce.SetFriction(fric);
            TheClient.SpawnEntity(ce);
            return true;
        }
    }
}
