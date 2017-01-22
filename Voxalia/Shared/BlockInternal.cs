//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Runtime.InteropServices;

namespace Voxalia.Shared
{
    /// <summary>
    /// Internal representation of a single block's data.
    /// Structure:
    /// 14 bits: Material
    /// 2 bits: block damage
    /// 8 bits: shape ('data')
    /// 6 bits: paint
    /// 1 bit: blockShareTexture
    /// 1 bit: --UNUSED--
    /// 8 bits: local data
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct BlockInternal
    {
        /// <summary>
        /// A sample block, a plain unmodified air block.
        /// </summary>
        public static BlockInternal AIR = new BlockInternal(0, 0, 0, 0);

        /// <summary>
        /// Converts an "item datum" method of storing a block internal data to an actual block internal data.
        /// </summary>
        /// <param name="dat">The item datum.</param>
        /// <returns>The actual block internal data.</returns>
        public static BlockInternal FromItemDatum(int dat)
        {
            return FromItemDatumU(BitConverter.ToUInt32(BitConverter.GetBytes(dat), 0)); // TODO: Less stupid conversion
        }

        /// <summary>
        /// Converts an unsigned "item datum" method of storing a block internal data to an actual block internal data.
        /// </summary>
        /// <param name="dat">The unsigned item datum.</param>
        /// <returns>The actual block internal data.</returns>
        public static BlockInternal FromItemDatumU(uint dat)
        {
            return new BlockInternal((ushort)(dat & (255u | (255u * 256u))), (byte)((dat & (255u * 256u * 256u)) / (256u * 256u)), (byte)((dat & (255u * 256u * 256u * 256u)) / (256u * 256u * 256)), 0);
        }

        /// <summary>
        /// The internal material and damage data of this block.
        /// </summary>
        public ushort _BlockMaterialInternal;

        /// <summary>
        /// The material represented by this block.
        /// This is a custom getter, that returns a small portion of the potential space.
        /// </summary>
        public ushort BlockMaterial
        {
            get
            {
                return (ushort)(_BlockMaterialInternal & (16384 - 1));
            }
            set
            {
                _BlockMaterialInternal = (ushort)(value | (DamageData * 16384));
            }
        }

        /// <summary>
        /// The material represented by this block.
        /// This is a custom getter, that returns a small portion of the potential space.
        /// </summary>
        public Material Material
        {
            get
            {
                return (Material)BlockMaterial;
            }
            set
            {
                BlockMaterial = (ushort)value;
            }
        }

        /// <summary>
        /// The damage data (0/1/2/3) of this block.
        /// </summary>
        public byte DamageData
        {
            get
            {
                return (byte)((_BlockMaterialInternal & (16384 | (16384 * 2))) / (16384));
            }
            set
            {
                _BlockMaterialInternal = (ushort)(BlockMaterial | (value * 16384));
            }
        }

        /// <summary>
        /// The damage data (NONE/SOME/MUCH/FULL) of this block.
        /// </summary>
        public BlockDamage Damage
        {
            get
            {
                return (BlockDamage)DamageData;
            }
            set
            {
                DamageData = (byte)value;
            }
        }

        /// <summary>
        /// The data represented by this block.
        /// Currently a directly read field, may be replaced by a getter that expands the bit count by stealing from other fields.
        /// </summary>
        public byte BlockData;

        public byte _BlockPaintInternal;

        /// <summary>
        /// The paint details represented by this block.
        /// This is a custom getter, that returns a small portion of the potential space.
        /// </summary>
        public byte BlockPaint
        {
            get
            {
                return (byte)(_BlockPaintInternal & 63);
            }
            set
            {
                _BlockPaintInternal = (byte)(value | (BlockShareTex ? 64 : 0));
            }
        }

        /// <summary>
        /// Whether this block should grab surrounding texture data to color itself.
        /// This is a custom getter, that returns a small portion of the potential space.
        /// </summary>
        public bool BlockShareTex
        {
            get
            {
                return (_BlockPaintInternal & 64) == 64;
            }
            set
            {
                _BlockPaintInternal = (byte)(BlockPaint | (value ? 64 : 0));
            }
        }
        
        /// <summary>
        /// The local details represented by this block.
        /// Only a direct field. Exact bit count may change.
        /// Generally for use with things such as light levels (Client), or block informational flags (Server).
        /// </summary>
        public byte BlockLocalData;

        /// <summary>
        /// Quickly constructs a basic BlockInternal from exact internal data input.
        /// </summary>
        /// <param name="mat">The material + damage data.</param>
        /// <param name="dat">The block data.</param>
        /// <param name="paint">The block paint.</param>
        /// <param name="loc">The block local data.</param>
        public BlockInternal(ushort mat, byte dat, byte paint, byte loc)
        {
            _BlockMaterialInternal = mat;
            BlockData = dat;
            _BlockPaintInternal = paint;
            BlockLocalData = loc;
        }

        /// <summary>
        /// Returns whether this block is opaque, checks both material and paint.
        /// </summary>
        /// <returns>Whether the block is opaque.</returns>
        public bool IsOpaque()
        {
            return ((Material)BlockMaterial).IsOpaque() && (BlockPaint < Colors.TRANS1 || BlockPaint > Colors.TRANS2);
        }

        /// <summary>
        /// Converts this block internal datum to an "item datum" integer.
        /// </summary>
        public int GetItemDatum()
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(GetItemDatumU()), 0); // TODO: Less stupid conversion
        }

        /// <summary>
        /// Converts this block internal datum to an "item datum" unsigned integer.
        /// </summary>
        public uint GetItemDatumU()
        {
            return (uint)_BlockMaterialInternal | ((uint)BlockData * 256u * 256u) | ((uint)_BlockPaintInternal * 256u * 256u * 256u);
        }

        /// <summary>
        /// Displays this block's data as a quick string. Rarely if ever useful.
        /// </summary>
        public override string ToString()
        {
            return ((Material)_BlockMaterialInternal) + ":" + BlockData + ":" + _BlockPaintInternal + ":" + BlockLocalData;
        }
    }
}
