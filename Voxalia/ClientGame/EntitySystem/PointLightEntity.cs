using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.Shared;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.ClientGame.GraphicsSystems.LightingSystem;

namespace Voxalia.ClientGame.EntitySystem
{
    public class PointLightEntity: PrimitiveEntity
    {
        public PointLight Internal = null;

        bool generated = false;

        public int texturesize = 256;

        public PointLightEntity(Client tclient)
            : base(tclient, false)
        {
        }

        public override void Spawn()
        {
            Generate();
        }

        public override void Destroy()
        {
            Internal.Destroy();
            TheClient.Lights.Remove(Internal);
        }

        public override void Render()
        {
            // Don't render
        }

        public void Generate()
        {
            if (generated)
            {
                Internal.Destroy();
                TheClient.Lights.Remove(Internal);
            }
            generated = true;
            Internal = new PointLight(GetPosition(), texturesize, Radius, LightColor);
            TheClient.Lights.Add(Internal);
        }

        public override void SetPosition(Location pos)
        {
            if (Internal != null)
            {
                Internal.Reposition(pos);
            }
            base.SetPosition(pos);
        }

        public float Radius = 100;
        public Location LightColor = new Location(1, 1, 1);

        public override string ToString()
        {
            return "POINTLIGHTENTITY{location=" + Position + ";radius=" + Radius + ";color=" + LightColor + "}";
        }
    }
}
