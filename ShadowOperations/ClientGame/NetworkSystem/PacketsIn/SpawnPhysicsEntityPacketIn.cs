﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.ClientMainSystem;
using ShadowOperations.ClientGame.EntitySystem;
using ShadowOperations.ClientGame.GraphicsSystems;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsIn
{
    public class SpawnPhysicsEntityPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            int len = 4 + 12 + 12 + 12 + 12 + 8 + 4 + 12 + 1;
            if (data.Length != len
                && data.Length != len + 4 * 6 + 4 * 6
                && data.Length != len + 4)
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
                CubeEntity ce1 = new CubeEntity(TheClient, halfsize);
                ce = ce1;
                int start = 4 + 12 + 12 + 12 + 12 + 8 + 4 + 12 + 1;
                NetStringManager strings = TheClient.Network.Strings;
                ce1.Textures[0] = strings.StringForIndex(Utilities.BytesToInt(Utilities.BytesPartial(data, start, 4)));
                ce1.Textures[1] = strings.StringForIndex(Utilities.BytesToInt(Utilities.BytesPartial(data, start + 4, 4)));
                ce1.Textures[2] = strings.StringForIndex(Utilities.BytesToInt(Utilities.BytesPartial(data, start + 4 * 2, 4)));
                ce1.Textures[3] = strings.StringForIndex(Utilities.BytesToInt(Utilities.BytesPartial(data, start + 4 * 3, 4)));
                ce1.Textures[4] = strings.StringForIndex(Utilities.BytesToInt(Utilities.BytesPartial(data, start + 4 * 4, 4)));
                ce1.Textures[5] = strings.StringForIndex(Utilities.BytesToInt(Utilities.BytesPartial(data, start + 4 * 5, 4)));
                ce1.Coords[0] = TextureCoordinates.FromString(strings.StringForIndex(Utilities.BytesToInt(Utilities.BytesPartial(data, start + 4 * 6, 4))));
                ce1.Coords[1] = TextureCoordinates.FromString(strings.StringForIndex(Utilities.BytesToInt(Utilities.BytesPartial(data, start + 4 * 6 + 4, 4))));
                ce1.Coords[2] = TextureCoordinates.FromString(strings.StringForIndex(Utilities.BytesToInt(Utilities.BytesPartial(data, start + 4 * 6 + 4 * 2, 4))));
                ce1.Coords[3] = TextureCoordinates.FromString(strings.StringForIndex(Utilities.BytesToInt(Utilities.BytesPartial(data, start + 4 * 6 + 4 * 3, 4))));
                ce1.Coords[4] = TextureCoordinates.FromString(strings.StringForIndex(Utilities.BytesToInt(Utilities.BytesPartial(data, start + 4 * 6 + 4 * 4, 4))));
                ce1.Coords[5] = TextureCoordinates.FromString(strings.StringForIndex(Utilities.BytesToInt(Utilities.BytesPartial(data, start + 4 * 6 + 4 * 5, 4))));
            }
            else if (type == 1)
            {
                ce = new OtherPlayerEntity(TheClient, halfsize);
            }
            else if (type == 2)
            {
                int start = 4 + 12 + 12 + 12 + 12 + 8 + 4 + 12 + 1;
                NetStringManager strings = TheClient.Network.Strings;
                ModelEntity me = new ModelEntity(strings.StringForIndex(Utilities.BytesToInt(Utilities.BytesPartial(data, start, 4))), TheClient);
                ce = me;
            }
            else
            {
                return false;
            }
            ce.SetMass(mass);
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
