using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ClientGame.UISystem;
using Voxalia.ClientGame.UISystem.MenuSystem;
using Voxalia.ClientGame.GraphicsSystems;
using OpenTK;
using OpenTK.Graphics;

namespace Voxalia.ClientGame.ClientMainSystem
{
    public partial class Client
    {
        public UIMenu InventoryMenu;

        public UIMenu EquipmentMenu;

        public UIMenu BuilderItemsMenu;

        public UIMenu CInvMenu = null;

        UITextLink InventoryExitButton()
        {
            return new UITextLink("Exit", "^0^e^7Exit", "^7^e^0Exit", () =>
            {
                HideInventory();
            },
            () => Window.Width - FontSets.SlightlyBigger.MeasureFancyText("Exit") - 20, () => Window.Height - FontSets.SlightlyBigger.font_default.Height - 20, FontSets.SlightlyBigger);
        }

        public void InitInventory()
        {
            CInvMenu = null;
            InventoryMenu = new UIMenu(this);
            UILabel inv_inventory = new UILabel("^(Inventory", () => 20, () => 20, FontSets.SlightlyBigger);
            UITextLink inv_equipment = new UITextLink("Equipment", "^0^e^7Equipment", "^7^e^0Equipment", () =>
            {
                CInvMenu = EquipmentMenu;
            },
            () => inv_inventory.GetX() + inv_inventory.GetWidth() + 20, () => inv_inventory.GetY(), FontSets.SlightlyBigger);
            UITextLink inv_builderitems = new UITextLink("Builder Items", "^0^e^&7uilder Items", "^7^e^0Builder Items", () =>
            {
                CInvMenu = BuilderItemsMenu;
            },
            () => inv_equipment.GetX() + inv_equipment.GetWidth() + 20, () => inv_equipment.GetY(), FontSets.SlightlyBigger);
            InventoryMenu.Add(inv_inventory);
            InventoryMenu.Add(inv_equipment);
            InventoryMenu.Add(inv_builderitems);
            InventoryMenu.Add(InventoryExitButton());
            EquipmentMenu = new UIMenu(this);
            UITextLink equ_inventory = new UITextLink("Inventory", "^0^e^7Inventory", "^7^e^0Inventory", () =>
            {
                CInvMenu = InventoryMenu;
            },
            () => 20, () => 20, FontSets.SlightlyBigger);
            UILabel equ_equipment = new UILabel("^(Equipment", () => equ_inventory.GetX() + equ_inventory.GetWidth() + 20, () => equ_inventory.GetY(), FontSets.SlightlyBigger);
            UITextLink equ_builderitems = new UITextLink("Builder Items", "^0^e^7Builder Items", "^7^e^0Builder Items", () =>
            {
                CInvMenu = BuilderItemsMenu;
            },
            () => equ_equipment.GetX() + equ_equipment.GetWidth() + 20, () => equ_equipment.GetY(), FontSets.SlightlyBigger);
            EquipmentMenu.Add(equ_inventory);
            EquipmentMenu.Add(equ_equipment);
            EquipmentMenu.Add(equ_builderitems);
            EquipmentMenu.Add(InventoryExitButton());
            BuilderItemsMenu = new UIMenu(this);
            UITextLink bui_inventory = new UITextLink("Inventory", "^0^e^7Inventory", "^7^e^0Inventory", () =>
            {
                CInvMenu = InventoryMenu;
            },
            () => 20, () => 20, FontSets.SlightlyBigger);
            UITextLink bui_equipment = new UITextLink("Equipment", "^0^e^7Equipment", "^7^e^0Equipment", () =>
            {
                CInvMenu = EquipmentMenu;
            },
            () => bui_inventory.GetX() + bui_inventory.GetWidth() + 20, () => bui_inventory.GetY(), FontSets.SlightlyBigger);
            UILabel bui_builderitems = new UILabel("^(Builder Items", () => bui_equipment.GetX() + bui_equipment.GetWidth() + 20, () => bui_equipment.GetY(), FontSets.SlightlyBigger);
            BuilderItemsMenu.Add(bui_inventory);
            BuilderItemsMenu.Add(bui_equipment);
            BuilderItemsMenu.Add(bui_builderitems);
            BuilderItemsMenu.Add(InventoryExitButton());
        }

        public void TickInvMenu()
        {
            if (CInvMenu != null)
            {
                CInvMenu.TickAll();
            }
        }

        bool invmousewascaptured = false;

        public void ShowInventory()
        {
            CInvMenu = InventoryMenu;
            invmousewascaptured = MouseHandler.MouseCaptured;
            if (invmousewascaptured)
            {
                MouseHandler.ReleaseMouse();
            }
        }

        public void HideInventory()
        {
            CInvMenu = null;
            if (invmousewascaptured)
            {
                MouseHandler.CaptureMouse();
            }
        }

        public void RenderInvMenu()
        {
            if (CInvMenu == null)
            {
                return;
            }
            Textures.White.Bind();
            Rendering.SetColor(new Vector4(0.5f, 0.5f, 0.5f, 0.7f));
            Rendering.RenderRectangle(0, 0, Window.Width, Window.Height);
            CInvMenu.RenderAll(gDelta);
        }
    }
}
