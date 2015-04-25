using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.ServerMainSystem;

namespace ShadowOperations.ServerGame.ItemSystem
{
    /// <summary>
    /// Represents an item or stack of items on the server.
    /// </summary>
    public class ItemStack: ItemStackBase
    {
        public Server TheServer;

        public ItemStack(string name, Server tserver)
        {
            TheServer = tserver;
            Load(name);
        }

        public ItemStack(byte[] data, Server tserver)
        {
            TheServer = tserver;
            Load(data);
        }

        public BaseItemInfo Info = null;

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
    }
}
