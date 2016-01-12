using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ClientGame.WorldSystem
{
    public partial class Region
    {
        public void TickClouds()
        {
            for (int i = 0; i < Clouds.Count; i++)
            {
                Clouds[i].Position += Clouds[i].Velocity * Delta;
            }
        }

        public List<Cloud> Clouds = new List<Cloud>();

        public void RenderClouds()
        {
            TheClient.Textures.GetTexture("effects/clouds/cloud1").Bind(); // TODO: Cache!
            foreach (Cloud cloud in Clouds)
            {
                for (int i = 0; i < cloud.Points.Count; i++)
                {
                    TheClient.Rendering.RenderBillboard(cloud.Points[i] + cloud.Position, new Location(cloud.Sizes[i]), TheClient.CameraPos);
                }
            }
        }
    }
}
