using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using BEPUphysics.CollisionRuleManagement;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    class SpawnPhysicsEntityPacketOut: AbstractPacketOut
    {
        public SpawnPhysicsEntityPacketOut(PhysicsEntity e)
        {
            ID = 2;
            Data = new byte[4 + 12 + 12 + 16 + 12 + 8 + 4 + 12 + 1 + (e is CubeEntity ? 4 * 6 + 4 * 6: (e is ModelEntity ? 4 + 1: 0)) + 4 + 1];
            Utilities.FloatToBytes(e.GetMass()).CopyTo(Data, 0);
            e.GetPosition().ToBytes().CopyTo(Data, 4);
            e.GetVelocity().ToBytes().CopyTo(Data, 4 + 12);
            Utilities.QuaternionToBytes(e.GetOrientation()).CopyTo(Data, 4 + 12 + 12);
            e.GetAngularVelocity().ToBytes().CopyTo(Data, 4 + 12 + 12 + 16);
            Utilities.LongToBytes(e.EID).CopyTo(Data, 4 + 12 + 12 + 16 + 12);
            Utilities.FloatToBytes(e.GetFriction()).CopyTo(Data, 4 + 12 + 12 + 16 + 12 + 8);
            // TODO: Unique gravity, restitution, etc. properties?
            // TODO: handle different e-types cleanly
            if (e is CubeEntity)
            {
                ((CubeEntity)e).HalfSize.ToBytes().CopyTo(Data, 4 + 12 + 12 + 16 + 12 + 8 + 4);
            }
            else if (e is PlayerEntity)
            {
                ((PlayerEntity)e).HalfSize.ToBytes().CopyTo(Data, 4 + 12 + 12 + 16 + 12 + 8 + 4);
            }
            else if (e is ModelEntity)
            {
                ((ModelEntity)e).scale.ToBytes().CopyTo(Data, 4 + 12 + 12 + 16 + 12 + 8 + 4);
            }
            else
            {
                new Location(5, 5, 5).ToBytes().CopyTo(Data, 4 + 12 + 12 + 16 + 12 + 8 + 4);
            }
            Data[4 + 12 + 12 + 16 + 12 + 8 + 4 + 12] = (byte)(e is CubeEntity ? 0 : (e is PlayerEntity ? 1: 2));
            if (e is CubeEntity)
            {
                CubeEntity ce = (CubeEntity)e;
                int start = 4 + 12 + 12 + 16 + 12 + 8 + 4 + 12 + 1;
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
            else if (e is ModelEntity)
            {
                ModelEntity me = (ModelEntity)e;
                int start = 4 + 12 + 12 + 16 + 12 + 8 + 4 + 12 + 1;
                NetStringManager strings = me.TheServer.Networking.Strings;
                Utilities.IntToBytes(strings.IndexForString(me.model)).CopyTo(Data, start);
                Data[start + 4] = (byte)me.mode;
            }
            Utilities.FloatToBytes(e.GetBounciness()).CopyTo(Data, Data.Length - (4 + 1));
            Data[Data.Length - 1] = (byte)(e.Visible ? 1 : 0);
            if (e.CGroup == e.TheServer.Collision.Solid)
            {
                Data[Data.Length - 1] |= 2;
            }
            else if (e.CGroup == e.TheServer.Collision.NonSolid)
            {
                Data[Data.Length - 1] |= 4;
            }
            else if (e.CGroup == e.TheServer.Collision.Item)
            {
                Data[Data.Length - 1] |= 2 | 4;
            }
            else if (e.CGroup == e.TheServer.Collision.Player)
            {
                Data[Data.Length - 1] |= 8;
            }
            else if (e.CGroup == e.TheServer.Collision.Trigger)
            {
                Data[Data.Length - 1] |= 4 | 8;
            }
        }
    }
}
