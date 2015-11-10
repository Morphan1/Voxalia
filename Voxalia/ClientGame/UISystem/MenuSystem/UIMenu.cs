using System.Collections.Generic;
using Voxalia.ClientGame.ClientMainSystem;
using OpenTK.Input;

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    public class UIMenu
    {
        public Client TheClient;

        public List<UIMenuItem> MenuItems = new List<UIMenuItem>();

        public UIMenu(Client tclient)
        {
            TheClient = tclient;
        }

        public void Add(UIMenuItem item)
        {
            item.Menus = this;
            item.Init();
            MenuItems.Add(item);
        }

        bool pDown;

        public void TickAll()
        {
            int mX = MouseHandler.MouseX();
            int mY = MouseHandler.MouseY();
            bool mDown = Mouse.GetState().IsButtonDown(MouseButton.Left);
            for (int i = 0; i < MenuItems.Count; i++)
            {
                if (MenuItems[i].Contains(mX, mY))
                {
                    if (!MenuItems[i].HoverInternal)
                    {
                        MenuItems[i].HoverInternal = true;
                        MenuItems[i].MouseEnter();
                    }
                    if (mDown && !pDown)
                    {
                        MenuItems[i].MouseLeftDown();
                    }
                    else if (!mDown && pDown)
                    {
                        MenuItems[i].MouseLeftUp();
                    }
                }
                else if (MenuItems[i].HoverInternal)
                {
                    MenuItems[i].HoverInternal = false;
                    MenuItems[i].MouseLeave();
                }
                MenuItems[i].Tick(TheClient.Delta);
            }
            pDown = mDown;
        }

        public void RenderAll(double delta)
        {
            for (int i = 0; i < MenuItems.Count; i++)
            {
                MenuItems[i].Render(delta, 0, 0);
            }
        }
    }
}
