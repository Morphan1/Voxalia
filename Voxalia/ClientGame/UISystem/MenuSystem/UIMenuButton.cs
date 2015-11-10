using System;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.Shared;

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    public class UIMenuButton: UIMenuItem
    {
        public Action ClickedTask;

        public string Text;

        public Texture Tex_None;

        public Texture Tex_Hover;

        public Texture Tex_Click;

        string tName;

        public bool Hovered = false;

        public bool Clicked = false;

        public float Height;

        public float Width;

        public float X;

        public float Y;

        public FontSet TextFont;

        public UIMenuButton(string buttontexname, string buttontext, Action clicked, float x, float y, float w, float h, FontSet font)
        {
            ClickedTask = clicked;
            Text = buttontext;
            tName = buttontexname;
            Height = h;
            Width = w;
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
            Tex_None = Menus.TheClient.Textures.GetTexture(tName + "_none");
            Tex_Hover = Menus.TheClient.Textures.GetTexture(tName + "_hover");
            Tex_Click = Menus.TheClient.Textures.GetTexture(tName + "_click");
        }

        public override void Render(double delta, int xoff, int yoff)
        {
            if (Clicked)
            {
                Tex_Click.Bind();
            }
            else if (Hovered)
            {
                Tex_Hover.Bind();
            }
            else
            {
                Tex_None.Bind();
            }
            Menus.TheClient.Rendering.RenderRectangle((int)X + xoff, (int)Y + yoff, (int)(X + xoff + Width), (int)(Y + yoff + Height));
            float len = TextFont.MeasureFancyText(Text);
            float hei = TextFont.font_default.Height;
            TextFont.DrawColoredText(Text, new Location(X + xoff + Width / 2 - len / 2, Y + yoff + Height / 2 - hei / 2, 0));
        }

        public override bool Contains(int x, int y)
        {
            return x > X && x < X + Width && y > Y && y < Y + Height;
        }
    }
}
