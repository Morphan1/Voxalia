//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.Shared;

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    public class UITextLink : UIElement
    {
        public Action ClickedTask;

        public string Text;

        public string TextHover;

        public string TextClick;

        public string BColor = "^r^7";

        public bool Hovered = false;

        public bool Clicked = false;

        public FontSet TextFont;

        public Texture Icon;

        public System.Drawing.Color IconColor = System.Drawing.Color.White;

        public UITextLink(Texture ico, string btext, string btexthover, string btextclick, FontSet font, Action clicked, UIAnchor anchor, Func<int> xOff, Func<int> yOff)
            : base(anchor, () => 0, () => 0, xOff, yOff)
        {
            Icon = ico;
            ClickedTask = clicked;
            Text = btext;
            TextHover = btexthover;
            TextClick = btextclick;
            TextFont = font;
            Width = () => font.MeasureFancyText(Text, BColor) + (Icon == null ? 0 : font.font_default.Height);
            Height = () => TextFont.font_default.Height;
        }

        protected override void MouseEnter()
        {
            Hovered = true;
        }

        protected override void MouseLeave()
        {
            Hovered = false;
            Clicked = false;
        }

        protected override void MouseLeftDown()
        {
            Hovered = true;
            Clicked = true;
        }

        protected override void MouseLeftUp()
        {
            if (Clicked && Hovered)
            {
                ClickedTask.Invoke();
            }
            Clicked = false;
        }

        protected override void Render(double delta, int xoff, int yoff)
        {
            string tt = Text;
            if (Clicked)
            {
                SysConsole.Output(OutputType.DEBUG, "TextClick!");
                tt = TextClick;
            }
            else if (Hovered)
            {
                SysConsole.Output(OutputType.DEBUG, "TextHover!");
                tt = TextHover;
            }
            if (Icon != null)
            {
                float x = GetX() + xoff;
                float y = GetY() + yoff;
                Icon.Bind();
                Client TheClient = GetClient();
                TheClient.Rendering.SetColor(IconColor);
                TheClient.Rendering.RenderRectangle(x, y, x + TextFont.font_default.Height, y + TextFont.font_default.Height);
                TextFont.DrawColoredText(tt, new Location(x + TextFont.font_default.Height, y, 0), int.MaxValue, 1, false, BColor);
                TheClient.Rendering.SetColor(OpenTK.Vector4.One);
            }
            else
            {
                TextFont.DrawColoredText(tt, new Location(GetX() + xoff, GetY() + yoff, 0), int.MaxValue, 1, false, BColor);
            }
        }
    }
}
