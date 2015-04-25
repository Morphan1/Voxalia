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
            Data = new byte[4 + 12 + 12 + 12 + 12 + 8 + 4 + 12 + 1 + (e is CubeEntity ? 4 * 6 + 4 * 6: 0)];
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
            if (e is CubeEntity)
            {
                CubeEntity ce = (CubeEntity)e;
                int start = 4 + 12 + 12 + 12 + 12 + 8 + 4 + 12 + 1;
                NetStringManager strings = ce.TheServer.Networking.Strings;
                Utilities.IntToBytes(strings.IndexForString(ce.Textures[0])).CopyTo(Data, start);
                Utilities.IntToBytes(strings.IndexForString(ce.Textures[1])).CopyTo(Data, start + 4);
                Utilities.IntToBytes(strings.IndexForString(ce.Textures[2])).CopyTo(Data, start + 4 * 2);
                Utilities.IntToBytes(strings.IndexForString(ce.Textures[3])).CopyTo(Data, start + 4 * 3);
                Utilities.IntToBytes(strings.IndexForString(ce.Textures[4])).CopyTo(Data, start + 4 * 4);
                Utilities.IntToBytes(strings.IndexForString(ce.Textures[5])).CopyTo(Data, start + 4 * 5);
                Utilities.IntToBytes(strings.IndexForString(ce.TexCoords[0])).CopyTo(Data, start + 4 * 6);
                Utilities.IntToBytes(strings.IndexForString(ce.TexCoords[1])).CopyTo(Data, start + 4 * 6 + 4);
                Utilities.IntToBytes(strings.IndexForString(ce.TexCoords[2])).CopyTo(Data, start + 4 * 6 + 4 * 2);
                Utilities.IntToBytes(strings.IndexForString(ce.TexCoords[3])).CopyTo(Data, start + 4 * 6 + 4 * 3);
                Utilities.IntToBytes(strings.IndexForString(ce.TexCoords[4])).CopyTo(Data, start + 4 * 6 + 4 * 4);
                Utilities.IntToBytes(strings.IndexForString(ce.TexCoords[5])).CopyTo(Data, start + 4 * 6 + 4 * 5);
            }
        }
    }
}
