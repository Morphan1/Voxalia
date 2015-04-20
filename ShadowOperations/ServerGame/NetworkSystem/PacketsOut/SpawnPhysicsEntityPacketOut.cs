using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.EntitySystem;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.NetworkSystem.PacketsOut
{
    class SpawnPhysicsEntityPacketOut: AbstractPacketOut
    {
        public SpawnPhysicsEntityPacketOut(PhysicsEntity e)
        {
            ID = 2;
            Data = new byte[4 + 12 + 12 + 12 + 12 + 8 + 4 + 12 + 1];
            Utilities.FloatToBytes(e.GetMass()).CopyTo(Data, 0);
            e.GetPosition().ToBytes().CopyTo(Data, 4);
            e.GetVelocity().ToBytes().CopyTo(Data, 4 + 12);
            e.GetAngles().ToBytes().CopyTo(Data, 4 + 12 + 12);
            e.GetAngularVelocity().ToBytes().CopyTo(Data, 4 + 12 + 12 + 12);
            Utilities.LongToBytes(e.EID).CopyTo(Data, 4 + 12 + 12 + 12 + 12);
            Utilities.FloatToBytes(e.GetFriction()).CopyTo(Data, 4 + 12 + 12 + 12 + 12 + 8);
            // TODO: Unique gravity, restitution, etc. properties?
            // TODO: handle different e-types cleanly
            if (e is CubeEntity)
            {
                ((CubeEntity)e).HalfSize.ToBytes().CopyTo(Data, 4 + 12 + 12 + 12 + 12 + 8 + 4);
            }
            else if (e is PlayerEntity)
            {
                ((PlayerEntity)e).HalfSize.ToBytes().CopyTo(Data, 4 + 12 + 12 + 12 + 12 + 8 + 4);
            }
            else
            {
                new Location(5, 5, 5).ToBytes().CopyTo(Data, 4 + 12 + 12 + 12 + 12 + 8 + 4);
            }
            Data[4 + 12 + 12 + 12 + 12 + 8 + 4 + 12] = (byte)(e is CubeEntity ? 0 : 1);
        }
    }
}
