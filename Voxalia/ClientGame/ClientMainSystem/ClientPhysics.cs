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

        public Location SunLightDef = Location.One;

        public Location PlanetLightDef = new Location(1, 0.5, 0) * 0.5f;

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
            TheSun = new SkyLight(Location.Zero, CVars.r_shadowquality_sun.ValueI, 30 * 12, SunLightDef, new Location(0, 0, -1), 30 * 8);
            // TODO: Separate planet quality CVar?
            ThePlanet = new SkyLight(Location.Zero, CVars.r_shadowquality_sun.ValueI / 2, 30 * 12, PlanetLightDef, new Location(0, 0, -1), 30 * 8);
            Lights.Add(TheSun);
            Lights.Add(ThePlanet);
            TheWorld = new World();
            TheWorld.BuildWorld();
            TheWorld.TheClient = this;
        }

        public Location SunAngle = new Location(0, -75, 0);

        public Location PlanetAngle = new Location(0, -56, 90);

        public float PlanetLight = 1;

        public float PlanetSunDist = 0;

        public void TickWorld(double delta)
        {
            // TODO: Z+ -> max view rad + 30
            TheSun.Direction = Utilities.ForwardVector_Deg(SunAngle.Yaw, SunAngle.Pitch);
            TheSun.Reposition(Player.GetPosition().GetBlockLocation() - TheSun.Direction * 30 * 4);
            ThePlanet.Direction = Utilities.ForwardVector_Deg(PlanetAngle.Yaw, PlanetAngle.Pitch);
            ThePlanet.Reposition(Player.GetPosition().GetBlockLocation() - ThePlanet.Direction * 30 * 4);
            Quaternion diff;
            Vector3 tsd = TheSun.Direction.ToBVector();
            Vector3 tpd = ThePlanet.Direction.ToBVector();
            Quaternion.GetQuaternionBetweenNormalizedVectors(ref tsd, ref tpd, out diff);
            PlanetSunDist = Quaternion.GetAngleFromQuaternion(ref diff) / (float)Utilities.PI180;
            if (PlanetSunDist < 25)
            {
                TheSun.InternalLights[0].color = new OpenTK.Vector3((float)Math.Min(SunLightDef.X * (PlanetSunDist / 5), 1),
                    (float)Math.Min(SunLightDef.Y * (PlanetSunDist / 20), 1), (float)Math.Min(SunLightDef.Z * (PlanetSunDist / 20), 1));
                ThePlanet.InternalLights[0].color = new OpenTK.Vector3(0, 0, 0);
            }
            else
            {
                TheSun.InternalLights[0].color = SunLightDef.ToOVector();
                ThePlanet.InternalLights[0].color = (PlanetLightDef * (PlanetSunDist / 180f)).ToOVector();
            }
            PlanetLight = PlanetSunDist / 180f;
            if (SunAngle.Pitch < 20 && SunAngle.Pitch > -20)
            {
                float rel = 20 + (float)SunAngle.Pitch;
                if (rel == 0)
                {
                    rel = 0.00001f;
                }
                rel = 1f / (rel / 5f);
                rel = Math.Max(Math.Min(rel, 1f), 0f);
                TheSun.InternalLights[0].color = new OpenTK.Vector3(TheSun.InternalLights[0].color.X, TheSun.InternalLights[0].color.Y * rel, TheSun.InternalLights[0].color.Z * rel);
            }
            TheWorld.TickWorld(delta);
        }
    }
}
