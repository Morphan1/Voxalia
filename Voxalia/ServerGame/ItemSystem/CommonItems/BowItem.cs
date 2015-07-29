using System;
using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;
using BEPUutilities;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class BowItem: BaseItemInfo
    {
        public BowItem()
        {
            Name = "bow";
        }

        public override void PrepItem(Entity entity, ItemStack item)
        {
        }

        public override void Tick(Entity entity, ItemStack item)
        {
        }

        public float Speed = 10;

        public override void Click(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                // TODO: non-player support
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            if (player.ItemStartClickTime >= 0)
            {
                return;
            }
            player.ItemStartClickTime = player.TheWorld.GlobalTickTime;
        }

        public override void AltClick(Entity entity, ItemStack item)
        {
        }

        public override void ReleaseClick(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                // TODO: non-player support
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            if (player.ItemStartClickTime < 0)
            {
                return;
            }
            double timeStretched = Math.Min(player.TheWorld.GlobalTickTime - player.ItemStartClickTime, 3) + 0.5;
            player.ItemStartClickTime = -1;
            if (timeStretched < 0.75)
            {
                return;
            }
            ArrowEntity ae = new ArrowEntity(player.TheWorld);
            ae.SetPosition(player.GetEyePosition());
            ae.NoCollide.Add(player.EID);
            Location forward = player.ForwardVector();
            ae.SetVelocity(forward * timeStretched * 20);
            Matrix lookatlh = Utilities.LookAtLH(Location.Zero, forward, Location.UnitZ);
            lookatlh.Transpose();
            ae.Angles = Quaternion.CreateFromRotationMatrix(lookatlh);
            ae.Angles *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, 90f * (float)Utilities.PI180);
            player.TheWorld.SpawnEntity(ae);
        }

        public override void ReleaseAltClick(Entity entity, ItemStack item)
        {
        }

        public override void Use(Entity entity, ItemStack item)
        {
        }

        public override void SwitchFrom(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                // TODO: non-player support
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            player.ItemStartClickTime = -1;
        }

        public override void SwitchTo(Entity entity, ItemStack item)
        {
        }
    }
}
