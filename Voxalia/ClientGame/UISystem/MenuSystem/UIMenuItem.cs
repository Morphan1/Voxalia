namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    public abstract class UIMenuItem
    {
        public UIMenu Menus;

        public abstract void MouseEnter();

        public abstract void MouseLeave();

        public abstract void MouseLeftDown();

        public abstract void MouseLeftUp();

        public abstract void Init();

        public abstract void Render(double delta, int xoff, int yoff);

        public abstract bool Contains(int x, int y);

        public bool HoverInternal = false;

        public virtual void Tick(double delta)
        {
        }

        public virtual void MouseLeftDownOutside()
        {

        }
    }
}
