using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.Shared;
using Voxalia.ServerGame.ItemSystem;
using FreneticScript.TagHandlers.Objects;

namespace Voxalia.ServerGame.EntitySystem
{
    public class PaintBombEntity: GrenadeEntity
    {
        public byte Color;

        public PaintBombEntity(byte col, Region tregion) :
            base(tregion)
        {
            Color = col;
        }

        public override EntityType GetEntityType()
        {
            return EntityType.PAINT_BOMB;
        }

        public override byte[] GetSaveBytes()
        {
            // Do not save
            return null;
        }

        public override void SpawnBody()
        {
            base.SpawnBody();
            Body.CollisionInformation.Events.ContactCreated += (s, o, p, c) =>
            {
                boomtime = 0;
            };
        }
        
        public double timer = 0.0;

        public double boomtime = 5.0;

        public override void Tick()
        {
            timer += TheRegion.Delta;
            if (timer > boomtime)
            {
                RemoveMe();
                TheRegion.PaintBomb(GetPosition(), Color, 5); // TODO: radius, etc. settings
            }
            else
            {
                base.Tick();
            }
        }
    }
}
