using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.EntitySystem
{
    class TargetEntity: CharacterEntity
    {
        public TargetEntity(Region tregion) :
            base (tregion, 100)
        {
            SetMass(70);
        }

        public override EntityType GetEntityType()
        {
            return EntityType.TARGET_ENTITY;
        }

        public double NextBoing = 0;

        public override byte[] GetSaveBytes()
        {
            return null; // TODO: Save
        }

        public override void Tick()
        {
            base.Tick();
            NextBoing -= TheRegion.Delta;
            if (NextBoing <= 0)
            {
                NextBoing = Utilities.UtilRandom.NextDouble() * 2;
                XMove = (float)Utilities.UtilRandom.NextDouble() * 2f - 1f;
                YMove = (float)Utilities.UtilRandom.NextDouble() * 2f - 1f;
                Upward = Utilities.UtilRandom.Next(100) > 75;
            }
        }
        
        public override void Die()
        {
            if (Removed)
            {
                return;
            }
            TheRegion.Explode(GetPosition(), 5);
            RemoveMe();
        }
    }
}
