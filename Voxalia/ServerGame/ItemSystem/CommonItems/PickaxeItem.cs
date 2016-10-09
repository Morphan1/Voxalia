//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared.Collision;
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.ServerGame.OtherSystems;
using System;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class PickaxeItem: BlockBreakerItem
    {
        public PickaxeItem()
            : base()
        {
            Name = "pickaxe";
        }

        public override MaterialBreaker GetBreaker()
        {
            return MaterialBreaker.PICKAXE;
        }
    }
}
