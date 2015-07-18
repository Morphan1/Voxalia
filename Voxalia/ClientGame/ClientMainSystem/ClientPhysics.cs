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

        public Location PlanetAngle = new Location(0, -75, 90);

        public float PlanetLight = 1;

        public void TickWorld(double delta)
        {
            // TODO: Z+ -> max view rad + 30
            TheSun.Direction = Utilities.ForwardVector_Deg(SunAngle.Yaw, SunAngle.Pitch);
            TheSun.Reposition(Player.GetPosition().GetBlockLocation() - TheSun.Direction * 30 * 4);
            ThePlanet.Direction = Utilities.ForwardVector_Deg(PlanetAngle.Yaw, PlanetAngle.Pitch);
            ThePlanet.Reposition(Player.GetPosition().GetBlockLocation() - ThePlanet.Direction * 30 * 4);
            Quaternion planetquat = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)(PlanetAngle.Pitch * Utilities.PI180))
                * Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), (float)(PlanetAngle.Yaw * Utilities.PI180));
            Quaternion sunquat = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)(SunAngle.Pitch * Utilities.PI180))
                * Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), (float)(SunAngle.Yaw * Utilities.PI180));
            SysConsole.Output(OutputType.INFO, SunAngle + " yields " + sunquat.X + "," + sunquat.Y + "," + sunquat.Z + "," + sunquat.W);
            Quaternion diff;
            Quaternion.GetRelativeRotation(ref sunquat, ref planetquat, out diff);
            float dist = Quaternion.GetAngleFromQuaternion(ref diff) / (float)Utilities.PI180;
            if (dist < 25)
            {
                TheSun.InternalLights[0].color = new OpenTK.Vector3((float)SunLightDef.X * (dist / 5), (float)SunLightDef.Y * (dist / 20), (float)SunLightDef.Z * (dist / 20));
                ThePlanet.InternalLights[0].color = new OpenTK.Vector3(0, 0, 0);
                PlanetLight = 0.1f;
            }
            else
            {
                TheSun.InternalLights[0].color = SunLightDef.ToOVector();
                ThePlanet.InternalLights[0].color = (PlanetLightDef * (dist / 180f)).ToOVector();
                PlanetLight = (dist / 180f);
            }
            // TODO: Set planet lightness by sun position (farther from planet = brighter!)
            // TODO: Set sun lightness by planet position (too closer to planet = darker, redder)
            TheWorld.TickWorld(delta);
        }
    }
}
