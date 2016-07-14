using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public abstract class BlockBreakerItem : GenericItem
    {
        public abstract MaterialBreaker GetBreaker();

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
            bool h = player.TheRegion.SpecialCaseRayTrace(eye, forw, 5, MaterialSolidity.FULLSOLID, player.IgnoreThis, out rcr);
            if (h)
            {
                bool hitIt = (player.TheRegion.GlobalTickTime - player.LastBlockBreak) > 0.5f; // TODO: Arbitrary constant!
                if (hitIt && rcr.HitObject != null && rcr.HitObject is EntityCollidable && ((EntityCollidable)rcr.HitObject).Entity != null)
                {
                    BEPUphysics.Entities.Entity hitent = ((EntityCollidable)rcr.HitObject).Entity;
                    Vector3 force = forw.ToBVector() * 75; // TODO: Arbitrary constant!
                    hitent.ApplyImpulse(ref rcr.HitData.Location, ref force);
                    // TODO: Damage
                    player.LastBlockBreak = player.TheRegion.GlobalTickTime;
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
                Material mat = player.TheRegion.GetBlockMaterial(block);
                if (player.Mode.GetDetails().FastBreak)
                {
                    breakIt = player.TheRegion.GlobalTickTime - player.LastBlockBreak >= 0.2; // TODO: Arbitrary constant!
                }
                else
                {
                    if (player.BlockBreakStarted <= 0)
                    {
                        player.BlockBreakStarted = player.TheRegion.GlobalTickTime;
                    }
                    float bt = mat.GetBreakTime();
                    MaterialBreaker breaker = GetBreaker();
                    MaterialBreaker matbreaker = mat.GetBreaker();
                    if (matbreaker == MaterialBreaker.NON_BREAKABLE)
                    {
                        return;
                    }
                    // TODO: arbitrary constants!
                    if (breaker == MaterialBreaker.PICKAXE)
                    {
                        if (matbreaker == MaterialBreaker.PICKAXE)
                        {
                            bt *= 0.3f;
                        }
                        else if (!mat.GetBreaksFromOtherTools())
                        {
                            return;
                        }
                        else if (matbreaker == MaterialBreaker.AXE)
                        {
                            bt *= 0.7f;
                        }
                        else if (matbreaker == MaterialBreaker.SHOVEL)
                        {
                            bt *= 1.2f;
                        }
                        else if (matbreaker == MaterialBreaker.HAND)
                        {
                            bt *= 0.87f;
                        }
                    }
                    // else: hand! Default values!
                    breakIt = (player.TheRegion.GlobalTickTime - player.BlockBreakStarted) > bt;
                }
                if (breakIt)
                {
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
    }
}
