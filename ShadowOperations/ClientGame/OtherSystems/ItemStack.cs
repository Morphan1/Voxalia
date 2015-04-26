using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.ClientMainSystem;
using ShadowOperations.ClientGame.GraphicsSystems;

namespace ShadowOperations.ClientGame.OtherSystems
{
    public class ItemStack: ItemStackBase
    {
        public Client TheClient;

        public ItemStack(Client tclient, string name)
        {
            TheClient = tclient;
            Load(name);
        }

        public ItemStack(Client tclient, byte[] data)
        {
            TheClient = tclient;
            Load(data);
        }

        public Texture Tex;

        public override void SetTextureName(string name)
        {
            Tex = TheClient.Textures.GetTexture(name);
        }

        public override string GetTextureName()
        {
            return Tex == null ? null: Tex.Name;
        }
    }
}
