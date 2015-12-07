using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.Shared;

namespace Voxalia.ServerGame.EntitySystem
{
    public class GlowstickEntity: GrenadeEntity
    {
        public int Color;

        public GlowstickEntity(int col, Region tregion) :
            base(tregion)
        {
            Color = col;
        }

        public override EntityType GetEntityType()
        {
            return EntityType.GLOWSTICK;
        }

        public override byte[] GetSaveBytes()
        {
            byte[] bbytes = GetPhysicsBytes();
            byte[] res = new byte[bbytes.Length + 4];
            bbytes.CopyTo(res, 0);
            Utilities.IntToBytes(Color).CopyTo(res, bbytes.Length);
            return res;
        }
    }

    public class GlowstickEntityConstructor : EntityConstructor
    {
        public override Entity Create(Region tregion, byte[] input)
        {
            int plen = 12 + 12 + 12 + 4 + 4 + 4 + 4 + 12 + 4 + 4 + 4;
            int colo = Utilities.BytesToInt(Utilities.BytesPartial(input, plen, 4));
            GlowstickEntity glowstick = new GlowstickEntity(colo, tregion);
            glowstick.ApplyBytes(input);
            return glowstick;
        }
    }
}
