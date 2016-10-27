//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.Shared;
using OpenTK;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    public class UILabel : UIElement
    {
        public string Text;

        public FontSet TextFont;

        public Func<int> MaxX = null;

        public Vector4 BackColor = Vector4.Zero;

        public string BColor = "^r^7";

        public UILabel(string btext, FontSet font, UIAnchor anchor, Func<int> xOff, Func<int> yOff, Func<int> maxx = null)
            : base(anchor, () => 0, () => 0, xOff, yOff)
        {
            Text = btext;
            TextFont = font;
            Width = () => (float)TextFont.MeasureFancyLinesOfText(MaxX != null ? TextFont.SplitAppropriately(Text, MaxX()) : Text, BColor).X;
            Height = () => (float)TextFont.MeasureFancyLinesOfText(MaxX != null ? TextFont.SplitAppropriately(Text, MaxX()) : Text, BColor).Y;
            MaxX = maxx;
        }

        protected override void Render(double delta, int xoff, int yoff)
        {
            string tex = MaxX != null ? TextFont.SplitAppropriately(Text, MaxX()) : Text;
            float bx = GetX() + xoff;
            float by = GetY() + yoff;
            if (BackColor.W > 0)
            {
                Location meas = TextFont.MeasureFancyLinesOfText(tex);
                Client TheClient = GetClient();
                TheClient.Rendering.SetColor(BackColor);
                TheClient.Rendering.RenderRectangle(bx, by, bx + (float)meas.X, by + (float)meas.Y);
                TheClient.Rendering.SetColor(Vector4.One);
            }
            TextFont.DrawColoredText(tex, new Location(bx, by, 0), int.MaxValue, 1, false, BColor);
        }
    }
}
