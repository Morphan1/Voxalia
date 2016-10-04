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

        public Location RelSpot()
        {
            double x = Utilities.UtilRandom.NextDouble();
            double y = Utilities.UtilRandom.NextDouble();
            return GetPosition() + new Location(x, y, 1);
        }

        const double maxDist = 9;

        public override void Tick()
        {
            float size = 0.1f;
            foreach (Entity entity in TheClient.TheRegion.Entities)
            {
                if (entity is FireEntity && entity.GetPosition().DistanceSquared(GetPosition()) < maxDist)
                {
                    size += 1;
                }
            }
            if (AttachedTo == null)
            {
                //TheClient.Particles.FireBlue(RelSpot() + new Location(0, 0, 0.3));
                for (int i = 0; i < 3; i++) // TODO: CVar instead of 3?
                {
                    TheClient.Particles.Fire(RelSpot() + new Location(0, 0, 0.5), size);
                }
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
