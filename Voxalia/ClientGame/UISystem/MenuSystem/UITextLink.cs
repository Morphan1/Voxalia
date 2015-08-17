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

        public bool Hovered = false;

        public bool Clicked = false;

        public Func<float> XGet;

        public Func<float> YGet;

        public FontSet TextFont;

        public UITextLink(string btext, string btexthover, string btextclick, Action clicked, Func<float> xer, Func<float> yer, FontSet font)
        {
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

        public override void Render(double delta)
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
            float len = TextFont.MeasureFancyText(Text);
            float hei = TextFont.font_default.Height;
            TextFont.DrawColoredText(tt, new Location(GetX(), GetY(), 0));
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
