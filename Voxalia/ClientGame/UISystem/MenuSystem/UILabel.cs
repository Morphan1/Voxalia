using System;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.Shared;

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    public class UILabel : UIMenuItem
    {
        public string Text;
        
        public Func<float> XGet;

        public Func<float> YGet;

        public FontSet TextFont;

        public UILabel(string btext, Func<float> xer, Func<float> yer, FontSet font)
        {
            Text = btext;
            TextFont = font;
            XGet = xer;
            YGet = yer;
        }

        public override void MouseEnter()
        {
        }

        public override void MouseLeave()
        {
        }

        public override void MouseLeftDown()
        {
        }

        public override void MouseLeftUp()
        {
        }

        public override void Init()
        {
        }

        public override void Render(double delta)
        {
            TextFont.DrawColoredText(Text, new Location(GetX(), GetY(), 0));
        }

        public float GetWidth()
        {
            return TextFont.MeasureFancyText(Text);
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
            return x > tx && x < tx + TextFont.MeasureFancyText(Text)
                && y > ty && y < ty + TextFont.font_default.Height;
        }
    }
}
