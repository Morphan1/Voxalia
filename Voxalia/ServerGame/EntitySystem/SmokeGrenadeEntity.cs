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
    // TODO: Maximum smoke usage counter!
    public class SmokegrenadeEntity : GrenadeEntity
    {
        System.Drawing.Color col;
        ParticleEffectNetType SmokeType;

        public SmokegrenadeEntity(System.Drawing.Color _col, Region tregion, ParticleEffectNetType smokeType) :
            base(tregion)
        {
            col = _col;
            SmokeType = smokeType;
        }

        public override EntityType GetEntityType()
        {
            return EntityType.SMOKE_GRENADE;
        }

        public override byte[] GetSaveBytes()
        {
            byte[] bbytes = GetPhysicsBytes();
            byte[] res = new byte[bbytes.Length + 4 + 1];
            bbytes.CopyTo(res, 0);
            Utilities.IntToBytes(col.ToArgb()).CopyTo(res, bbytes.Length);
            res[bbytes.Length + 4] = (byte)SmokeType;
            return res;
        }

        public double timer = 0;

        public double pulse = 1.0 / 15.0;

        public override void Tick()
        {
            timer += TheRegion.Delta;
            while (timer > pulse)
            {
                Location colo = new Location(col.R / 255f, col.G / 255f, col.B / 255f);
                TheRegion.SendToAll(new ParticleEffectPacketOut(SmokeType, 5, GetPosition(), colo));
                timer -= pulse;
            }
            base.Tick();
        }
    }

    public class SmokegrenadeEntityConstructor : EntityConstructor
    {
        public override Entity Create(Region tregion, byte[] input)
        {
            int plen = 12 + 12 + 12 + 4 + 4 + 4 + 4 + 12 + 4 + 4 + 4;
            int colo = Utilities.BytesToInt(Utilities.BytesPartial(input, plen, 4));
            byte effecttype = input[plen + 4];
            SmokegrenadeEntity grenade = new SmokegrenadeEntity(System.Drawing.Color.FromArgb(colo), tregion, (ParticleEffectNetType)effecttype);
            grenade.ApplyBytes(input);
            return grenade;
        }
    }
}
