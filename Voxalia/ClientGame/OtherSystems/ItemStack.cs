using System;
using System.Drawing;
using Voxalia.Shared;
using Voxalia.Shared.Files;
using Voxalia.ClientGame.EntitySystem;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.GraphicsSystems;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
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

        public BlockItemEntity RenderedBlock = null;

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
                if (name.Contains(":") && name.Before(":").ToLowerFast() == "render_block")
                {
                    string[] blockDataToRender = name.After(":").SplitFast(',');
                    Material mat = MaterialHelpers.FromNameOrNumber(blockDataToRender[0]);
                    byte data = (byte)(blockDataToRender.Length < 2 ? 0 : Utilities.StringToInt(blockDataToRender[1]));
                    byte paint = (byte)(blockDataToRender.Length < 3 ? 0 : Colors.ForName(blockDataToRender[2]));
                    BlockDamage damage = blockDataToRender.Length < 4 ? BlockDamage.NONE : (BlockDamage)Enum.Parse(typeof(BlockDamage), blockDataToRender[3], true);
                    RenderedBlock = new BlockItemEntity(TheClient.TheRegion, mat, data, paint, damage);
                    RenderedBlock.GenVBO();
                    Tex = null;
                }
                else
                {
                    Tex = TheClient.Textures.GetTexture(name);
                }
            }
        }

        public override string GetTextureName()
        {
            if (RenderedBlock != null)
            {
                return "render_block:" + RenderedBlock.Mat + "," + RenderedBlock.Dat + "," + RenderedBlock.Paint + "," + RenderedBlock.Damage;
            }
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

        public void Forget() // TODO: use this!
        {
            if (RenderedBlock != null)
            {
                RenderedBlock.vbo.Destroy();
            }
        }

        public void Render3D(Location pos, float rot, Location size)
        {
            if (RenderedBlock != null)
            {
                RenderedBlock.WorldTransform = BEPUutilities.Matrix.Identity;
                RenderedBlock.WorldTransform = BEPUutilities.Matrix.CreateScale(size.ToBVector())
                    * BEPUutilities.Matrix.CreateFromAxisAngle(BEPUutilities.Vector3.UnitZ, rot)
                    * BEPUutilities.Matrix.CreateFromAxisAngle(BEPUutilities.Vector3.UnitX, (float)(Math.PI * 0.25))
                    * BEPUutilities.Matrix.CreateTranslation(pos.ToBVector());
                RenderedBlock.Render();
            }
        }

        public void Render(Location pos, Location size)
        {
            if (RenderedBlock != null)
            {
                return;
            }
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
