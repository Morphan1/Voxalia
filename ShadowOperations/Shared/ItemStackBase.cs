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

        public byte[] ToBytes()
        {
            byte[] b_name = FileHandler.encoding.GetBytes(Name);
            byte[] b_dname = FileHandler.encoding.GetBytes(DisplayName);
            byte[] b_desc = FileHandler.encoding.GetBytes(Description);
            byte[] b_tex = FileHandler.encoding.GetBytes(GetTextureName());
            byte[] data = new byte[4 + 4 + 4 + 4 + 4 + b_name.Length + b_dname.Length + b_desc.Length + b_tex.Length + 4 + 4];
            Utilities.IntToBytes(Count).CopyTo(data, 0);
            Utilities.IntToBytes(b_name.Length).CopyTo(data, 4);
            Utilities.IntToBytes(b_dname.Length).CopyTo(data, 4 + 4);
            Utilities.IntToBytes(b_desc.Length).CopyTo(data, 4 + 4 + 4);
            Utilities.IntToBytes(b_tex.Length).CopyTo(data, 4 + 4 + 4 + 4);
            b_name.CopyTo(data, 4 + 4 + 4 + 4 + 4);
            b_dname.CopyTo(data, 4 + 4 + 4 + 4 + 4 + b_name.Length);
            b_desc.CopyTo(data, 4 + 4 + 4 + 4 + 4 + b_name.Length + b_dname.Length);
            b_tex.CopyTo(data, 4 + 4 + 4 + 4 + 4 + b_name.Length + b_dname.Length + b_desc.Length);
            Utilities.IntToBytes(Datum).CopyTo(data, 4 + 4 + 4 + 4 + 4 + b_name.Length + b_dname.Length + b_desc.Length + b_tex.Length);
            Utilities.IntToBytes(DrawColor).CopyTo(data, 4 + 4 + 4 + 4 + 4 + b_name.Length + b_dname.Length + b_desc.Length + b_tex.Length + 4);
            return data;
        }

        public virtual void SetName(string name)
        {
            Name = name;
        }

        public void Load(string name, int count, string tex, string display, string descrip, int color)
        {
            SetName(name);
            DisplayName = display;
            Description = descrip;
            SetTextureName(tex);
            Datum = 0;
            DrawColor = color;
        }

        public void Load(byte[] data)
        {
            if (data.Length < 4 + 4 + 4 + 4 + 4)
            {
                throw new Exception("Invalid item stack bytes!");
            }
            Count = Utilities.BytesToInt(Utilities.BytesPartial(data, 0, 4));
            int c_name = Utilities.BytesToInt(Utilities.BytesPartial(data, 4, 4));
            int c_dname = Utilities.BytesToInt(Utilities.BytesPartial(data, 4 + 4, 4));
            int c_desc = Utilities.BytesToInt(Utilities.BytesPartial(data, 4 + 4 + 4, 4));
            int c_tex = Utilities.BytesToInt(Utilities.BytesPartial(data, 4 + 4 + 4 + 4, 4));
            if (data.Length < 4 + 4 + 4 + 4 + 4 + c_name + c_dname + c_desc + c_tex + 4)
            {
                throw new Exception("Invalid item stack bytes!");
            }
            SetName(FileHandler.encoding.GetString(data, 4 + 4 + 4 + 4 + 4, c_name));
            DisplayName = FileHandler.encoding.GetString(data, 4 + 4 + 4 + 4 + 4 + c_name, c_dname);
            Description = FileHandler.encoding.GetString(data, 4 + 4 + 4 + 4 + 4 + c_name + c_dname, c_desc);
            SetTextureName(FileHandler.encoding.GetString(data, 4 + 4 + 4 + 4 + 4 + c_name + c_dname + c_desc, c_tex));
            Datum = Utilities.BytesToInt(Utilities.BytesPartial(data, 4 + 4 + 4 + 4 + 4 + c_name + c_dname + c_desc + c_tex, 4));
            DrawColor = Utilities.BytesToInt(Utilities.BytesPartial(data, 4 + 4 + 4 + 4 + 4 + c_name + c_dname + c_desc + c_tex + 4, 4));
        }
    }
}
