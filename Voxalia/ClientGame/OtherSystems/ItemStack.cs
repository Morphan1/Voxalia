using System.Drawing;
using Voxalia.Shared;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.GraphicsSystems;
using OpenTK.Graphics;

namespace Voxalia.ClientGame.OtherSystems
{
    public class ItemStack: ItemStackBase
    {
        public Client TheClient;

        public ItemStack(Client tclient, byte[] data)
        {
            TheClient = tclient;
            Load(data);
        }

        public ItemStack(Client tclient, string name)
        {
            TheClient = tclient;
            Name = name;
        }

        public Texture Tex;

        public Color4 GetColor()
        {
            Color col = Color.FromArgb(DrawColor);
            return new Color4(col.R, col.G, col.B, col.A);
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
                    Tex = TheClient.Textures.GetTexture("blocks/icons/" + SecondaryName.ToLower());
                }
                else
                {
                    return;
                }
            }
            Tex.Bind();
            TheClient.Rendering.SetColor(GetColor());
            TheClient.Rendering.RenderRectangle((int)pos.X, (int)pos.Y, (int)(pos.X + size.X), (int)(pos.Y + size.Y));
        }
    }
}
