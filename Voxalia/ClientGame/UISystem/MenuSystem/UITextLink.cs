//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.Shared;

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    public class UITextLink : UIMenuItem
    {
        public Action ClickedTask;

        public string Text;

        public string TextHover;

        public string TextClick;

        public string BColor = "^r^7";

        public bool Hovered = false;

        public bool Clicked = false;

        public Func<float> XGet;

        public Func<float> YGet;

        public FontSet TextFont;

        public Texture Icon;

        public System.Drawing.Color IconColor = System.Drawing.Color.White;

        public UITextLink(Texture ico, string btext, string btexthover, string btextclick, Action clicked, Func<float> xer, Func<float> yer, FontSet font)
        {
            Icon = ico;
            ClickedTask = clicked;
            Text = btext;
            TextHover = btexthover;
            TextClick = btextclick;
            TextFont = font;
            XGet = xer;
            YGet = yer;
        }

        public override void MouseEnter()
        {
            Hovered = true;
        }

        public override void MouseLeave()
        {
            Hovered = false;
            Clicked = false;
        }

        public override void MouseLeftDown()
        {
            Hovered = true;
            Clicked = true;
        }

        public override void MouseLeftUp()
        {
            if (Clicked && Hovered)
            {
                ClickedTask.Invoke();
            }
            Clicked = false;
        }

        public override void Init()
        {
        }

        public override void Render(double delta, int xoff, int yoff)
        {
            string tt = Text;
            if (Clicked)
            {
                tt = TextClick;
            }
            else if (Hovered)
            {
                tt = TextHover;
            }
            if (Icon != null)
            {
                float x = GetX() + xoff;
                float y = GetY() + yoff;
                Icon.Bind();
                Menus.TheClient.Rendering.SetColor(IconColor);
                Menus.TheClient.Rendering.RenderRectangle(x, y, x + TextFont.font_default.Height, y + TextFont.font_default.Height);
                TextFont.DrawColoredText(tt, new Location(x + TextFont.font_default.Height, y, 0), int.MaxValue, 1, false, BColor);
                Menus.TheClient.Rendering.SetColor(OpenTK.Vector4.One);
            }
            else
            {
                TextFont.DrawColoredText(tt, new Location(GetX() + xoff, GetY() + yoff, 0), int.MaxValue, 1, false, BColor);
            }
        }

        public float GetWidth()
        {
            return TextFont.MeasureFancyText(Text, BColor) + (Icon == null ? 0 :  TextFont.font_default.Height);
        }

        public float GetHeight()
        {
            return TextFont.font_default.Height;
        }

        public float GetX()
        {
            return XGet.Invoke();
        }

        public float GetY()
        {
            return YGet.Invoke();
        }

        public override bool Contains(int x, int y)
        {
            float tx = GetX();
            float ty = GetY();
            return x > tx && x < tx + GetWidth()
                && y > ty && y < ty + GetHeight();
        }
    }
}
