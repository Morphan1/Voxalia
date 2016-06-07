using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared.Collision;
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.ServerGame.OtherSystems;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class FistItem: BlockBreakerItem
    {
        public FistItem()
            : base()
        {
            Name = "fist";
        }

        public override MaterialBreaker GetBreaker()
        {
            return MaterialBreaker.HAND;
        }
    }
}
