//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ClientGame.ClientMainSystem;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
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
