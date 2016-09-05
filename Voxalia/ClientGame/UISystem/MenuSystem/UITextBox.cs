using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.Shared;

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    public class UITextBox : UIMenuItem
    {
        public Func<int> X;

        public Func<int> Y;

        public string Info;

        public string Text;

        public Func<int> Width;

        public FontSet Fonts;

        public bool Selected = false;

        public bool MultiLine = false;
        
        public UITextBox(string def, string info, Func<int> x, Func<int> y, Func<int> width, FontSet fonts)
        {
            Text = def;
            Info = info;
            X = x;
            Y = y;
            Width = width;
            Fonts = fonts;
        }

        public override bool Contains(int x, int y)
        {
            int mx = X();
            int my = Y();
            return x >= mx && x <= mx + Width() && y >= my && y <= my + Fonts.font_default.Height;
        }

        public override void Init()
        {
        }

        public override void MouseEnter()
        {
        }

        public override void MouseLeave()
        {
        }

        public override void MouseLeftDown()
        {
            Selected = true;
            /* KeyHandlerState khs = */KeyHandler.GetKBState();
        }

        public override void MouseLeftDownOutside()
        {
            Selected = false;
        }

        public override void MouseLeftUp()
        {
        }

        public override void Tick(double delta)
        {
            if (Selected)
            {
                KeyHandlerState khs = KeyHandler.GetKBState();
                Text = Text.Substring(0, Text.Length - Math.Min(khs.InitBS, Text.Length));
                Text += khs.KeyboardString;
                if (!MultiLine && Text.Contains('\n'))
                {
                    Text = Text.Substring(0, Text.IndexOf('\n'));
                    EnterPressed();
                }
                if (TextModified != null)
                {
                    TextModified.Invoke(this, null);
                }
            }
        }

        public EventHandler<EventArgs> TextModified;

        public Action EnterPressed;

        public override void Render(double delta, int xoff, int yoff)
        {
            int x = X() + xoff;
            int y = Y() + yoff;
            int w = Width();
            Menus.TheClient.Textures.White.Bind();
            Menus.TheClient.Rendering.SetColor(Color4.White);
            Menus.TheClient.Rendering.RenderRectangle(x - 1, y - 1, x + w + 1, y + Fonts.font_default.Height + 1);
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(x, Menus.TheClient.Window.Height - (y + (int)Fonts.font_default.Height), w, (int)Fonts.font_default.Height);
            Fonts.DrawColoredText((Selected ? "^0|": "") + (Text.Length == 0 ? ("^)^i" + Info): ("^0" + Text)), new Location(x, y, 0));
            GL.Scissor(0, 0, Menus.TheClient.Window.Width, Menus.TheClient.Window.Height); // TODO: Bump around a stack, for embedded scroll groups?
            GL.Disable(EnableCap.ScissorTest);
        }
    }
}
