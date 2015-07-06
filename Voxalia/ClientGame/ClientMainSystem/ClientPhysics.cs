using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ClientGame.EntitySystem;
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.Settings;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.ClientGame.GraphicsSystems.LightingSystem;

namespace Voxalia.ClientGame.ClientMainSystem
{
    public partial class Client
    {
        public SkyLight TheSun = null;

        // Note: the client only has one world loaded at any given time.
        public World TheWorld = null;

        public void BuildWorld()
        {
            if (TheSun != null)
            {
                TheSun.Destroy();
                Lights.Remove(TheSun);
            }
            // TODO: Radius, Size -> view rad
            TheSun = new SkyLight(Location.Zero, 2048, 30 * 6, Location.One, new Location(0, 0, -1), 30 * 6);
            Lights.Add(TheSun);
            TheWorld = new World();
            TheWorld.BuildWorld();
            TheWorld.TheClient = this;
        }

        public void TickWorld(double delta)
        {
            // TODO: Z+ -> view rad
            TheSun.Reposition(Player.GetPosition() + new Location(0, 0, 30 * 6));
            TheWorld.TickWorld(delta);
        }
    }
}
