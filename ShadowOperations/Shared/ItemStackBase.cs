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
            byte[] b_name = FileHandler.encoding.GetBytes(Name);
            byte[] b_dname = FileHandler.encoding.GetBytes(DisplayName);
            byte[] b_desc = FileHandler.encoding.GetBytes(Description);
            byte[] b_tex = FileHandler.encoding.GetBytes(GetTextureName());
            byte[] b_model = FileHandler.encoding.GetBytes(GetModelName());
            DataStream ds = new DataStream(4 + 4 + 4 + 4 + 4 + b_name.Length + b_dname.Length + b_desc.Length + b_tex.Length + 4 + 4 + 4 + b_model.Length);
            DataWriter dw = new DataWriter(ds);
            dw.WriteInt(Count);
            dw.WriteInt(b_name.Length);
            dw.WriteInt(b_dname.Length);
            dw.WriteInt(b_desc.Length);
            dw.WriteInt(b_tex.Length);
            dw.WriteBytes(b_name);
            dw.WriteBytes(b_dname);
            dw.WriteBytes(b_desc);
            dw.WriteBytes(b_tex);
            dw.WriteInt(Datum);
            dw.WriteInt(DrawColor);
            dw.WriteInt(b_model.Length);
            dw.WriteBytes(b_model);
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
            int flen = 4 + 4 + 4 + 4 + 4;
            if (data.Length < flen)
            {
                throw new Exception("Invalid item stack bytes!");
            }
            DataStream ds = new DataStream(data);
            DataReader dr = new DataReader(ds);
            Count = dr.ReadInt();
            int c_name = dr.ReadInt();
            int c_dname = dr.ReadInt();
            int c_desc = dr.ReadInt();
            int c_tex = dr.ReadInt();
            flen += c_name + c_dname + c_desc + c_tex + 4 + 4 + 4;
            if (data.Length < flen)
            {
                throw new Exception("Invalid item stack bytes!");
            }
            SetName(dr.ReadString(c_name));
            DisplayName = dr.ReadString(c_dname);
            Description = dr.ReadString(c_desc);
            SetTextureName(dr.ReadString(c_tex));
            Datum = dr.ReadInt();
            DrawColor = dr.ReadInt();
            int c_model = dr.ReadInt();
            flen += c_model;
            if (data.Length < flen)
            {
                throw new Exception("Invalid item stack bytes!");
            }
            SetModelName(dr.ReadString(c_model));
        }
    }
}
