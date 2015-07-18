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

        public SkyLight ThePlanet = null;

        // Note: the client only has one world loaded at any given time.
        public World TheWorld = null;

        public void BuildWorld()
        {
            if (TheSun != null)
            {
                TheSun.Destroy();
                Lights.Remove(TheSun);
                ThePlanet.Destroy();
                Lights.Remove(ThePlanet);
            }
            // TODO: Radius -> max view rad * 2
            // TODO: Size -> max view rad * 2 + 30 * 2
            TheSun = new SkyLight(Location.Zero, CVars.r_shadowquality_sun.ValueI, 30 * 12, Location.One, new Location(0, 0, -1), 30 * 8);
            // TODO: Separate planet quality CVar?
            ThePlanet = new SkyLight(Location.Zero, CVars.r_shadowquality_sun.ValueI / 2, 30 * 12, new Location(1, 0.5, 0) * 0.25, new Location(0, 0, -1), 30 * 8);
            Lights.Add(TheSun);
            Lights.Add(ThePlanet);
            TheWorld = new World();
            TheWorld.BuildWorld();
            TheWorld.TheClient = this;
        }

        public Location SunAngle = new Location(0, -75, 0);

        public Location PlanetAngle = new Location(0, -75, 90);

        public void TickWorld(double delta)
        {
            // TODO: Z+ -> max view rad + 30
            TheSun.Direction = Utilities.ForwardVector_Deg(SunAngle.Yaw, SunAngle.Pitch);
            TheSun.Reposition(Player.GetPosition().GetBlockLocation() - TheSun.Direction * 30 * 4);
            ThePlanet.Direction = Utilities.ForwardVector_Deg(PlanetAngle.Yaw, PlanetAngle.Pitch);
            ThePlanet.Reposition(Player.GetPosition().GetBlockLocation() - ThePlanet.Direction * 30 * 4);
            TheWorld.TickWorld(delta);
        }
    }
}
