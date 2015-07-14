using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ServerGame.ServerMainSystem;

namespace Voxalia.ServerGame.ItemSystem
{
    /// <summary>
    /// Represents an item or stack of items on the server.
    /// </summary>
    public class ItemStack: ItemStackBase
    {
        public Server TheServer;

        public ItemStack(string name, string secondary_name, Server tserver, int count, string tex, string display, string descrip, int color, string model, bool bound, params string[] attrs)
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

        public ItemStack(string name, Server tserver, int count, string tex, string display, string descrip, int color, string model, bool bound, params string[] attrs)
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
    }
}
