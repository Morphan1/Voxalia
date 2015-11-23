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
        /// The block needs to be recalculated (physics, liquid movement, etc. could be relevant.)
        /// </summary>
        NEEDS_RECALC = 32,
        SIXTYFOUR = 64,
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
                || ((BlockFlags)bi.BlockLocalData).HasFlag(BlockFlags.FILLED3);
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
            if (f1)
            {
                if (f2)
                {
                    if (f3)
                    {
                        return FillType.WATER;
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
                        return FillType.MUD;
                    }
                    else
                    {
                        return FillType.NONE;
                    }
                }
            }
        }
    }

    public enum FillType : byte
    {
        NONE = 0,
        WATER = 1,
        BAD_WATER = 2,
        OIL = 3,
        LAVA = 4,
        POISON = 5,
        BLOOD = 6,
        MUD = 7
    }
}
