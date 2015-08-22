using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ClientGame.UISystem;
using Voxalia.ClientGame.UISystem.MenuSystem;
using OpenTK;

namespace Voxalia.ClientGame.ClientMainSystem
{
    class InventoryScreen: Screen
    {
        public UIMenu Menus;
        public UILabel Label_Inventory;
        public UITextLink Link_Equipment;
        public UITextLink Link_Exit;

        public override void Init()
        {
            Menus = new UIMenu(TheClient);
            Label_Inventory = new UILabel("^&Inventory", () => 20, () => 20, TheClient.FontSets.SlightlyBigger);
            Menus.Add(Label_Inventory);
            Link_Equipment = new UITextLink("Equipment", "^0^e^&Equipment", "^7^e^0Equipment", () =>
            {
                UIConsole.WriteLine("Equipment!");
            }, () => Label_Inventory.GetX() + Label_Inventory.GetWidth() + 20, () => Label_Inventory.GetY(), TheClient.FontSets.SlightlyBigger);
            Menus.Add(Link_Equipment);
            Link_Exit = new UITextLink("Exit", "^0^e^&Exit", "^7^e^0Exit", () =>
            {
                TheClient.ShowGame();
            }, () => TheClient.Window.Width - Link_Exit.GetWidth() - 20, () => TheClient.Window.Height - Link_Exit.GetHeight() - 20, TheClient.FontSets.SlightlyBigger);
            Menus.Add(Link_Exit);
        }

        public override void Tick()
        {
            TheClient.TheGameScreen.Tick();
            Menus.TickAll();
        }

        public override void SwitchTo()
        {
            TheClient.TheGameScreen.SwitchTo();
            MouseHandler.ReleaseMouse();
        }

        public override void Render()
        {
            TheClient.TheGameScreen.Render();
            TheClient.Establish2D();
            TheClient.Textures.Black.Bind();
            TheClient.Rendering.SetColor(new Vector4(1f, 1f, 1f, 0.7f));
            TheClient.Rendering.RenderRectangle(0, 0, TheClient.Window.Width, TheClient.Window.Height);
            TheClient.Rendering.SetColor(Vector4.One);
            Menus.RenderAll(TheClient.gDelta);
        }
    }
}
