using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.GraphicsSystems;

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    public class UIImage : UIElement
    {
        public Texture Image;

        public UIImage(Texture image, UIAnchor anchor, Func<float> width, Func<float> height, Func<int> xOff, Func<int> yOff)
            : base(anchor, width, height, xOff, yOff)
        {
            Image = image;
        }

        protected override void Render(double delta, int xoff, int yoff)
        {
            Client TheClient = GetClient();
            Image.Bind();
            int x = GetX() + xoff;
            int y = GetY() + yoff;
            TheClient.Rendering.RenderRectangle(x, y, x + GetWidth(), y + GetHeight());
        }
    }
}
