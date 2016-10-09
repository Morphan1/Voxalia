//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

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
        public void SurroundRunPhysics(Location start)
        {
            start = start.GetBlockLocation();
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

        public int physThisTick = 0;

        public const double defPhysBoost = 0.5;

        public double physBoost = defPhysBoost;

        public void PhysicsSetBlock(Location block, Material mat, byte dat = 0, byte paint = 0, BlockDamage damage = BlockDamage.NONE)
        {
            SetBlockMaterial(block, mat, dat, paint, (byte)(BlockFlags.EDITED | BlockFlags.NEEDS_RECALC), damage);
            physThisTick++;
            if (physThisTick > 5)
            {
                physThisTick = 0;
                physBoost += 0.05;
            }
            TheServer.Schedule.ScheduleSyncTask(() => { SurroundRunPhysics(block); }, physBoost);
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
            LiquidPhysics(block, c);
        }

        public void LiquidPhysics(Location block, BlockInternal c)
        {
            Material cmat = c.Material;
            if (!cmat.ShouldSpread())
            {
                return;
            }
            Material spreadAs = cmat.GetBigSpreadsAs();
            if (spreadAs == Material.AIR)
            {
                spreadAs = cmat;
            }
            byte cpaint = c.BlockPaint;
            if (c.BlockData > 5 || c.Damage != BlockDamage.NONE)
            {
                PhysicsSetBlock(block, cmat, 0, cpaint, BlockDamage.NONE);
                return;
            }
            Location block_below = block + new Location(0, 0, -1);
            BlockInternal below = GetBlockInternal(block_below);
            Material below_mat = below.Material;
            if (below_mat == Material.AIR)
            {
                PhysicsSetBlock(block_below, spreadAs, 0, cpaint, BlockDamage.NONE);
                return;
            }
            byte below_paint = below.BlockPaint;
            if ((below_mat == spreadAs || below_mat == cmat) && below_paint == cpaint)
            {
                if (below.BlockData != 0)
                {
                    PhysicsSetBlock(block_below, below_mat, 0, cpaint, BlockDamage.NONE);
                }
                return;
            }
            // TODO: What happens when one liquid is on top of another of a different type?!
            // For liquid on top of gas, we can swap their places to make the gas rise...
            // But for the rest?
            if (c.BlockData == 5)
            {
                return;
            }
            TryLiquidSpreadSide(block, c, cmat, cpaint, spreadAs, block + new Location(1, 0, 0));
            TryLiquidSpreadSide(block, c, cmat, cpaint, spreadAs, block + new Location(-1, 0, 0));
            TryLiquidSpreadSide(block, c, cmat, cpaint, spreadAs, block + new Location(0, 1, 0));
            TryLiquidSpreadSide(block, c, cmat, cpaint, spreadAs, block + new Location(0, -1, 0));
        }

        public void TryLiquidSpreadSide(Location block, BlockInternal c, Material cmat, byte cpaint, Material spreadAs, Location two)
        {
            BlockInternal tc = GetBlockInternal(two);
            Material tmat = tc.Material;
            if (tmat == Material.AIR)
            {
                PhysicsSetBlock(two, spreadAs, (byte)(c.BlockData + 1), cpaint, BlockDamage.NONE);
                return;
            }
            byte tpaint = tc.BlockPaint;
            if ((tmat == cmat || tmat == spreadAs) && tpaint == cpaint)
            {
                if (tc.BlockData > c.BlockData + 1)
                {
                    PhysicsSetBlock(two, tmat, (byte)(c.BlockData + 1), cpaint, BlockDamage.NONE);
                }
                return;
            }
        }
    }
}
