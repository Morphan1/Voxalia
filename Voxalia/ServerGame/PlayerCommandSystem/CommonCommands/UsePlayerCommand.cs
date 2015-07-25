using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;

namespace Voxalia.ServerGame.PlayerCommandSystem.CommonCommands
{
    class UsePlayerCommand: AbstractPlayerCommand
    {
        public UsePlayerCommand()
        {
            Name = "use";
            Silent = true;
        }

        Server TheServer;

        public bool TryForUseValidity(BroadPhaseEntry entry)
        {
            if (entry.CollisionRules.Group == CollisionUtil.Solid
                || entry.CollisionRules.Group == CollisionUtil.Item)
            {
                return true;
            }
            if (entry.CollisionRules.Group == CollisionUtil.Trigger
                && ((EntityCollidable)entry).Entity.Tag is EntityUseable)
            {
                return true;
            }
            return false;
        }

        public override void Execute(PlayerCommandEntry entry)
        {
            TheServer = entry.Player.TheServer;
            Location forw = entry.Player.ForwardVector();
            CollisionResult cr = entry.Player.TheWorld.Collision.CuboidLineTrace(new Location(0.1f),
                entry.Player.GetEyePosition(), entry.Player.GetEyePosition() + forw * 5, TryForUseValidity);
            if (cr.Hit && cr.HitEnt != null && cr.HitEnt.Tag is EntityUseable)
            {
                if (!((EntityUseable)cr.HitEnt.Tag).Use(entry.Player))
                {
                    entry.Player.Network.SendMessage("Can't use that at this time!");
                }
            }
            else
            {
                entry.Player.Network.SendMessage("Nothing there to use!");
            }
        }
    }
}
