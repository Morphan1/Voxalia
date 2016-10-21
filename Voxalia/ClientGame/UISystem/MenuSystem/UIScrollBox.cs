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
    public class UIScrollBox : UIElement
    {
        public int Scroll = 0;

        public UIScrollBox(UIAnchor anchor, Func<float> width, Func<float> height, Func<int> xOff, Func<int> yOff)
            : base(anchor, width, height, xOff, yOff)
        {
        }

        bool watchMouse = false;

        protected override void MouseEnter()
        {
            watchMouse = true;
        }

        protected override void MouseLeave()
        {
            watchMouse = false;
        }

        protected override HashSet<UIElement> GetAllAt(int x, int y)
        {
            HashSet<UIElement> found = new HashSet<UIElement>();
            if (SelfContains(x, y))
            {
                found.Add(this);
                x -= GetX();
                y += Scroll - GetY();
                foreach (UIElement element in Children)
                {
                    if (element.Contains(x, y))
                    {
                        found.Add(element);
                    }
                }
            }
            return found;
        }

        protected override void Tick(double delta)
        {
            if (watchMouse)
            {
                Scroll -= MouseHandler.MouseScroll * 10;
                if (Scroll < 0)
                {
                    Scroll = 0;
                }
            }
        }

        protected override void Render(double delta, int xoff, int yoff)
        {
            int x = GetX() + xoff;
            int y = GetY() + yoff;
            int h = (int)GetHeight();
            int w = (int)GetWidth();
            Client TheClient = GetClient();
            TheClient.Rendering.SetColor(new Vector4(0f, 0.5f, 0.5f, 0.3f));
            TheClient.Rendering.RenderRectangle(x, y, x + w, y + h);
            TheClient.Rendering.SetColor(new Vector4(1f));
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(0, 0, TheClient.Window.Width, TheClient.Window.Height); // TODO: Bump around a stack, for embedded scroll groups?
            GL.Disable(EnableCap.ScissorTest);
        }

        protected override void RenderChildren(double delta, int xoff, int yoff)
        {
            int x = GetX() + xoff;
            int y = GetY() + yoff;
            int h = (int)GetHeight();
            int w = (int)GetWidth();
            Client TheClient = GetClient();
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(x, TheClient.Window.Height - (y + h), w, h);
            base.RenderChildren(delta, x, y - Scroll);
            GL.Scissor(0, 0, TheClient.Window.Width, TheClient.Window.Height); // TODO: Bump around a stack, for embedded scroll groups?
            GL.Disable(EnableCap.ScissorTest);
        }
    }
}
