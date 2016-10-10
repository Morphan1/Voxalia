//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using Voxalia.Shared;
using Voxalia.ClientGame.EntitySystem;
using BEPUutilities;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.ClientGame.GraphicsSystems.LightingSystem;
using Voxalia.ClientGame.OtherSystems;

namespace Voxalia.ClientGame.ClientMainSystem
{
    public partial class Client
    {
        public SkyLight TheSun = null;
        
        public SkyLight ThePlanet = null;

        public SkyLight TheSunClouds = null;

        // Note: the client only has one region loaded at any given time.
        public Region TheRegion = null;

        public const float SunLightMod = 1.5f;
        public const float SunLightModDirect = 3.0f;

        public Location SunLightDef = Location.One * SunLightMod * 0.5;
        public Location CloudSunLightDef = Location.One * SunLightMod * 0.5;

        public Location PlanetLightDef = new Location(0.75, 0.3, 0) * 0.25f;

        public void BuildWorld()
        {
            if (TheSun != null)
            {
                TheSun.Destroy();
                MainWorldView.Lights.Remove(TheSun);
                ThePlanet.Destroy();
                MainWorldView.Lights.Remove(ThePlanet);
                TheSunClouds.Destroy();
                MainWorldView.Lights.Remove(TheSunClouds);
            }
            // TODO: DESTROY OLD REGION!
            // TODO: Radius -> max view rad * 2
            // TODO: Size -> max view rad * 2 + 30 * 2
            TheSun = new SkyLight(Location.Zero, Chunk.CHUNK_SIZE * 30, SunLightDef, new Location(0, 0, -1), Chunk.CHUNK_SIZE * 35, false);
            MainWorldView.Lights.Add(TheSun);
            // TODO: Separate cloud quality CVar?
            TheSunClouds = new SkyLight(Location.Zero, Chunk.CHUNK_SIZE * 30, CloudSunLightDef, new Location(0, 0, -1), Chunk.CHUNK_SIZE * 35, true);
            MainWorldView.Lights.Add(TheSunClouds);
            // TODO: Separate planet quality CVar?
            ThePlanet = new SkyLight(Location.Zero, Chunk.CHUNK_SIZE * 30, PlanetLightDef, new Location(0, 0, -1), Chunk.CHUNK_SIZE * 35, false);
            MainWorldView.Lights.Add(ThePlanet);
            TheRegion = new Region();
            TheRegion.TheClient = this;
            TheRegion.BuildWorld();
            Player = new PlayerEntity(TheRegion);
            TheRegion.SpawnEntity(Player);
        }

        public void onCloudShadowChanged(object obj, EventArgs e)
        {
            bool cloudsready = MainWorldView.Lights.Contains(TheSunClouds);
            if (cloudsready && !CVars.r_cloudshadows.ValueB)
            {
                MainWorldView.Lights.Remove(TheSunClouds);
                SunLightDef = Location.One * SunLightMod;
            }
            else if (!cloudsready && CVars.r_cloudshadows.ValueB)
            {
                MainWorldView.Lights.Add(TheSunClouds);
                SunLightDef = Location.One * SunLightMod * 0.5;
            }
        }

        public Location SunAngle = new Location(0, -75, 0);

        public Location PlanetAngle = new Location(0, -56, 90);

        public float PlanetLight = 1;

        public float PlanetSunDist = 0;

        public Location BaseAmbient = new Location(0.1, 0.1, 0.1);

        public float sl_min = 0;
        public float sl_max = 1;

        Location PlanetDir;

        public void TickWorld(double delta)
        {
            rTicks++;
            if (rTicks >= CVars.r_shadowpace.ValueI)
            {
                // TODO: Z+ -> max view rad + 30
                TheSun.Direction = Utilities.ForwardVector_Deg(SunAngle.Yaw, SunAngle.Pitch);
                TheSun.Reposition(Player.GetPosition().GetBlockLocation() - TheSun.Direction * 30 * 6);
                TheSunClouds.Direction = TheSun.Direction;
                TheSunClouds.Reposition(TheSun.EyePos);
                PlanetDir = Utilities.ForwardVector_Deg(PlanetAngle.Yaw, PlanetAngle.Pitch);
                ThePlanet.Direction = PlanetDir;
                TheSunClouds.Reposition(Player.GetPosition().GetBlockLocation() - ThePlanet.Direction * 30 * 6);
                Quaternion diff;
                Vector3 tsd = TheSun.Direction.ToBVector();
                Vector3 tpd = PlanetDir.ToBVector();
                Quaternion.GetQuaternionBetweenNormalizedVectors(ref tsd, ref tpd, out diff);
                PlanetSunDist = (float)Quaternion.GetAngleFromQuaternion(ref diff) / (float)Utilities.PI180;
                if (PlanetSunDist < 75)
                {
                    TheSun.InternalLights[0].color = new OpenTK.Vector3((float)Math.Min(SunLightDef.X * (PlanetSunDist / 15), 1),
                        (float)Math.Min(SunLightDef.Y * (PlanetSunDist / 20), 1), (float)Math.Min(SunLightDef.Z * (PlanetSunDist / 60), 1));
                    TheSunClouds.InternalLights[0].color = new OpenTK.Vector3((float)Math.Min(CloudSunLightDef.X * (PlanetSunDist / 15), 1),
                        (float)Math.Min(CloudSunLightDef.Y * (PlanetSunDist / 20), 1), (float)Math.Min(CloudSunLightDef.Z * (PlanetSunDist / 60), 1));
                    ThePlanet.InternalLights[0].color = new OpenTK.Vector3(0, 0, 0);
                }
                else
                {
                    TheSun.InternalLights[0].color = ClientUtilities.Convert(SunLightDef);
                    TheSunClouds.InternalLights[0].color = ClientUtilities.Convert(CloudSunLightDef);
                    ThePlanet.InternalLights[0].color = ClientUtilities.Convert(PlanetLightDef * Math.Min((PlanetSunDist / 180f), 1f));
                }
                PlanetLight = PlanetSunDist / 180f;
                if (SunAngle.Pitch < 10 && SunAngle.Pitch > -30)
                {
                    float rel = 30 + (float)SunAngle.Pitch;
                    if (rel == 0)
                    {
                        rel = 0.00001f;
                    }
                    rel = 1f - (rel / 40f);
                    rel = Math.Max(Math.Min(rel, 1f), 0f);
                    float rel2 = Math.Max(Math.Min(rel * 1.5f, 1f), 0f);
                    TheSun.InternalLights[0].color = new OpenTK.Vector3(TheSun.InternalLights[0].color.X * rel2, TheSun.InternalLights[0].color.Y * rel, TheSun.InternalLights[0].color.Z * rel);
                    TheSunClouds.InternalLights[0].color = new OpenTK.Vector3(TheSunClouds.InternalLights[0].color.X * rel2, TheSunClouds.InternalLights[0].color.Y * rel, TheSunClouds.InternalLights[0].color.Z * rel);
                    MainWorldView.DesaturationAmount = (1f - rel) * 0.75f;
                    MainWorldView.ambient = BaseAmbient * ((1f - rel) * 0.5f + 0.5f);
                    sl_min = 0.2f - (1f - rel) * (0.2f - 0.05f);
                    sl_max = 0.8f - (1f - rel) * (0.8f - 0.15f);
                }
                else if (SunAngle.Pitch >= 10)
                {
                    TheSun.InternalLights[0].color = new OpenTK.Vector3(0, 0, 0);
                    TheSunClouds.InternalLights[0].color = new OpenTK.Vector3(0, 0, 0);
                    MainWorldView.DesaturationAmount = 0.75f;
                    MainWorldView.ambient = BaseAmbient * 0.5f;
                    sl_min = 0.05f;
                    sl_max = 0.15f;
                }
                else
                {
                    sl_min = 0.2f;
                    sl_max = 0.8f;
                    MainWorldView.DesaturationAmount = 0f;
                    MainWorldView.ambient = BaseAmbient;
                    TheSun.InternalLights[0].color = ClientUtilities.Convert(SunLightDef);
                    TheSunClouds.InternalLights[0].color = ClientUtilities.Convert(CloudSunLightDef);
                }
                rTicks = 0;
                shouldRedrawShadows = true;
            }
            TheRegion.TickWorld(delta);
        }
    }
}
