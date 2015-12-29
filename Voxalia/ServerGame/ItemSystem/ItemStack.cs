using System;
using System.Text;
using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ServerGame.ServerMainSystem;
using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Common;
using Voxalia.ServerGame.TagSystem.TagObjects;

namespace Voxalia.ServerGame.ItemSystem
{
    /// <summary>
    /// Represents an item or stack of items on the server.
    /// </summary>
    public class ItemStack: ItemStackBase
    {
        public Server TheServer;

        public ItemStack(string name, string secondary_name, Server tserver, int count, string tex, string display, string descrip, System.Drawing.Color color, string model, bool bound, params string[] attrs)
        {
            TheServer = tserver;
            Load(name, secondary_name, count, tex, display, descrip, color, model);
            IsBound = bound;
            if (attrs != null)
            {
                for (int i = 0; i < attrs.Length; i += 2)
                {
                    Attributes.Add(attrs[i], attrs[i + 1]);
                }
            }
        }

        public ItemStack(string name, Server tserver, int count, string tex, string display, string descrip, System.Drawing.Color color, string model, bool bound, params string[] attrs)
            : this(name, null, tserver, count, tex, display, descrip, color, model, bound, attrs)
        {
        }

        public ItemStack(byte[] data, Server tserver)
        {
            TheServer = tserver;
            Load(data);
        }

        public float GetAttributeF(string attr, float def)
        {
            string outp;
            if (Attributes.TryGetValue(attr, out outp))
            {
                return Utilities.StringToFloat(outp);
            }
            return def;
        }

        public int GetAttributeI(string attr, int def)
        {
            string outp;
            if (Attributes.TryGetValue(attr, out outp))
            {
                return Utilities.StringToInt(outp);
            }
            return def;
        }

        public BaseItemInfo Info = null;

        public Dictionary<string, string> Attributes = new Dictionary<string, string>();

        public bool IsBound = false;

        public override void SetName(string name)
        {
            Info = TheServer.Items.GetInfoFor(name);
            base.SetName(name);
        }

        /// <summary>
        /// The image used to render this item.
        /// </summary>
        public string Image;

        public override string GetTextureName()
        {
            return Image;
        }

        public override void SetTextureName(string name)
        {
            Image = name;
        }

        public string Model;
        
        public override string GetModelName()
        {
            return Model;
        }

        public override void SetModelName(string name)
        {
            Model = name;
        }

        public ItemStack Duplicate()
        {
            return (ItemStack)MemberwiseClone();
        }

        public string EscapedSharedStr()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            foreach (KeyValuePair<string, float> val in SharedAttributes)
            {
                sb.Append(EscapeTagBase.Escape(val.Key) + "=" + val.Value + ";");
            }
            sb.Append("}");
            return sb.ToString();
        }

        public string EscapedLocalStr()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            foreach (KeyValuePair<string, string> val in Attributes)
            {
                sb.Append(EscapeTagBase.Escape(val.Key) + "=" + EscapeTagBase.Escape(val.Value) + ";");
            }
            sb.Append("}");
            return sb.ToString();
        }

        public string ToEscapedString()
        {
            return TagParser.Escape(Name) + "[secondary=" + (SecondaryName == null ? "" : EscapeTagBase.Escape(SecondaryName)) + ";display=" + EscapeTagBase.Escape(DisplayName) + ";count=" + Count
                + ";description=" + EscapeTagBase.Escape(Description) + ";texture=" + EscapeTagBase.Escape(GetTextureName()) + ";model=" + EscapeTagBase.Escape(GetModelName()) + ";bound=" + (IsBound ? "true": "false")
                + ";drawcolor=" + new ColorTag(DrawColor) + ";datum=" + Datum + ";shared=" + EscapedSharedStr() + ";local=" + EscapedLocalStr() + "]";
        }

        static List<KeyValuePair<string, string>> SplitUpPairs(string input)
        {
            List<KeyValuePair<string, string>> toret = new List<KeyValuePair<string, string>>();
            if (input.Length == 0)
            {
                return toret;
            }
            int start = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == ';')
                {
                    string valt = input.Substring(start, i - start);
                    string[] splitt = valt.Split(new char[] { '=' }, 2);
                    if (splitt.Length < 2)
                    {
                        continue;
                    }
                    toret.Add(new KeyValuePair<string, string>(splitt[0], splitt[1]));
                    start = i + 1;
                }
            }
            string valx = input.Substring(start);
            string[] splitx = valx.Split(new char[] { '=' }, 2);
            if (splitx.Length < 2)
            {
                return toret;
            }
            toret.Add(new KeyValuePair<string, string>(splitx[0], splitx[1]));
            return toret;
        }

        public static ItemStack FromString(Server tserver, string input)
        {
            int brack = input.IndexOf('[');
            string name = input.Substring(0, brack);
            string contents = input.Substring(brack + 1, input.Length - (brack + 1));
            List<KeyValuePair<string, string>> pairs = SplitUpPairs(contents);
            string secname = "";
            int count = 1;
            string tex = "";
            string display = "";
            string descrip = "";
            string model = "";
            bool bound = false;
            string shared = "";
            string local = "";
            int datum = 0;
            System.Drawing.Color color = System.Drawing.Color.White;
            foreach (KeyValuePair<string, string> pair in pairs)
            {
                string tkey = UnescapeTagBase.Unescape(pair.Key);
                string tval = UnescapeTagBase.Unescape(pair.Value);
                switch (tkey)
                {
                    case "secondary":
                        secname = tval;
                        break;
                    case "display":
                        display = tval;
                        break;
                    case "count":
                        count = Utilities.StringToInt(tval);
                        break;
                    case "description":
                        descrip = tval;
                        break;
                    case "texture":
                        tex = tval;
                        break;
                    case "model":
                        model = tval;
                        break;
                    case "bound":
                        bound = tval == "true";
                        break;
                    case "drawcolor":
                        color = (ColorTag.For(tval) ?? new ColorTag(color)).Internal;
                        break; // TODO
                    case "datum":
                        datum = Utilities.StringToInt(tval);
                        break;
                    case "shared":
                        shared = tval;
                        break;
                    case "local":
                        local = tval;
                        break;
                    default:
                        throw new Exception("Invalid item key: " + tkey);
                }
            }
            ItemStack item = new ItemStack(name, secname, tserver, count, tex, display, descrip, color, model, bound);
            item.Datum = datum;
            pairs = SplitUpPairs(shared.Substring(1, shared.Length - 2));
            foreach (KeyValuePair<string, string> pair in pairs)
            {
                item.SharedAttributes.Add(UnescapeTagBase.Unescape(pair.Key), Utilities.StringToFloat(pair.Value));
            }
            pairs = SplitUpPairs(local.Substring(1, local.Length - 2));
            foreach (KeyValuePair<string, string> pair in pairs)
            {
                item.Attributes.Add(UnescapeTagBase.Unescape(pair.Key), UnescapeTagBase.Unescape(pair.Value));
            }
            return item;
        }
    }
}
