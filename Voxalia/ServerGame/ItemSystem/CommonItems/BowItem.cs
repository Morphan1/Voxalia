using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class BowItem: BaseItemInfo
    {
        public BowItem()
        {
            Name = "bow";
        }

        public override void PrepItem(PlayerEntity player, ItemStack item)
        {
        }

        public override void Tick(PlayerEntity player, ItemStack item)
        {
        }

        public float Speed = 10;

        public override void Click(PlayerEntity player, ItemStack item)
        {
            ArrowEntity ae = new ArrowEntity(player.TheServer);
            ae.SetPosition(player.GetEyePosition());
            ae.NoCollide.Add(player.EID);
            Location forward = player.ForwardVector();
            ae.SetVelocity(forward * 10);
            BEPUutilities.Matrix lookatlh = Utilities.LookAtLH(Location.Zero, forward, Location.UnitZ);
            lookatlh.Transpose();
            ae.Angles = BEPUutilities.Quaternion.CreateFromRotationMatrix(lookatlh);
            ae.Angles *= BEPUutilities.Quaternion.CreateFromAxisAngle(BEPUutilities.Vector3.UnitX, 90f * (float)Utilities.PI180);
            player.TheServer.SpawnEntity(ae);
        }

        public override void AltClick(PlayerEntity player, ItemStack item)
        {
        }

        public override void ReleaseClick(PlayerEntity player, ItemStack item)
        {
        }

        public override void Use(PlayerEntity player, ItemStack item)
        {
        }

        public override void SwitchFrom(PlayerEntity player, ItemStack item)
        {
        }

        public override void SwitchTo(PlayerEntity player, ItemStack item)
        {
        }
    }
}
