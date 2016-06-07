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
