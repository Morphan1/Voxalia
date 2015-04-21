using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.OtherSystems
{
    /// <summary>
    /// Represents an item or stack of items on the server.
    /// </summary>
    class ItemStack: ItemStackBase
    {
        public ItemStack(string name)
            : base(name)
        {
        }

        public ItemStack(byte[] data)
            : base(data)
        {
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
