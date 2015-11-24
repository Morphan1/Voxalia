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
    public class SmokegrenadeEntity: GlowstickEntity
    {
        public SmokegrenadeEntity(int col, Region tregion):
            base(col, tregion)
        {
        }

        public double timer = 0;

        public double pulse = 1.0 / 15.0;

        public override void Tick()
        {
            timer += TheRegion.Delta;
            while (timer > pulse)
            {
                TheRegion.SendToAll(new ParticleEffectPacketOut(ParticleEffectNetType.SMOKE, 5, GetPosition()));
                timer -= pulse;
            }
            base.Tick();
        }
    }
}
