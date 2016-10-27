using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    // TODO: I have no idea if this is the right name for this class.
    public class UIColoredBox : UIElement
    {
        public Vector4 Color;

        public UIColoredBox(Vector4 color, UIAnchor anchor, Func<float> width, Func<float> height, Func<int> xOff, Func<int> yOff)
            : base(anchor, width, height, xOff, yOff)
        {
            Color = color;
        }

        protected override void Render(double delta, int xoff, int yoff)
        {
            int x = GetX() + xoff;
            int y = GetY() + yoff;
            int w = (int)GetWidth();
            int h = (int)GetHeight();
            Client TheClient = GetClient();
            TheClient.Shaders.ColorMultShader.Bind();
            TheClient.Rendering.SetColor(Color);
            TheClient.Textures.White.Bind();
            TheClient.Rendering.RenderRectangle(x, y, x + w, y + h);
            TheClient.Rendering.SetColor(Vector4.One);
            GL.BindTexture(TextureTarget.Texture2D, TheClient.MainItemView.CurrentFBOTexture);
            TheClient.Rendering.RenderRectangle(x, y, x + w, y + h);
            TheClient.Textures.White.Bind();
        }
    }
}
