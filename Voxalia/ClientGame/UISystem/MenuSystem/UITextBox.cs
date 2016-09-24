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

        public int MinCursor = 0;

        public int MaxCursor = 0;
        
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

        public bool MDown = false;

        public int MStart = 0;

        public override void MouseLeftDown()
        {
            MDown = true;
            Selected = true;
            /* KeyHandlerState khs = */KeyHandler.GetKBState();
            int xs = X();
            for (int i = 0; i < Text.Length; i++)
            {
                if (xs + Fonts.MeasureFancyText(Text.Substring(0, i)) > MouseHandler.MouseX())
                {
                    MinCursor = i;
                    MaxCursor = i;
                    MStart = i;
                    return;
                }
            }
            MinCursor = Text.Length;
            MaxCursor = Text.Length;
            MStart = Text.Length;
        }

        public override void MouseLeftDownOutside()
        {
            Selected = false;
        }

        public override void MouseLeftUp()
        {
            AdjustMax();
            MDown = false;
        }

        public void AdjustMax()
        {
            int xs = X();
            for (int i = 0; i < Text.Length; i++)
            {
                if (xs + Fonts.MeasureFancyText(Text.Substring(0, i)) > MouseHandler.MouseX())
                {
                    MinCursor = Math.Min(i, MStart);
                    MaxCursor = Math.Max(i, MStart);
                    return;
                }
            }
            MaxCursor = Text.Length;
        }

        public override void Tick(double delta)
        {
            if (MDown)
            {
                AdjustMax();
            }
            if (Selected)
            {
                int min = MinCursor;
                int max = MaxCursor;
                if (min > max)
                {
                    MinCursor = max;
                    MaxCursor = min;
                }
                KeyHandlerState khs = KeyHandler.GetKBState();
                if (khs.InitBS > 0)
                {
                    int end = MinCursor - Math.Min(khs.InitBS, MinCursor);
                    Text = Text.Substring(0, end) + Text.Substring(MaxCursor);
                    MinCursor = end;
                    MaxCursor = end;
                }
                if (khs.KeyboardString.Length > 0)
                {
                    Text = Text.Substring(0, MinCursor) + khs.KeyboardString + Text.Substring(MaxCursor);
                    MinCursor = MinCursor + khs.KeyboardString.Length;
                    MaxCursor = MinCursor;
                }
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
            string typed = Text;
            int c = 0;
            int cmax = 0;
            if (!Menus.TheClient.CVars.u_colortyping.ValueB)
            {
                for (int i = 0; i < typed.Length && i < MinCursor; i++)
                {
                    if (typed[i] == '^')
                    {
                        c++;
                    }
                }
                for (int i = 0; i < typed.Length && i < MaxCursor; i++)
                {
                    if (typed[i] == '^')
                    {
                        cmax++;
                    }
                }
                typed = typed.Replace("^", "^^n");
            }
            int x = X() + xoff;
            int y = Y() + yoff;
            int w = Width();
            Menus.TheClient.Textures.White.Bind();
            Menus.TheClient.Rendering.SetColor(Color4.White);
            Menus.TheClient.Rendering.RenderRectangle(x - 1, y - 1, x + w + 1, y + Fonts.font_default.Height + 1);
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(x, Menus.TheClient.Window.Height - (y + (int)Fonts.font_default.Height), w, (int)Fonts.font_default.Height);
            if (Selected)
            {
                float textw = Fonts.MeasureFancyText(typed.Substring(0, MinCursor + c));
                float textw2 = Fonts.MeasureFancyText(typed.Substring(0, MaxCursor + cmax));
                Menus.TheClient.Rendering.SetColor(new Color4(0f, 0.2f, 1f, 0.5f));
                Menus.TheClient.Rendering.RenderRectangle(x + textw, y, x + textw2 + 1, y + Fonts.font_default.Height);
            }
            Menus.TheClient.Rendering.SetColor(Color4.White);
            Fonts.DrawColoredText((typed.Length == 0 ? ("^)^i" + Info): ("^0" + typed)), new Location(x, y, 0));
            GL.Scissor(0, 0, Menus.TheClient.Window.Width, Menus.TheClient.Window.Height); // TODO: Bump around a stack, for embedded scroll groups?
            GL.Disable(EnableCap.ScissorTest);
        }
    }
}
