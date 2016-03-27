using System.Drawing;
using Voxalia.Shared;
using Voxalia.Shared.Files;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.GraphicsSystems;
using OpenTK.Graphics;
using FreneticScript;

namespace Voxalia.ClientGame.OtherSystems
{
    public class ItemStack: ItemStackBase
    {
        public Client TheClient;

        public ItemStack(Client tclient, byte[] data)
        {
            TheClient = tclient;
            DataStream ds = new DataStream(data);
            DataReader dr = new DataReader(ds);
            Load(dr);
        }

        public ItemStack(Client tclient, string name)
        {
            TheClient = tclient;
            Name = name;
        }

        public Texture Tex;

        public System.Drawing.Color GetColor()
        {
            return DrawColor;
        }

        public override void SetTextureName(string name)
        {
            if (name == null || name.Length == 0)
            {
                Tex = null;
            }
            else
            {
                Tex = TheClient.Textures.GetTexture(name);
            }
        }

        public override string GetTextureName()
        {
            return Tex == null ? null: Tex.Name;
        }

        public Model Mod;

        public override string GetModelName()
        {
            return Mod == null ? null: Mod.Name;
        }

        public override void SetModelName(string name)
        {
            Mod = TheClient.Models.GetModel(name);
        }

        public void Render(Location pos, Location size)
        {
            if (Tex == null)
            {
                if (Name == "block")
                {
                    Tex = TheClient.Textures.GetTexture("blocks/icons/" + SecondaryName.ToLowerFast());
                }
                else
                {
                    return;
                }
            }
            Tex.Bind();
            TheClient.Rendering.SetColor(TheClient.Rendering.AdaptColor(ClientUtilities.Convert(TheClient.Player.GetPosition()), GetColor()));
            TheClient.Rendering.RenderRectangle((int)pos.X, (int)pos.Y, (int)(pos.X + size.X), (int)(pos.Y + size.Y));
        }
    }
}
