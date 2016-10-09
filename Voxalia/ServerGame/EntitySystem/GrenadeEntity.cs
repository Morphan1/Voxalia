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
using Voxalia.ServerGame.WorldSystem;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.Shared;

namespace Voxalia.ServerGame.EntitySystem
{
    public abstract class GrenadeEntity : PhysicsEntity
    {
        public GrenadeEntity(Region tregion) :
            base(tregion)
        {
            Shape = new CylinderShape(0.2f, 0.05f);
            Bounciness = 0.95f;
            SetMass(1);
        }

        public override NetworkEntityType GetNetType()
        {
            return NetworkEntityType.GRENADE;
        }

        public override byte[] GetNetData()
        {
            return GetPhysicsNetData();
        }
    }
}
