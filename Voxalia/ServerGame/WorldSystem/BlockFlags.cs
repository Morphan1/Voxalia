//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using Voxalia.Shared;

namespace Voxalia.ServerGame.WorldSystem
{
    [Flags]
    public enum BlockFlags: byte
    {
        /// <summary>
        /// The block has nothing special about it.
        /// </summary>
        NONE = 0,
        /// <summary>
        /// The block has been edited by a user.
        /// </summary>
        EDITED = 1,
        /// <summary>
        /// The block is powered.
        /// </summary>
        POWERED = 2,
        /// <summary>
        /// The block has some form of filling.
        /// </summary>
        FILLED = 4,
        /// <summary>
        /// The block has some form of filling.
        /// </summary>
        FILLED2 = 8,
        /// <summary>
        /// The block has some form of filling.
        /// </summary>
        FILLED3 = 16,
        /// <summary>
        /// The block has some form of filling.
        /// </summary>
        FILLED4 = 32,
        /// <summary>
        /// The block needs to be recalculated (physics, liquid movement, etc. could be relevant.)
        /// </summary>
        NEEDS_RECALC = 64,
        /// <summary>
        /// The block cannot be edited by users.
        /// </summary>
        PROTECTED = 128
    }

    public static class BlockInternalExtensions
    {
        public static bool IsFilled(this BlockInternal bi)
        {
            return ((BlockFlags)bi.BlockLocalData).HasFlag(BlockFlags.FILLED)
                || ((BlockFlags)bi.BlockLocalData).HasFlag(BlockFlags.FILLED2)
                || ((BlockFlags)bi.BlockLocalData).HasFlag(BlockFlags.FILLED3)
                || ((BlockFlags)bi.BlockLocalData).HasFlag(BlockFlags.FILLED4);
        }

        public static bool WasEdited(this BlockInternal bi)
        {
            return ((BlockFlags)bi.BlockLocalData).HasFlag(BlockFlags.EDITED);
        }

        public static bool IsPowered(this BlockInternal bi)
        {
            return ((BlockFlags)bi.BlockLocalData).HasFlag(BlockFlags.POWERED);
        }

        public static FillType GetFillType(this BlockInternal bi)
        {
            bool f1 = ((BlockFlags)bi.BlockLocalData).HasFlag(BlockFlags.FILLED);
            bool f2 = ((BlockFlags)bi.BlockLocalData).HasFlag(BlockFlags.FILLED2);
            bool f3 = ((BlockFlags)bi.BlockLocalData).HasFlag(BlockFlags.FILLED3);
            bool f4 = ((BlockFlags)bi.BlockLocalData).HasFlag(BlockFlags.FILLED4);
            if (f4)
            {
                if (f1)
                {
                    if (f2)
                    {
                        if (f3)
                        {
                            return FillType.MUD;
                        }
                        else
                        {
                            return FillType.STEAM;
                        }
                    }
                    else
                    {
                        if (f3)
                        {
                            return FillType.HEAVY_GAS;
                        }
                        else
                        {
                            return FillType.POISON_GAS;
                        }
                    }
                }
                else
                {
                    if (f2)
                    {
                        if (f3)
                        {
                            return FillType.FRESH_WATER;
                        }
                        else
                        {
                            return FillType.HELIUM_GAS;
                        }
                    }
                    else
                    {
                        if (f3)
                        {
                            return FillType.LIQUID_NITROGEN;
                        }
                        else
                        {
                            return FillType.BOILING_WATER;
                        }
                    }
                }
            }
            else
            {
                if (f1)
                {
                    if (f2)
                    {
                        if (f3)
                        {
                            return FillType.SALT_WATER;
                        }
                        else
                        {
                            return FillType.BAD_WATER;
                        }
                    }
                    else
                    {
                        if (f3)
                        {
                            return FillType.OIL;
                        }
                        else
                        {
                            return FillType.LAVA;
                        }
                    }
                }
                else
                {
                    if (f2)
                    {
                        if (f3)
                        {
                            return FillType.POISON;
                        }
                        else
                        {
                            return FillType.BLOOD;
                        }
                    }
                    else
                    {
                        if (f3)
                        {
                            return FillType.HONEY;
                        }
                        else
                        {
                            return FillType.NONE;
                        }
                    }
                }
            }
        }
    }

    public enum FillType : byte
    {
        NONE = 0,
        SALT_WATER = 1,
        BAD_WATER = 2,
        OIL = 3,
        LAVA = 4,
        POISON = 5,
        BLOOD = 6,
        HONEY = 7,
        MUD = 8,
        STEAM = 9,
        HEAVY_GAS = 10,
        POISON_GAS = 11,
        FRESH_WATER = 12,
        HELIUM_GAS = 13,
        LIQUID_NITROGEN = 14,
        BOILING_WATER = 15
    }
}
