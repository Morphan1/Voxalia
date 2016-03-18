using System;
using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ServerGame.ServerMainSystem;
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.Settings;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.JointSystem;
using Voxalia.ServerGame.NetworkSystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using BEPUutilities.Threading;
using Voxalia.ServerGame.WorldSystem.SimpleGenerator;
using System.Threading;
using System.Threading.Tasks;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.Shared.Collision;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.ItemSystem.CommonItems;

namespace Voxalia.ServerGame.WorldSystem
{
    public partial class Region
    {
        public void SurroundBlockPhysics(Location start)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        RunBlockPhysics(start + new Location(x, y, z));
                    }
                }
            }
            foreach (Entity e in GetEntitiesInRadius(start + new Location(0.5), 3f))
            {
                e.PotentialActivate();
            }
        }

        public void PhysicsSetBlock(Location block, Material mat, byte dat = 0, byte paint = 0, BlockDamage damage = BlockDamage.NONE)
        {
            SetBlockMaterial(block, mat, dat, paint, (byte)(BlockFlags.EDITED | BlockFlags.NEEDS_RECALC), damage);
            TheServer.Schedule.ScheduleSyncTask(() => { SurroundBlockPhysics(block); }, 0.1);
        }

        int remPercFor(byte b)
        {
            switch (b)
            {
                case 0:
                    return 100;
                case 1:
                    return 84;
                case 2:
                    return 68;
                case 3:
                    return 50;
                case 4:
                    return 34;
                case 5:
                    return 13;
                default:
                    return 0;
            }
        }

        public void RunBlockPhysics(Location block)
        {
            block = block.GetBlockLocation();
            BlockInternal c = GetBlockInternal(block);
            if (((BlockFlags)c.BlockLocalData).HasFlag(BlockFlags.NEEDS_RECALC))
            {
                c.BlockLocalData = (byte)(c.BlockLocalData & ~((byte)BlockFlags.NEEDS_RECALC));
                SetBlockMaterial(block, c, false, false, true);
            }
            Material cmat = (Material)c.BlockMaterial;
            if (cmat.ShouldSpread())
            {
                int remainingperc = remPercFor(c.BlockData);
                if (remainingperc == 0)
                {
                    SetBlockMaterial(block, Material.AIR);
                }
                else
                {
                    Location lxp = block + new Location(1, 0, 0);
                    BlockInternal xp = GetBlockInternal(lxp);
                    Material mxp = (Material)xp.BlockMaterial;
                    Location lxm = block + new Location(-1, 0, 0);
                    BlockInternal xm = GetBlockInternal(lxm);
                    Material mxm = (Material)xm.BlockMaterial;
                    Location lyp = block + new Location(0, 1, 0);
                    BlockInternal yp = GetBlockInternal(lyp);
                    Material myp = (Material)yp.BlockMaterial;
                    Location lym = block + new Location(0, -1, 0);
                    BlockInternal ym = GetBlockInternal(lym);
                    Material mym = (Material)ym.BlockMaterial;
                    Location lzm = block + new Location(0, 0, -1);
                    BlockInternal zm = GetBlockInternal(lzm);
                    Material mzm = (Material)zm.BlockMaterial;
                    if (mzm == Material.AIR)
                    {
                        PhysicsSetBlock(lzm, cmat);
                        PhysicsSetBlock(block, Material.AIR);
                    }
                    else if (mzm == cmat && zm.BlockData != 0)
                    {
                        CombineWater(remainingperc, cmat, remPercFor(zm.BlockData), block, lzm, c.BlockPaint);
                    }
                    else if (mxp == Material.AIR && myp == Material.AIR && mxm == Material.AIR && mym == Material.AIR)
                    {
                        if (remainingperc == 100)
                        {
                            PhysicsSetBlock(lxp, cmat, 5, c.BlockPaint);
                            PhysicsSetBlock(lxm, cmat, 5, c.BlockPaint);
                            PhysicsSetBlock(lyp, cmat, 5, c.BlockPaint);
                            PhysicsSetBlock(lym, cmat, 5, c.BlockPaint);
                            PhysicsSetBlock(block, cmat, 4, c.BlockPaint);
                        }
                        else if (remainingperc == 84)
                        {
                            PhysicsSetBlock(lxp, cmat, 5, c.BlockPaint);
                            PhysicsSetBlock(lxm, cmat, 5, c.BlockPaint);
                            PhysicsSetBlock(lyp, cmat, 5, c.BlockPaint);
                            PhysicsSetBlock(lym, cmat, 5, c.BlockPaint);
                            PhysicsSetBlock(block, cmat, 5, c.BlockPaint);
                        }
                        else if (remainingperc == 68)
                        {
                            PhysicsSetBlock(lxp, cmat, 5, c.BlockPaint);
                            PhysicsSetBlock(lxm, cmat, 5, c.BlockPaint);
                            PhysicsSetBlock(lyp, cmat, 5, c.BlockPaint);
                            PhysicsSetBlock(block, cmat, 5, c.BlockPaint);
                        }
                        else if (remainingperc == 50)
                        {
                            PhysicsSetBlock(lxp, cmat, 5, c.BlockPaint);
                            PhysicsSetBlock(lxm, cmat, 5, c.BlockPaint);
                            PhysicsSetBlock(block, cmat, 5, c.BlockPaint);
                        }
                        else if (remainingperc == 34)
                        {
                            PhysicsSetBlock(lxp, cmat, 5, c.BlockPaint);
                            PhysicsSetBlock(block, cmat, 5, c.BlockPaint);
                        }
                        // 13 doesn't move!
                    }
                    else if (mxp == Material.AIR && myp == Material.AIR && mxm == Material.AIR)
                    {
                        LiquidSpread3(block, cmat, lxp, lyp, lxm, remainingperc, c.BlockPaint);
                    }
                    else if (mxp == Material.AIR && myp == Material.AIR && mym == Material.AIR)
                    {
                        LiquidSpread3(block, cmat, lxp, lyp, lym, remainingperc, c.BlockPaint);
                    }
                    else if (mxp == Material.AIR && mym == Material.AIR && mxm == Material.AIR)
                    {
                        LiquidSpread3(block, cmat, lxp, lym, lxm, remainingperc, c.BlockPaint);
                    }
                    else if (mym == Material.AIR && myp == Material.AIR && mxm == Material.AIR)
                    {
                        LiquidSpread3(block, cmat, lym, lyp, lxm, remainingperc, c.BlockPaint);
                    }
                    else if (mym == Material.AIR && myp == Material.AIR)
                    {
                        LiquidSpread2(block, cmat, lym, lyp, remainingperc, c.BlockPaint);
                    }
                    else if (mym == Material.AIR && mxp == Material.AIR)
                    {
                        LiquidSpread2(block, cmat, lym, lxp, remainingperc, c.BlockPaint);
                    }
                    else if (mym == Material.AIR && mxm == Material.AIR)
                    {
                        LiquidSpread2(block, cmat, lym, lxm, remainingperc, c.BlockPaint);
                    }
                    else if (myp == Material.AIR && mxm == Material.AIR)
                    {
                        LiquidSpread2(block, cmat, lyp, lxm, remainingperc, c.BlockPaint);
                    }
                    else if (myp == Material.AIR && mxp == Material.AIR)
                    {
                        LiquidSpread2(block, cmat, lyp, lxp, remainingperc, c.BlockPaint);
                    }
                    else if (mxp == Material.AIR && mxm == Material.AIR)
                    {
                        LiquidSpread2(block, cmat, lxp, lxm, remainingperc, c.BlockPaint);
                    }
                    else if (mxp == Material.AIR)
                    {
                        LiquidSpread1(block, cmat, lxp, remainingperc, c.BlockPaint);
                    }
                    else if (mxm == Material.AIR)
                    {
                        LiquidSpread1(block, cmat, lxm, remainingperc, c.BlockPaint);
                    }
                    else if (myp == Material.AIR)
                    {
                        LiquidSpread1(block, cmat, lyp, remainingperc, c.BlockPaint);
                    }
                    else if (mym == Material.AIR)
                    {
                        LiquidSpread1(block, cmat, lym, remainingperc, c.BlockPaint);
                    }
                    else
                    {
                        int rxp = remPercFor(xp.BlockData);
                        int rxm = remPercFor(xm.BlockData);
                        int ryp = remPercFor(yp.BlockData);
                        int rym = remPercFor(ym.BlockData);
                        if (mxp == cmat && rxp < remainingperc)
                        {
                            CombineWaterSide(remainingperc, cmat, rxp, block, lxp, c.BlockPaint);
                        }
                        else if (mxm == cmat && rxm < remainingperc)
                        {
                            CombineWaterSide(remainingperc, cmat, rxm, block, lxm, c.BlockPaint);
                        }
                        else if (myp == cmat && ryp < remainingperc)
                        {
                            CombineWaterSide(remainingperc, cmat, ryp, block, lyp, c.BlockPaint);
                        }
                        else if (mym == cmat && rym < remainingperc)
                        {
                            CombineWaterSide(remainingperc, cmat, rym, block, lym, c.BlockPaint);
                        }
                    }
                }
            }
        }

        void CombineWaterSide(int rempart, Material cmat, int remperc, Location block, Location one, byte paint)
        {
            if (remperc == 68)
            {
                if (rempart == 100)
                {
                    PhysicsSetBlock(block, cmat, 1, paint);
                    PhysicsSetBlock(one, cmat, 1, paint);
                }
            }
            else if (remperc == 50)
            {
                if (rempart == 100)
                {
                    PhysicsSetBlock(block, cmat, 1, paint);
                    PhysicsSetBlock(one, cmat, 2, paint);
                }
                else if (rempart == 84)
                {
                    PhysicsSetBlock(block, cmat, 2, paint);
                    PhysicsSetBlock(one, cmat, 2, paint);
                }
            }
            else if (remperc == 34)
            {
                if (rempart == 100)
                {
                    PhysicsSetBlock(block, cmat, 2, paint);
                    PhysicsSetBlock(one, cmat, 2, paint);
                }
                else if (rempart == 84)
                {
                    PhysicsSetBlock(block, cmat, 2, paint);
                    PhysicsSetBlock(one, cmat, 3, paint);
                }
                else if (rempart == 68)
                {
                    PhysicsSetBlock(block, cmat, 3, paint);
                    PhysicsSetBlock(one, cmat, 3, paint);
                }
            }
            else if (remperc == 13)
            {
                if (rempart == 100)
                {
                    PhysicsSetBlock(block, cmat, 2, paint);
                    PhysicsSetBlock(one, cmat, 3, paint);
                }
                else if (rempart == 84)
                {
                    PhysicsSetBlock(block, cmat, 3, paint);
                    PhysicsSetBlock(one, cmat, 3, paint);
                }
                else if (rempart == 68)
                {
                    PhysicsSetBlock(block, cmat, 3, paint);
                    PhysicsSetBlock(one, cmat, 4, paint);
                }
                else if (rempart == 50)
                {
                    PhysicsSetBlock(block, cmat, 4, paint);
                    PhysicsSetBlock(one, cmat, 4, paint);
                }
            }
        }

        void CombineWater(int rempart, Material cmat, int remperc, Location block, Location one, byte paint)
        {
            // TODO: Simplify!
            if (remperc == 84)
            {
                if (rempart == 100)
                {
                    PhysicsSetBlock(block, cmat, 1, paint);
                }
                else if (rempart == 84)
                {
                    PhysicsSetBlock(block, cmat, 2, paint);
                }
                else if (rempart == 68)
                {
                    PhysicsSetBlock(block, cmat, 3, paint);
                }
                else if (rempart == 50)
                {
                    PhysicsSetBlock(block, cmat, 4, paint);
                }
                else if (rempart == 34)
                {
                    PhysicsSetBlock(block, cmat, 5, paint);
                }
                else if (rempart == 13)
                {
                    PhysicsSetBlock(block, Material.AIR);
                }
                PhysicsSetBlock(one, cmat, 0);
            }
            else if (remperc == 68)
            {
                if (rempart == 100)
                {
                    PhysicsSetBlock(block, cmat, 2, paint);
                    PhysicsSetBlock(one, cmat, 0, paint);
                }
                else if (rempart == 84)
                {
                    PhysicsSetBlock(block, cmat, 3, paint);
                    PhysicsSetBlock(one, cmat, 0, paint);
                }
                else if (rempart == 68)
                {
                    PhysicsSetBlock(block, cmat, 4, paint);
                    PhysicsSetBlock(one, cmat, 0, paint);
                }
                else if (rempart == 50)
                {
                    PhysicsSetBlock(block, cmat, 5, paint);
                    PhysicsSetBlock(one, cmat, 0, paint);
                }
                else if (rempart == 34)
                {
                    PhysicsSetBlock(block, Material.AIR);
                    PhysicsSetBlock(one, cmat, 0, paint);
                }
                else if (rempart == 13)
                {
                    PhysicsSetBlock(block, Material.AIR);
                    PhysicsSetBlock(one, cmat, 1, paint);
                }
            }
            else if (remperc == 50)
            {
                if (rempart == 100)
                {
                    PhysicsSetBlock(block, cmat, 3, paint);
                    PhysicsSetBlock(one, cmat, 0, paint);
                }
                else if (rempart == 84)
                {
                    PhysicsSetBlock(block, cmat, 4, paint);
                    PhysicsSetBlock(one, cmat, 0, paint);
                }
                else if (rempart == 68)
                {
                    PhysicsSetBlock(block, cmat, 5, paint);
                    PhysicsSetBlock(one, cmat, 0, paint);
                }
                else if (rempart == 50)
                {
                    PhysicsSetBlock(block, Material.AIR);
                    PhysicsSetBlock(one, cmat, 0, paint);
                }
                else if (rempart == 34)
                {
                    PhysicsSetBlock(block, Material.AIR);
                    PhysicsSetBlock(one, cmat, 1, paint);
                }
                else if (rempart == 13)
                {
                    PhysicsSetBlock(block, Material.AIR);
                    PhysicsSetBlock(one, cmat, 2, paint);
                }
            }
            else if (remperc == 34)
            {
                if (rempart == 100)
                {
                    PhysicsSetBlock(block, cmat, 4, paint);
                    PhysicsSetBlock(one, cmat, 0, paint);
                }
                else if (rempart == 84)
                {
                    PhysicsSetBlock(block, cmat, 5, paint);
                    PhysicsSetBlock(one, cmat, 0, paint);
                }
                else if (rempart == 68)
                {
                    PhysicsSetBlock(block, Material.AIR);
                    PhysicsSetBlock(one, cmat, 0, paint);
                }
                else if (rempart == 50)
                {
                    PhysicsSetBlock(block, Material.AIR);
                    PhysicsSetBlock(one, cmat, 1, paint);
                }
                else if (rempart == 34)
                {
                    PhysicsSetBlock(block, Material.AIR);
                    PhysicsSetBlock(one, cmat, 2, paint);
                }
                else if (rempart == 13)
                {
                    PhysicsSetBlock(block, Material.AIR);
                    PhysicsSetBlock(one, cmat, 3, paint);
                }
            }
            else if (remperc == 13)
            {
                if (rempart == 100)
                {
                    PhysicsSetBlock(block, cmat, 5, paint);
                    PhysicsSetBlock(one, cmat, 0, paint);
                }
                else if (rempart == 84)
                {
                    PhysicsSetBlock(block, Material.AIR);
                    PhysicsSetBlock(one, cmat, 0, paint);
                }
                else if (rempart == 68)
                {
                    PhysicsSetBlock(block, Material.AIR);
                    PhysicsSetBlock(one, cmat, 1, paint);
                }
                else if (rempart == 50)
                {
                    PhysicsSetBlock(block, Material.AIR);
                    PhysicsSetBlock(one, cmat, 2, paint);
                }
                else if (rempart == 34)
                {
                    PhysicsSetBlock(block, Material.AIR);
                    PhysicsSetBlock(one, cmat, 3, paint);
                }
                else if (rempart == 13)
                {
                    PhysicsSetBlock(block, Material.AIR);
                    PhysicsSetBlock(one, cmat, 4, paint);
                }
            }
        }

        void LiquidSpread1(Location block, Material cmat, Location one, float remainingperc, byte paint)
        {
            if (remainingperc == 100)
            {
                PhysicsSetBlock(one, cmat, 5, paint);
                PhysicsSetBlock(block, cmat, 1, paint);
            }
            else if (remainingperc == 84)
            {
                PhysicsSetBlock(one, cmat, 5, paint);
                PhysicsSetBlock(block, cmat, 2, paint);
            }
            else if (remainingperc == 68)
            {
                PhysicsSetBlock(one, cmat, 5, paint);
                PhysicsSetBlock(block, cmat, 3, paint);
            }
            else if (remainingperc == 50)
            {
                PhysicsSetBlock(one, cmat, 5, paint);
                PhysicsSetBlock(block, cmat, 4, paint);
            }
            else if (remainingperc == 34)
            {
                PhysicsSetBlock(one, cmat, 5, paint);
                PhysicsSetBlock(block, cmat, 5, paint);
            }
            // 13 doesn't move!
        }

        void LiquidSpread2(Location block, Material cmat, Location one, Location two, float remainingperc, byte paint)
        {
            if (remainingperc == 100)
            {
                PhysicsSetBlock(one, cmat, 5, paint);
                PhysicsSetBlock(two, cmat, 5, paint);
                PhysicsSetBlock(block, cmat, 2, paint);
            }
            else if (remainingperc == 84)
            {
                PhysicsSetBlock(one, cmat, 5, paint);
                PhysicsSetBlock(two, cmat, 5, paint);
                PhysicsSetBlock(block, cmat, 3, paint);
            }
            else if (remainingperc == 68)
            {
                PhysicsSetBlock(one, cmat, 5, paint);
                PhysicsSetBlock(two, cmat, 5, paint);
                PhysicsSetBlock(block, cmat, 4, paint);
            }
            else if (remainingperc == 50)
            {
                PhysicsSetBlock(one, cmat, 5, paint);
                PhysicsSetBlock(two, cmat, 5, paint);
                PhysicsSetBlock(block, cmat, 5, paint);
            }
            else if (remainingperc == 34)
            {
                PhysicsSetBlock(one, cmat, 5);
                PhysicsSetBlock(block, cmat, 5);
            }
            // 13 doesn't move!
        }

        void LiquidSpread3(Location block, Material cmat, Location one, Location two, Location three, float remainingperc, byte paint)
        {
            if (remainingperc == 100)
            {
                PhysicsSetBlock(one, cmat, 5, paint);
                PhysicsSetBlock(two, cmat, 5, paint);
                PhysicsSetBlock(three, cmat, 5, paint);
                PhysicsSetBlock(block, cmat, 3, paint);
            }
            else if (remainingperc == 84)
            {
                PhysicsSetBlock(one, cmat, 5, paint);
                PhysicsSetBlock(two, cmat, 5, paint);
                PhysicsSetBlock(three, cmat, 5, paint);
                PhysicsSetBlock(block, cmat, 4, paint);
            }
            else if (remainingperc == 68)
            {
                PhysicsSetBlock(one, cmat, 5, paint);
                PhysicsSetBlock(two, cmat, 5, paint);
                PhysicsSetBlock(three, cmat, 5, paint);
                PhysicsSetBlock(block, cmat, 5, paint);
            }
            else if (remainingperc == 50)
            {
                PhysicsSetBlock(one, cmat, 5, paint);
                PhysicsSetBlock(two, cmat, 5, paint);
                PhysicsSetBlock(block, cmat, 5, paint);
            }
            else if (remainingperc == 34)
            {
                PhysicsSetBlock(one, cmat, 5, paint);
                PhysicsSetBlock(block, cmat, 5, paint);
            }
            // 13 doesn't move!
        }
    }
}
