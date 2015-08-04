using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.JointSystem;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    class OpenHandItem: BaseItemInfo
    {
        public OpenHandItem()
            : base()
        {
            Name = "open_hand";
        }

        public override void PrepItem(Entity entity, ItemStack item)
        {
        }

        public override void AltClick(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                // TODO: non-player support
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            if (player.GrabJoint != null)
            {
                player.TheRegion.DestroyJoint(player.GrabJoint);
            }
            player.GrabJoint = null;
        }

        public override void Click(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                // TODO: non-player support
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            Location end = player.GetEyePosition() + player.ForwardVector() * 5;
            CollisionResult cr = player.TheRegion.Collision.CuboidLineTrace(new Location(0.1, 0.1, 0.1), player.GetEyePosition(), end, player.IgnoreThis);
            if (cr.Hit && cr.HitEnt != null)
            {
                // TODO: handle static world impact
                PhysicsEntity pe = (PhysicsEntity)cr.HitEnt.Tag;
                if (pe.GetMass() > 0 && pe.GetMass() < 200)
                {
                    Grab(player, pe, cr.Position);
                }
                else
                {
                    // If (HandHold) { Grab(player, pe, cr.Position); }
                }
            }
        }

        public void Grab(PlayerEntity player, PhysicsEntity pe, Location hit)
        {
            AltClick(player, null);
            JointBallSocket jbs = new JointBallSocket(player.CursorMarker, pe, hit);
            player.TheRegion.AddJoint(jbs);
            player.GrabJoint = jbs;
        }

        public override void ReleaseClick(Entity entity, ItemStack item)
        {
        }

        public override void ReleaseAltClick(Entity entity, ItemStack item)
        {
        }

        public override void Use(Entity entity, ItemStack item)
        {
        }

        public override void SwitchFrom(Entity entity, ItemStack item)
        {
            AltClick(entity, item);
        }

        public override void SwitchTo(Entity entity, ItemStack item)
        {
        }

        public override void Tick(Entity entity, ItemStack item)
        {
        }
    }
}
