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
using System.Threading.Tasks;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    public abstract class UIElement
    {
        /// <summary>
        /// Do not access directly, except for debugging.
        /// TODO: Why is this a HashSet?
        /// </summary>
        public HashSet<UIElement> Children;
        
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
            Anchor = anchor != null ? anchor : UIAnchor.TOP_LEFT;
            Width = width != null ? width : () => 0;
            Height = height != null ? height : () => 0;
            OffsetX = xOff != null ? xOff : () => 0;
            OffsetY = yOff != null ? yOff : () => 0;
        }

        public void AddChild(UIElement child)
        {
            if (child.Parent != null)
            {
                throw new Exception("Tried to add a child that already has a parent!");
            }
            if (!Children.Contains(child))
            {
                if (!ToAdd.Add(child))
                {
                    throw new Exception("Tried to add a child twice!");
                }
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
            return Children.Contains(element) && !ToRemove.Contains(element);
        }

        public virtual Client GetClient()
        {
            return Parent != null ? Parent.GetClient() : null;
        }

        public int GetX()
        {
            return (Parent != null ? (int)Anchor.GetX(this) : 0) + OffsetX();
        }

        public int GetY()
        {
            return (Parent != null ? (int)Anchor.GetY(this) : 0) + OffsetY();
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

        private HashSet<UIElement> ToAdd = new HashSet<UIElement>();
        private HashSet<UIElement> ToRemove = new HashSet<UIElement>();

        public void CheckChildren()
        {
            foreach (UIElement element in ToAdd)
            {
                if (Children.Add(element))
                {
                    element.Parent = this;
                    element.Init();
                }
                else
                {
                    throw new Exception("Failed to add a child!");
                }
            }
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
            ToAdd.Clear();
            ToRemove.Clear();
        }

        public void FullTick(double delta)
        {
            CheckChildren();
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
                element.FullTick(delta);
            }
        }

        public void FullRender(double delta, int xoff, int yoff)
        {
            if (Parent == null || !Parent.ToRemove.Contains(this))
            {
                Render(delta, xoff, yoff);
                RenderChildren(delta, GetX() + xoff, GetY() + yoff);
            }
        }

        protected virtual void Render(double delta, int xoff, int yoff)
        {
        }

        protected virtual void RenderChildren(double delta, int xoff, int yoff)
        {
            CheckChildren();
            foreach (UIElement element in Children)
            {
                element.FullRender(delta, xoff, yoff);
            }
        }

        public void MouseEnter(int x, int y)
        {
            MouseEnter();
            foreach (UIElement child in GetAllAt(x, y))
            {
                child.MouseEnter(x, y);
            }
        }

        public void MouseLeave(int x, int y)
        {
            MouseLeave();
            foreach (UIElement child in GetAllNotAt(x, y))
            {
                child.MouseLeave(x, y);
            }
        }

        public void MouseLeftDown(int x, int y)
        {
            MouseLeftDown();
            foreach (UIElement child in GetAllAt(x, y))
            {
                child.MouseLeftDown(x, y);
            }
        }

        public void MouseLeftDownOutside(int x, int y)
        {
            MouseLeftDownOutside();
            foreach (UIElement child in GetAllNotAt(x, y))
            {
                child.MouseLeftDownOutside(x, y);
            }
        }

        public void MouseLeftUp(int x, int y)
        {
            MouseLeftUp();
            foreach (UIElement child in GetAllAt(x, y))
            {
                child.MouseLeftUp(x, y);
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
            foreach (UIElement element in Children)
            {
                if (element.Contains(x, y))
                {
                    found.Add(element);
                }
            }
            return found;
        }

        protected virtual HashSet<UIElement> GetAllNotAt(int x, int y)
        {
            HashSet<UIElement> found = new HashSet<UIElement>();
            foreach (UIElement element in Children)
            {
                if (!element.Contains(x, y))
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
