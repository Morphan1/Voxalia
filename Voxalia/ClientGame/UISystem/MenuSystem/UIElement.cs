using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    public abstract class UIElement
    {
        protected HashSet<UIElement> Children;
        
        public bool HoverInternal;

        public UIElement Parent
        {
            get;
            private set;
        }

        private UIAnchor Anchor;
        protected Func<float> Width;
        protected Func<float> Height;
        private Func<int> OffsetX;
        private Func<int> OffsetY;

        public UIElement(UIAnchor anchor, Func<float> width, Func<float> height, Func<int> xOff, Func<int> yOff)
        {
            Children = new HashSet<UIElement>();
            Anchor = anchor;
            Width = width;
            Height = height;
            OffsetX = xOff;
            OffsetY = yOff;
        }

        public void AddChild(UIElement child)
        {
            if (child.Parent != null)
            {
                throw new Exception("Tried to add a child that already has a parent!");
            }
            if (Children.Add(child))
            {
                child.Parent = this;
                child.Init();
            }
            else
            {
                throw new Exception("Tried to add a child that already belongs to this element!");
            }
        }

        public void RemoveChild(UIElement child)
        {
            if (Children.Contains(child))
            {
                if (!ToRemove.Add(child))
                {
                    throw new Exception("Tried to remove a child twice!");
                }
            }
            else
            {
                throw new Exception("Tried to remove a child that does not belong to this element!");
            }
        }

        public void RemoveAllChildren()
        {
            foreach (UIElement child in Children)
            {
                RemoveChild(child);
            }
        }

        public bool HasChild(UIElement element)
        {
            return Children.Contains(element);
        }

        public virtual Client GetClient()
        {
            return Parent != null ? Parent.GetClient() : null;
        }

        public int GetX()
        {
            return (Parent != null ? (int)Anchor.GetX(Parent) : 0) + OffsetX.Invoke();
        }

        public int GetY()
        {
            return (Parent != null ? (int)Anchor.GetY(Parent) : 0) + OffsetY.Invoke();
        }

        public float GetWidth()
        {
            return Width.Invoke();
        }

        public float GetHeight()
        {
            return Height.Invoke();
        }

        public bool Contains(int x, int y)
        {
            foreach (UIElement child in Children)
            {
                if (child.Contains(x, y))
                {
                    return true;
                }
            }
            return SelfContains(x, y);
        }

        protected bool SelfContains(int x, int y)
        {
            int lowX = GetX();
            int lowY = GetY();
            int highX = lowX + (int)GetWidth();
            int highY = lowY + (int)GetHeight();
            return x > lowX && x < highX
                && y > lowY && y < highY;
        }

        private HashSet<UIElement> ToRemove = new HashSet<UIElement>();

        public void FullTick(double delta)
        {
            foreach (UIElement element in ToRemove)
            {
                if (Children.Remove(element))
                {
                    element.Parent = null;
                }
                else
                {
                    throw new Exception("Failed to remove a child!");
                }
            }
            ToRemove.Clear();
            Tick(delta);
            TickChildren(delta);
        }

        protected virtual void Tick(double delta)
        {
        }

        protected virtual void TickChildren(double delta)
        {
            foreach (UIElement element in Children)
            {
                element.Tick(delta);
                element.TickChildren(delta);
            }
        }

        public void FullRender(double delta, int xoff, int yoff)
        {
            if (Parent == null || !Parent.ToRemove.Contains(this))
            {
                int x = GetX();
                int y = GetY();
                Render(delta, x, y);
                RenderChildren(delta, x, y);
            }
        }

        protected virtual void Render(double delta, int xoff, int yoff)
        {
        }

        protected virtual void RenderChildren(double delta, int xoff, int yoff)
        {

        }

        public void MouseEnter(int x, int y)
        {
            foreach (UIElement child in GetAllAt(x, y))
            {
                child.MouseEnter();
            }
        }

        public void MouseLeave(int x, int y)
        {
            foreach (UIElement child in GetAllAt(x, y))
            {
                child.MouseLeave();
            }
        }

        public void MouseLeftDown(int x, int y)
        {
            foreach (UIElement child in GetAllAt(x, y))
            {
                child.MouseLeftDown();
            }
        }

        public void MouseLeftDownOutside(int x, int y)
        {
            foreach (UIElement child in GetAllAt(x, y))
            {
                child.MouseLeftDownOutside();
            }
        }

        public void MouseLeftUp(int x, int y)
        {
            foreach (UIElement child in GetAllAt(x, y))
            {
                child.MouseLeftUp();
            }
        }

        protected virtual void MouseEnter()
        {
        }

        protected virtual void MouseLeave()
        {
        }

        protected virtual void MouseLeftDown()
        {
        }

        protected virtual void MouseLeftDownOutside()
        {
        }

        protected virtual void MouseLeftUp()
        {
        }

        protected virtual HashSet<UIElement> GetAllAt(int x, int y)
        {
            HashSet<UIElement> found = new HashSet<UIElement>();
            if (SelfContains(x, y))
            {
                found.Add(this);
            }
            foreach (UIElement element in Children)
            {
                if (element.Contains(x, y))
                {
                    found.Add(element);
                }
            }
            return found;
        }

        protected virtual void Init()
        {
        }
    }
}
