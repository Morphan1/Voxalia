using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ClientGame.UISystem.MenuSystem;

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    public class UIAnchor
    {
        public readonly Func<UIElement, int> GetX;
        public readonly Func<UIElement, int> GetY;

        private UIAnchor(Func<UIElement, int> x, Func<UIElement, int> y)
        {
            GetX = x;
            GetY = y;
        }

        public static readonly UIAnchor TOP_LEFT = new UIAnchor(LEFT_X, TOP_Y);
        public static readonly UIAnchor TOP_CENTER = new UIAnchor(CENTER_X, TOP_Y);
        public static readonly UIAnchor TOP_RIGHT = new UIAnchor(RIGHT_X, TOP_Y);
        public static readonly UIAnchor CENTER_LEFT = new UIAnchor(LEFT_X, CENTER_Y);
        public static readonly UIAnchor CENTER = new UIAnchor(CENTER_X, CENTER_Y);
        public static readonly UIAnchor CENTER_RIGHT = new UIAnchor(RIGHT_X, CENTER_Y);
        public static readonly UIAnchor BOTTOM_LEFT = new UIAnchor(LEFT_X, BOTTOM_Y);
        public static readonly UIAnchor BOTTOM_CENTER = new UIAnchor(CENTER_X, BOTTOM_Y);
        public static readonly UIAnchor BOTTOM_RIGHT = new UIAnchor(RIGHT_X, BOTTOM_Y);

        private static Func<UIElement, int> LEFT_X = (element) => 0;
        private static Func<UIElement, int> CENTER_X = (element) => (int)(element.GetWidth() / 2);
        private static Func<UIElement, int> RIGHT_X = (element) => (int)element.GetWidth();
        private static Func<UIElement, int> TOP_Y = (element) => 0;
        private static Func<UIElement, int> CENTER_Y = (element) => (int)(element.GetHeight() / 2);
        private static Func<UIElement, int> BOTTOM_Y = (element) => (int)element.GetHeight();
    }
}
