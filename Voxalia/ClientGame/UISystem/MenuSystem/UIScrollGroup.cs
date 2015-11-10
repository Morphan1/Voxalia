using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    public class UIScrollGroup : UIMenuItem
    {
        public List<UIMenuItem> MenuItems = new List<UIMenuItem>();

        public Func<int> X;

        public Func<int> Y;

        public int MaxWidth;

        public Func<int> Height;

        public int Scroll = 0;

        public UIScrollGroup(Func<int> x, Func<int> y, int maxwidth, Func<int> height)
        {
            X = x;
            Y = y;
            MaxWidth = maxwidth;
            Height = height;
        }

        public void Add(UIMenuItem item)
        {
            MenuItems.Add(item);
            item.Menus = Menus;
            item.Init();
        }

        public void Remove(UIMenuItem item)
        {
            MenuItems.Remove(item);
        }

        public void Clear()
        {
            MenuItems.Clear();
        }

        public override bool Contains(int x, int y)
        {
            int tx = X();
            int ty = Y();
            return x >= tx && x <= tx + MaxWidth && y >= ty && y <= ty + Height();
        }

        public override void Init()
        {
        }

        bool watchMouse = false;

        public override void MouseEnter()
        {
            watchMouse = true;
        }

        public override void MouseLeave()
        {
            watchMouse = false;
        }

        public override void Tick(double delta)
        {
            if (watchMouse)
            {
                Scroll += MouseHandler.MouseScroll * 10;
                if (Scroll < 0)
                {
                    Scroll = 0;
                }
                // TODO: Maximum value?
                int mx = MouseHandler.MouseX();
                int my = MouseHandler.MouseY() + Scroll;
                for (int i = 0; i < MenuItems.Count; i++)
                {
                    if (MenuItems[i].Contains(mx, my))
                    {
                        if (!MenuItems[i].HoverInternal)
                        {
                            MenuItems[i].HoverInternal = true;
                            MenuItems[i].MouseEnter();
                        }
                        if (mLDown && !mLDP)
                        {
                            MenuItems[i].MouseLeftDown();
                        }
                        else if (!mLDown && mLDP)
                        {
                            MenuItems[i].MouseLeftUp();
                        }
                    }
                    else if (MenuItems[i].HoverInternal)
                    {
                        MenuItems[i].HoverInternal = false;
                        MenuItems[i].MouseLeave();
                    }
                }
                mLDP = mLDown;
            }
            for (int i = 0; i < MenuItems.Count; i++)
            {
                MenuItems[i].Tick(delta);
            }
        }

        public bool mLDP = false;

        public bool mLDown = false;

        public override void MouseLeftDown()
        {
            mLDown = true;
        }

        public override void MouseLeftUp()
        {
            mLDown = false;
        }

        public override void Render(double delta, int xoff, int yoff)
        {
            int x = X() + xoff;
            int y = Y() + yoff;
            int h = Height();
            Menus.TheClient.Rendering.SetColor(new Vector4(0f, 0.5f, 0.5f, 0.3f));
            Menus.TheClient.Rendering.RenderRectangle(x, y, x + MaxWidth, y + h);
            Menus.TheClient.Rendering.SetColor(new Vector4(1f));
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(x, Menus.TheClient.Window.Height - (y + h), MaxWidth, h);
            // TODO: Adust everything by scroll amount!
            for (int i = 0; i < MenuItems.Count; i++)
            {
                MenuItems[i].Render(delta, xoff, yoff - Scroll);
            }
            GL.Scissor(0, 0, Menus.TheClient.Window.Width, Menus.TheClient.Window.Height); // TODO: Bump around a stack, for embedded scroll groups?
            GL.Disable(EnableCap.ScissorTest);
        }
    }
}
