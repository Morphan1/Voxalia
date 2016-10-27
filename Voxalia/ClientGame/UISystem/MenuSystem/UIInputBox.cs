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
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.Shared;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    public class UIInputBox : UIElement
    {
        public string Text;
        public string Info;
        public FontSet Fonts;

        public bool Selected = false;
        public bool MultiLine = false;
        public int MinCursor = 0;
        public int MaxCursor = 0;

        public UIInputBox(string text, string info, FontSet fonts, UIAnchor anchor, Func<float> width, Func<int> xOff, Func<int> yOff)
            : base(anchor, width, () => fonts.font_default.Height, xOff, yOff)
        {
            Text = text;
            Info = info;
            Fonts = fonts;
        }

        public bool MDown = false;

        public int MStart = 0;

        protected override void MouseLeftDown()
        {
            MDown = true;
            Selected = true;
            /* KeyHandlerState khs = */KeyHandler.GetKBState();
            int xs = GetX();
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

        protected override void MouseLeftDownOutside()
        {
            Selected = false;
        }

        protected override void MouseLeftUp()
        {
            AdjustMax();
            MDown = false;
        }

        protected void AdjustMax()
        {
            int xs = GetX();
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

        protected override void Tick(double delta)
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
                    if (MaxCursor > Text.Length)
                    {
                        MaxCursor = Text.Length;
                        if (MinCursor > MaxCursor)
                        {
                            MinCursor = MaxCursor;
                        }
                    }
                    if (EnterPressed != null)
                    {
                        EnterPressed();
                    }
                }
                if (TextModified != null)
                {
                    TextModified.Invoke(this, null);
                }
            }
        }

        public EventHandler<EventArgs> TextModified;

        public Action EnterPressed;

        protected override void Render(double delta, int xoff, int yoff)
        {
            string typed = Text;
            int c = 0;
            int cmax = 0;
            Client TheClient = GetClient();
            if (!TheClient.CVars.u_colortyping.ValueB)
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
            int x = GetX() + xoff;
            int y = GetY() + yoff;
            int w = (int)GetWidth();
            TheClient.Textures.White.Bind();
            TheClient.Rendering.SetColor(Color4.White);
            TheClient.Rendering.RenderRectangle(x - 1, y - 1, x + w + 1, y + Fonts.font_default.Height + 1);
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(x, TheClient.Window.Height - (y + (int)Fonts.font_default.Height), w, (int)Fonts.font_default.Height);
            if (Selected)
            {
                float textw = Fonts.MeasureFancyText(typed.Substring(0, MinCursor + c));
                float textw2 = Fonts.MeasureFancyText(typed.Substring(0, MaxCursor + cmax));
                TheClient.Rendering.SetColor(new Color4(0f, 0.2f, 1f, 0.5f));
                TheClient.Rendering.RenderRectangle(x + textw, y, x + textw2 + 1, y + Fonts.font_default.Height);
            }
            TheClient.Rendering.SetColor(Color4.White);
            Fonts.DrawColoredText((typed.Length == 0 ? ("^)^i" + Info) : ("^0" + typed)), new Location(x, y, 0));
            GL.Scissor(0, 0, TheClient.Window.Width, TheClient.Window.Height); // TODO: Bump around a stack, for embedded scroll groups?
            GL.Disable(EnableCap.ScissorTest);
        }
    }
}
