//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.Shared;

namespace Voxalia.ClientGame.EntitySystem
{
    public class FireEntity : PrimitiveEntity
    {
        public Entity AttachedTo = null;

        public FireEntity(Location spawn, Entity attached_to, Region tregion) : base(tregion, false)
        {
            AttachedTo = attached_to;
            SetPosition(spawn);
        }

        public override void Destroy()
        {
            // No destroy calculations.
        }

        public Location RelSpot(out float height)
        {
            double x = Utilities.UtilRandom.NextDouble();
            double y = Utilities.UtilRandom.NextDouble();
            height = (1.3f - (float)((x - 0.5) * (y - 0.5))) * 0.64f;
            return GetPosition() + new Location(x, y, 1);
        }

        const double maxDist = 3.5;

        public override void Tick()
        {
            float size = 0.5f;
            foreach (Entity entity in TheClient.TheRegion.Entities)
            {
                if (entity is FireEntity && entity.GetPosition().DistanceSquared(GetPosition()) < maxDist)
                {
                    size += 5f;
                }
            }
            if (AttachedTo == null)
            {
                float heightmod;
                Location rel = RelSpot(out heightmod);
                TheClient.Particles.Fire(rel + new Location(0, 0, 0.5), size * heightmod * 0.2f);
            }
        }

        public override void Render()
        {
            // Doesn't currently render on its own.
        }

        public override void Spawn()
        {
            // No spawn calculations.
        }
    }
}
