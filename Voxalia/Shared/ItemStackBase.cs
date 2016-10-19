//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using Voxalia.Shared.Files;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace Voxalia.Shared
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
        /// The internal secondary name of this item, for use with items that are subtypes.
        /// </summary>
        public string SecondaryName;

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
        /// Any attributes shared between all users of the item.
        /// </summary>
        public Dictionary<string, TemplateObject> SharedAttributes = new Dictionary<string, TemplateObject>();

        /// <summary>
        /// All item stacks that make up this item.
        /// </summary>
        public List<ItemStackBase> Components = new List<ItemStackBase>();

        /// <summary>
        /// Whether this item should render when it is a component.
        /// </summary>
        public bool RenderAsComponent = true;

        /// <summary>
        /// Where, relative to an item, this component should render.
        /// </summary>
        public Location ComponentRenderOffset = Location.Zero;

        public void AddComponent(ItemStackBase item)
        {
            if (this == item || HasComponentDeep(item))
            {
                // TODO: Error?
                return;
            }
            Components.Add(item);
        }

        public bool HasComponentDeep(ItemStackBase item)
        {
            foreach (ItemStackBase itb in Components)
            {
                if (itb == item)
                {
                    return true;
                }
                if (item.HasComponentDeep(item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// How many of this item there are.
        /// </summary>
        public int Count;

        /// <summary>
        /// What color to draw this item as.
        /// </summary>
        public Color DrawColor = Color.White;

        /// <summary>
        /// How much volume this item takes up.
        /// </summary>
        public double Volume = 1;

        /// <summary>
        /// How much weight this item takes up.
        /// </summary>
        public double Weight = 1;

        /// <summary>
        /// What temperature (F) this item is at.
        /// </summary>
        public double Temperature = 70;

        /// <summary>
        /// The temperature (C) this item is at.
        /// </summary>
        public double TemperatureC
        {
            get
            {
                return (Temperature - 32f) * (5f / 9f);
            }
            set
            {
                Temperature = (value * (9f / 5f)) + 32f;
            }
        }

        public abstract string GetTextureName();

        public abstract void SetTextureName(string name);

        public abstract string GetModelName();

        public abstract void SetModelName(string name);

        public void WriteBasicBytes(DataWriter dw)
        {
            dw.WriteInt(Count);
            dw.WriteInt(Datum);
            dw.WriteFloat((float)Weight);
            dw.WriteFloat((float)Volume);
            dw.WriteFloat((float)Temperature);
            dw.WriteInt(DrawColor.ToArgb());
            dw.WriteFullString(Name);
            dw.WriteFullString(SecondaryName == null ? "" : SecondaryName);
            dw.WriteFullString(DisplayName);
            dw.WriteFullString(Description);
            dw.WriteFullString(GetTextureName());
            dw.WriteFullString(GetModelName());
            dw.WriteByte((byte)(RenderAsComponent ? 1 : 0));
            dw.WriteFloat((float)ComponentRenderOffset.X);
            dw.WriteFloat((float)ComponentRenderOffset.Y);
            dw.WriteFloat((float)ComponentRenderOffset.Z);
            dw.WriteInt(SharedAttributes.Count);
            foreach (KeyValuePair<string, TemplateObject> entry in SharedAttributes)
            {
                if (entry.Key == null || entry.Value == null)
                {
                    SysConsole.Output(OutputType.WARNING, "Null entry in SharedAttributes for " + Name);
                    continue;
                }
                dw.WriteFullString(entry.Key);
                if (entry.Value is IntegerTag)
                {
                    dw.WriteByte(0);
                    dw.WriteLong(((IntegerTag)entry.Value).Internal);
                }
                else if (entry.Value is NumberTag)
                {
                    dw.WriteByte(1);
                    dw.WriteDouble(((NumberTag)entry.Value).Internal);
                }
                else if (entry.Value is BooleanTag)
                {
                    dw.WriteByte(2);
                    dw.WriteByte((byte)(((BooleanTag)entry.Value).Internal ? 1 : 0));
                }
                // TODO: shared BaseItemTag?
                else
                {
                    dw.WriteByte(3);
                    dw.WriteFullString(entry.Value.ToString());
                }
            }
        }

        public byte[] ToBytes()
        {
            DataStream ds = new DataStream(1000);
            DataWriter dw = new DataWriter(ds);
            WriteBasicBytes(dw);
            dw.WriteInt(Components.Count);
            foreach (ItemStackBase itb in Components)
            {
                dw.WriteFullBytes(itb.ToBytes());
            }
            dw.Flush();
            return ds.ToArray();
        }

        public virtual void SetName(string name)
        {
            Name = name;
        }

        public void Load(string name, string secondary_name, int count, string tex, string display, string descrip, System.Drawing.Color color, string model, int datum)
        {
            SetName(name);
            SecondaryName = secondary_name;
            Count = count;
            DisplayName = display;
            Description = descrip;
            SetModelName(model);
            Datum = datum;
            SetTextureName(tex);
            DrawColor = color;
        }

        public void Load(DataReader dr, Func<byte[], ItemStackBase> getItem)
        {
            Count = dr.ReadInt();
            Datum = dr.ReadInt();
            Weight = dr.ReadFloat();
            Volume = dr.ReadFloat();
            Temperature = dr.ReadFloat();
            DrawColor = System.Drawing.Color.FromArgb(dr.ReadInt());
            SetName(dr.ReadFullString());
            string secondary_name = dr.ReadFullString();
            SecondaryName = secondary_name.Length == 0 ? null : secondary_name;
            DisplayName = dr.ReadFullString();
            Description = dr.ReadFullString();
            string tex = dr.ReadFullString();
            SetModelName(dr.ReadFullString());
            SetTextureName(tex);
            RenderAsComponent = dr.ReadByte() == 1;
            ComponentRenderOffset.X = dr.ReadFloat();
            ComponentRenderOffset.Y = dr.ReadFloat();
            ComponentRenderOffset.Z = dr.ReadFloat();
            int attribs = dr.ReadInt();
            for (int i = 0; i < attribs; i++)
            {
                string cattrib = dr.ReadFullString();
                byte b = dr.ReadByte();
                if (b == 0)
                {
                    SharedAttributes.Add(cattrib, new IntegerTag(dr.ReadInt64()));
                }
                else if (b == 1)
                {
                    SharedAttributes.Add(cattrib, new NumberTag(dr.ReadDouble()));
                }
                else if (b == 2)
                {
                    SharedAttributes.Add(cattrib, new BooleanTag(dr.ReadByte() == 1));
                }
                else
                {
                    SharedAttributes.Add(cattrib, new TextTag(dr.ReadFullString()));
                }
            }
            int comps = dr.ReadInt();
            for (int i = 0; i < comps; i++)
            {
                Components.Add(getItem(dr.ReadFullBytes()));
            }
        }
        
        public string SharedStr()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            foreach (KeyValuePair<string, TemplateObject> val in SharedAttributes)
            {
                string type = "text";
                if (val.Value is IntegerTag)
                {
                    type = "inte";
                }
                else if (val.Value is NumberTag)
                {
                    type = "numb";
                }
                else if (val.Value is BooleanTag)
                {
                    type = "bool";
                }
                sb.Append(TagParser.Escape(val.Key) + "=" + type + "/" + TagParser.Escape(val.Value.ToString()) + ";");
            }
            sb.Append("}");
            return sb.ToString();
        }

        public string ComponentString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            foreach (ItemStackBase itb in Components)
            {
                sb.Append(TagParser.Escape(itb.ToString()) + ";");
            }
            sb.Append("}");
            return sb.ToString();
        }

        public override string ToString()
        {
            return Name + "[secondary=" + (SecondaryName == null ? "{NULL}" : SecondaryName) + ";display=" + DisplayName + ";count=" + Count + ";renderascomponent=" + RenderAsComponent + ";componentrenderoffset=" + ComponentRenderOffset.ToSimpleString()
                + ";description=" + Description + ";texture=" + GetTextureName() + ";model=" + GetModelName() + ";weight=" + Weight + ";volume=" + Volume + ";temperature=" + Temperature
                + ";drawcolor=" + DrawColor.R / 255f + "," + DrawColor.G / 255f + "," + DrawColor.B / 255f + "," + DrawColor.A / 255f + ";datum=" + Datum + ";shared=" + SharedStr() + ";components=" + ComponentString() + "]";
            // TODO: Shared color tag?
        }
    }
}
