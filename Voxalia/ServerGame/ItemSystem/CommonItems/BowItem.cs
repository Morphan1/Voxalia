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

        public float DrawMinimum = 0.5f;

        public float DrawRate = 1f;

        public float FireStrength = 1f;

        public override void PrepItem(Entity entity, ItemStack item)
        {
            if (!item.SharedAttributes.ContainsKey("charge"))
            {
                item.SharedAttributes.Add("charge", 1);
                item.SharedAttributes.Add("drawrate", DrawRate);
                item.SharedAttributes.Add("drawmin", DrawMinimum);
            }
        }

        public override void Tick(Entity entity, ItemStack item)
        {
        }
        
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
            if (player.ItemStartClickTime == -2)
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
                player.ItemStartClickTime = -1;
                return;
            }
            float drawRate = DrawRate;
            float dw2 = 1;
            if (item.SharedAttributes.TryGetValue("drawrate", out dw2))
            {
                drawRate *= dw2;
            }
            float drawMin = DrawMinimum;
            float dm2 = 1;
            if (item.SharedAttributes.TryGetValue("drawmin", out dm2))
            {
                drawMin *= dm2;
            }
            double timeStretched = Math.Min((player.TheWorld.GlobalTickTime - player.ItemStartClickTime) * drawRate, 3) + drawMin;
            player.ItemStartClickTime = -1;
            if (timeStretched < DrawMinimum + 0.25)
            {
                return;
            }
            ArrowEntity ae = new ArrowEntity(player.TheWorld);
            ae.SetPosition(player.GetEyePosition());
            ae.NoCollide.Add(player.EID);
            Location forward = player.ForwardVector();
            ae.SetVelocity(forward * timeStretched * 20 * FireStrength);
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
