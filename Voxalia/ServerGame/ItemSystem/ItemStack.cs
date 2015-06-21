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

        public ItemStack(string name, string secondary_name, Server tserver, int count, string tex, string display, string descrip, int color, string model, bool bound)
        {
            TheServer = tserver;
            Load(name, secondary_name, count, tex, display, descrip, color, model);
            IsBound = bound;
        }

        public ItemStack(string name, Server tserver, int count, string tex, string display, string descrip, int color, string model, bool bound)
            : this(name, null, tserver, count, tex, display, descrip, color, model, bound)
        {
        }

        public ItemStack(byte[] data, Server tserver)
        {
            TheServer = tserver;
            Load(data);
        }

        public BaseItemInfo Info = null;

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
