using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ShadowOperations.Shared
{
    /// <summary>
    /// Represents an item or stack of items on the server or client.
    /// </summary>
    public abstract class ItemStackBase
    {
        /// <summary>
        /// The internal name of this item.
        /// </summary>
        public string Name;

        /// <summary>
        /// The display name of this item.
        /// </summary>
        public string DisplayName;

        /// <summary>
        /// The description of this item.
        /// </summary>
        public string Description;

        /// <summary>
        /// A bit of data associated with this item stack, for free usage by the Item Info.
        /// </summary>
        public int Datum = 0;

        /// <summary>
        /// How many of this item there are.
        /// </summary>
        public int Count;

        /// <summary>
        /// What color to draw this item as.
        /// </summary>
        public int DrawColor = Color.White.ToArgb();

        public abstract string GetTextureName();

        public abstract void SetTextureName(string name);

        public abstract string GetModelName();

        public abstract void SetModelName(string name);

        public byte[] ToBytes()
        {
            DataStream ds = new DataStream(1000);
            DataWriter dw = new DataWriter(ds);
            dw.WriteInt(Count);
            dw.WriteInt(Datum);
            dw.WriteInt(DrawColor);
            dw.WriteFullString(Name);
            dw.WriteFullString(DisplayName);
            dw.WriteFullString(Description);
            dw.WriteFullString(GetTextureName());
            dw.WriteFullString(GetModelName());
            return ds.ToArray();
        }

        public virtual void SetName(string name)
        {
            Name = name;
        }

        public void Load(string name, int count, string tex, string display, string descrip, int color, string model)
        {
            SetName(name);
            Count = count;
            DisplayName = display;
            Description = descrip;
            SetTextureName(tex);
            SetModelName(model);
            Datum = 0;
            DrawColor = color;
        }

        public void Load(byte[] data)
        {
            DataStream ds = new DataStream(data);
            DataReader dr = new DataReader(ds);
            Count = dr.ReadInt();
            Datum = dr.ReadInt();
            DrawColor = dr.ReadInt();
            SetName(dr.ReadFullString());
            DisplayName = dr.ReadFullString();
            Description = dr.ReadFullString();
            SetTextureName(dr.ReadFullString());
            SetModelName(dr.ReadFullString());
        }
    }
}
