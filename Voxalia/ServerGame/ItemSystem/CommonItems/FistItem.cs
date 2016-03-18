﻿using Voxalia.Shared;
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
    public class FistItem: BaseItemInfo
    {
        public FistItem()
            : base()
        {
            Name = "fist";
        }

        public override void PrepItem(Entity player, ItemStack item)
        {
        }

        public override void AltClick(Entity player, ItemStack item)
        {
        }

        public override void Click(Entity ent, ItemStack item)
        {
            if (!(ent is PlayerEntity))
            {
                // TODO: update to generic entity
                return;
            }
            PlayerEntity player = (PlayerEntity)ent;
            Location eye = player.GetEyePosition();
            Location forw = player.ForwardVector();
            RayCastResult rcr;
            bool h = player.TheRegion.SpecialCaseRayTrace(eye, forw, 5, MaterialSolidity.ANY, player.IgnoreThis, out rcr);
            if (h)
            {
                if (rcr.HitObject != null && rcr.HitObject is EntityCollidable && ((EntityCollidable)rcr.HitObject).Entity != null)
                {
                    // TODO: Damage
                    return;
                }
                if (!player.Mode.GetDetails().CanBreak)
                {
                    return;
                }
                bool breakIt = false;
                Location block = (new Location(rcr.HitData.Location) - new Location(rcr.HitData.Normal).Normalize() * 0.01).GetBlockLocation();
                if (block != player.BlockBreakTarget)
                {
                    player.BlockBreakStarted = 0;
                    player.BlockBreakTarget = block;
                }
                if (player.Mode.GetDetails().FastBreak)
                {
                    breakIt = player.TheRegion.GlobalTickTime - player.LastBlockBreak >= 0.2;
                }
                else
                {
                    if (player.BlockBreakStarted <= 0)
                    {
                        player.BlockBreakStarted = player.TheRegion.GlobalTickTime;
                    }
                    breakIt = player.TheRegion.GlobalTickTime - player.BlockBreakStarted > player.TheRegion.GetBlockMaterial(block).GetBreakTime();
                }
                if (breakIt)
                {
                    Material mat = player.TheRegion.GetBlockMaterial(block);
                    if (player.TheRegion.IsAllowedToBreak(player, block, mat))
                    {
                        player.TheRegion.BreakNaturally(block);
                        player.Network.SendPacket(new DefaultSoundPacketOut(block, DefaultSound.BREAK, (byte)mat.Sound()));
                        player.LastBlockBreak = player.TheRegion.GlobalTickTime;
                    }
                    player.BlockBreakStarted = 0;
                }
            }
        }

        public override void ReleaseClick(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                // TODO: non-player support
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            player.LastBlockBreak = 0;
            player.BlockBreakStarted = 0;
        }

        public override void ReleaseAltClick(Entity player, ItemStack item)
        {
        }

        public override void Use(Entity player, ItemStack item)
        {
        }

        public override void SwitchFrom(Entity player, ItemStack item)
        {
        }

        public override void SwitchTo(Entity player, ItemStack item)
        {
        }

        public override void Tick(Entity player, ItemStack item)
        {
        }
    }
}
