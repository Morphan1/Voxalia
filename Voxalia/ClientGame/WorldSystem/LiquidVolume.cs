//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.UpdateableSystems.ForceFields;
using BEPUutilities;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.UpdateableSystems;
using Voxalia.Shared;
using BEPUphysics.BroadPhaseEntries;
using Voxalia.Shared.Collision;
using BEPUutilities.DataStructures;

namespace Voxalia.ClientGame.WorldSystem
{
    public class LiquidVolume : Updateable, IDuringForcesUpdateable
    {
        public Region TheRegion;

        public LiquidVolume(Region tregion)
        {
            TheRegion = tregion;
        }

        public void Update(double dt)
        {
            ReadOnlyList<Entity> ents = TheRegion.PhysicsWorld.Entities; // TODO: Direct/raw read?
            TheRegion.PhysicsWorld.ParallelLooper.ForLoop(0, ents.Count, (i) =>
            {
                ApplyLiquidForcesTo(ents[i], dt);
            });
        }

        void ApplyLiquidForcesTo(Entity e, double dt)
        {
            if (e.Mass <= 0)
            {
                return;
            }
            Location min = new Location(e.CollisionInformation.BoundingBox.Min);
            Location max = new Location(e.CollisionInformation.BoundingBox.Max);
            min = min.GetBlockLocation();
            max = max.GetUpperBlockBorder();
            for (int x = (int)min.X; x < max.X; x++)
            {
                for (int y = (int)min.Y; y < max.Y; y++)
                {
                    for (int z = (int)min.Z; z < max.Z; z++)
                    {
                        Location c = new Location(x, y, z);
                        Material mat = (Material)TheRegion.GetBlockInternal(c).BlockMaterial;
                        if (mat.GetSolidity() != MaterialSolidity.LIQUID)
                        {
                            continue;
                        }
                        // TODO: Account for block shape?
                        double vol = e.CollisionInformation.Shape.Volume;
                        double dens = (e.Mass / vol);
                        double WaterDens = 5; // TODO: Read from material. // TODO: Sanity of values.
                        float modifier = (float)(WaterDens / dens);
                        float submod = 0.125f;
                        // TODO: Tracing accuracy!
                        Vector3 impulse = -(TheRegion.PhysicsWorld.ForceUpdater.Gravity + TheRegion.GravityNormal.ToBVector() * 0.4f) * e.Mass * dt * modifier * submod;
                        // TODO: Don't apply smaller logic this if scale is big!
                        for (float x2 = 0.25f; x2 < 1; x2 += 0.5f)
                        {
                            for (float y2 = 0.25f; y2 < 1; y2 += 0.5f)
                            {
                                for (float z2 = 0.25f; z2 < 1; z2 += 0.5f)
                                {
                                    Location lc = c + new Location(x2, y2, z2);
                                    RayHit rh;
                                    if (e.CollisionInformation.RayCast(new Ray(lc.ToBVector(), new Vector3(0, 0, 1)), 0.01f, out rh)) // TODO: Efficiency!
                                    {
                                        Vector3 center = lc.ToBVector();
                                        e.ApplyImpulse(ref center, ref impulse);
                                        e.ModifyLinearDamping(mat.GetSpeedMod());
                                        e.ModifyAngularDamping(mat.GetSpeedMod());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
