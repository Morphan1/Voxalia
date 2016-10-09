//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;
using FreneticScript.TagHandlers.Objects;
using BEPUutilities;
using FreneticScript.TagHandlers;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class BowItem: BaseItemInfo
    {
        public BowItem()
        {
            Name = "bow";
        }

        public double DrawMinimum = 0.5f;

        public double DrawRate = 1f;

        public double FireStrength = 1f;

        public override void PrepItem(Entity entity, ItemStack item)
        {
            bool has = item.SharedAttributes.ContainsKey("charge");
            BooleanTag bt = has ? BooleanTag.TryFor(item.SharedAttributes["charge"]): null;
            if (!has || bt == null || !bt.Internal)
            {
                item.SharedAttributes.Add("charge", new BooleanTag(true));
                item.SharedAttributes.Add("drawrate", new NumberTag(DrawRate));
                item.SharedAttributes.Add("drawmin", new NumberTag(DrawMinimum));
                item.SharedAttributes.Add("cspeedm", new NumberTag(0.5f));
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
            player.ItemStartClickTime = player.TheRegion.GlobalTickTime;
            player.ItemDoSpeedMod = true;
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
            player.ItemDoSpeedMod = false;
            if (player.ItemStartClickTime < 0)
            {
                player.ItemStartClickTime = -1;
                return;
            }
            double drawRate = DrawRate;
            TemplateObject dw2;
            if (item.SharedAttributes.TryGetValue("drawrate", out dw2))
            {
                NumberTag nt = NumberTag.TryFor(dw2);
                if (nt != null)
                {
                    drawRate = (double)nt.Internal;
                }
            }
            double drawMin = DrawMinimum;
            TemplateObject dm2;
            if (item.SharedAttributes.TryGetValue("drawmin", out dm2))
            {
                NumberTag nt = NumberTag.TryFor(dm2);
                if (nt != null)
                {
                    drawMin = (double)nt.Internal;
                }
            }
            double timeStretched = Math.Min((player.TheRegion.GlobalTickTime - player.ItemStartClickTime) * drawRate, 3) + drawMin;
            player.ItemStartClickTime = -1;
            if (timeStretched < DrawMinimum + 0.25)
            {
                return;
            }
            SpawnArrow(player, item, timeStretched);
        }

        public virtual ArrowEntity SpawnArrow(PlayerEntity player, ItemStack item, double timeStretched)
        {
            ArrowEntity ae = new ArrowEntity(player.TheRegion);
            ae.SetPosition(player.GetEyePosition());
            ae.NoCollide.Add(player.EID);
            Location forward = player.ForwardVector();
            ae.SetVelocity(forward * timeStretched * 20 * FireStrength);
            Matrix lookatlh = Utilities.LookAtLH(Location.Zero, forward, Location.UnitZ);
            lookatlh.Transpose();
            ae.Angles = Quaternion.CreateFromRotationMatrix(lookatlh);
            ae.Angles *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, 90f * (double)Utilities.PI180);
            player.TheRegion.SpawnEntity(ae);
            return ae;
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
            player.ItemDoSpeedMod = false;
        }

        public override void SwitchTo(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                // TODO: non-player support
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            double speedm = 1f;
            if (item.SharedAttributes.ContainsKey("cspeedm"))
            {
                NumberTag nt = NumberTag.TryFor(item.SharedAttributes["cspeedm"]);
                if (nt != null)
                {
                    speedm = (double)nt.Internal;
                }
            }
            player.ItemSpeedMod = speedm;
            player.ItemDoSpeedMod = false;
        }
    }
}
