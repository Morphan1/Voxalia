using OpenTK.Graphics.OpenGL4;
using Voxalia.Shared;

namespace Voxalia.ClientGame.GraphicsSystems
{
    public class TextureBlock
    {
        public TextureEngine TEngine;

        public int TextureID;

        public void Generate(TextureEngine eng)
        {
            TEngine = eng;
            TextureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, TextureID);
            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, 128, 128, MaterialHelpers.MAX_TEXTURES);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2DArray, TextureID);
            // Default Textures
            SetTexture((int)Material.AIR, "clear");
            SetTexture((int)Material.STONE, "blocks/solid/stone");
            SetTexture((int)Material.GRASS, "blocks/solid/grass_side");
            SetTexture((int)Material.DIRT, "blocks/solid/dirt");
            SetTexture((int)Material.WATER, "blocks/liquid/water");
            SetTexture((int)Material.DEBUG, "blocks/solid/db_top");
            SetTexture((int)Material.LEAVES1, "blocks/transparent/leaves_basic1");
            SetTexture((int)Material.CONCRETE, "blocks/solid/concrete");
            SetTexture((int)Material.SLIPGOO, "blocks/liquid/slipgoo");
            SetTexture((int)Material.SNOW, "blocks/solid/snow");
            SetTexture((int)Material.SMOKE, "blocks/liquid/smoke");
            SetTexture(MaterialHelpers.MAX_TEXTURES - 6, "blocks/solid/db_ym");
            SetTexture(MaterialHelpers.MAX_TEXTURES - 5, "blocks/solid/db_yp");
            SetTexture(MaterialHelpers.MAX_TEXTURES - 4, "blocks/solid/db_xp");
            SetTexture(MaterialHelpers.MAX_TEXTURES - 3, "blocks/solid/db_xm");
            SetTexture(MaterialHelpers.MAX_TEXTURES - 2, "blocks/solid/db_bottom");
            SetTexture(MaterialHelpers.MAX_TEXTURES - 1, "blocks/solid/grass");
            GL.BindTexture(TextureTarget.Texture2DArray, 0);
        }

        private void SetTexture(int ID, string texture)
        {
            TEngine.LoadTextureIntoArray(texture, ID);
        }
    }
}
