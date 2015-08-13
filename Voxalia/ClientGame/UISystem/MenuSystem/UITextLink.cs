using System;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.Shared;

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    public class UITextLink: UIMenuItem
    {
        public Action ClickedTask;

        public string Text;

        public string TextHover;

        public string TextClick;
        
        public bool Hovered = false;

        public bool Clicked = false;
        
        public float X;

        public float Y;

        public FontSet TextFont;

        public UITextLink(string btext, string btexthover, string btextclick, Action clicked, float x, float y, FontSet font)
        {
            ClickedTask = clicked;
            Text = btext;
            TextHover = btexthover;
            TextClick = btextclick;
            X = x;
            Y = y;
            TextFont = font;
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
            TextFont.DrawColoredText(tt, new Location(X, Y, 0));
        }

        public override bool Contains(int x, int y)
        {
            return x > X && x < X + TextFont.MeasureFancyText(Text) && y > Y && y < Y + TextFont.font_default.Height;
        }
    }
}
