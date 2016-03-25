using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ClientGame.UISystem;
using Voxalia.ClientGame.UISystem.MenuSystem;
using Voxalia.ClientGame.GraphicsSystems;
using OpenTK;
using OpenTK.Graphics;
using FreneticScript;
using Voxalia.ClientGame.OtherSystems;

namespace Voxalia.ClientGame.ClientMainSystem
{
    public partial class Client
    {
        public UIMenu InventoryMenu;
        public UIScrollGroup UI_Inv_Items;
        public UITextBox UI_Inv_Filter;

        public UIMenu EquipmentMenu;

        public UIMenu BuilderItemsMenu;

        public UIMenu CInvMenu = null;

        UITextLink InventoryExitButton()
        {
            return new UITextLink(null, "Exit", "^0^e^7Exit", "^7^e^0Exit", () =>
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
            UITextLink inv_equipment = new UITextLink(null, "Equipment", "^0^e^7Equipment", "^7^e^0Equipment", () =>
            {
                CInvMenu = EquipmentMenu;
            }, () => inv_inventory.GetX() + inv_inventory.GetWidth() + 20, () => inv_inventory.GetY(), FontSets.SlightlyBigger);
            UITextLink inv_builderitems = new UITextLink(null, "Builder Items", "^0^e^&7uilder Items", "^7^e^0Builder Items", () =>
            {
                CInvMenu = BuilderItemsMenu;
            }, () => inv_equipment.GetX() + inv_equipment.GetWidth() + 20, () => inv_equipment.GetY(), FontSets.SlightlyBigger);
            InventoryMenu.Add(inv_inventory);
            InventoryMenu.Add(inv_equipment);
            InventoryMenu.Add(inv_builderitems);
            InventoryMenu.Add(InventoryExitButton());
            Func<int> height = () => (int)(inv_inventory.GetY() + inv_inventory.GetHeight() + 20 + FontSets.Standard.font_default.Height + 20);
            UI_Inv_Items = new UIScrollGroup(() => 20, height, ItemsListSize, () => Window.Height - height() - 20);
            UI_Inv_Filter = new UITextBox("", "Item Filter", () => 20, () => (int)(inv_inventory.GetY() + inv_inventory.GetHeight() + 20), 200, FontSets.Standard);
            UI_Inv_Filter.TextModified += (o, e) => UpdateInventoryMenu();
            InventoryMenu.Add(UI_Inv_Items);
            InventoryMenu.Add(UI_Inv_Filter);
            GenerateItemDescriptors();
            UpdateInventoryMenu();
            EquipmentMenu = new UIMenu(this);
            UITextLink equ_inventory = new UITextLink(null, "Inventory", "^0^e^7Inventory", "^7^e^0Inventory", () =>
            {
                CInvMenu = InventoryMenu;
            }, () => 20, () => 20, FontSets.SlightlyBigger);
            UILabel equ_equipment = new UILabel("^(Equipment", () => equ_inventory.GetX() + equ_inventory.GetWidth() + 20, () => equ_inventory.GetY(), FontSets.SlightlyBigger);
            UITextLink equ_builderitems = new UITextLink(null, "Builder Items", "^0^e^7Builder Items", "^7^e^0Builder Items", () =>
            {
                CInvMenu = BuilderItemsMenu;
            }, () => equ_equipment.GetX() + equ_equipment.GetWidth() + 20, () => equ_equipment.GetY(), FontSets.SlightlyBigger);
            EquipmentMenu.Add(equ_inventory);
            EquipmentMenu.Add(equ_equipment);
            EquipmentMenu.Add(equ_builderitems);
            EquipmentMenu.Add(InventoryExitButton());
            BuilderItemsMenu = new UIMenu(this);
            UITextLink bui_inventory = new UITextLink(null, "Inventory", "^0^e^7Inventory", "^7^e^0Inventory", () =>
            {
                CInvMenu = InventoryMenu;
            }, () => 20, () => 20, FontSets.SlightlyBigger);
            UITextLink bui_equipment = new UITextLink(null, "Equipment", "^0^e^7Equipment", "^7^e^0Equipment", () =>
            {
                CInvMenu = EquipmentMenu;
            }, () => bui_inventory.GetX() + bui_inventory.GetWidth() + 20, () => bui_inventory.GetY(), FontSets.SlightlyBigger);
            UILabel bui_builderitems = new UILabel("^(Builder Items", () => bui_equipment.GetX() + bui_equipment.GetWidth() + 20, () => bui_equipment.GetY(), FontSets.SlightlyBigger);
            BuilderItemsMenu.Add(bui_inventory);
            BuilderItemsMenu.Add(bui_equipment);
            BuilderItemsMenu.Add(bui_builderitems);
            BuilderItemsMenu.Add(InventoryExitButton());
        }

        int ItemsListSize = 250;

        UILabel UI_Inv_Displayname;
        UILabel UI_Inv_Description;
        UILabel UI_Inv_Detail;

        void GenerateItemDescriptors()
        {
            UI_Inv_Displayname = new UILabel("^B<Display name>", () => 20 + ItemsListSize, () => Window.Height / 2, FontSets.SlightlyBigger, () => Window.Width - (20 + ItemsListSize));
            UI_Inv_Description = new UILabel("^B<Description>", () => 20 + ItemsListSize, () => UI_Inv_Displayname.GetY() + UI_Inv_Displayname.GetHeight(), FontSets.Standard, () => Window.Width - (20 + ItemsListSize));
            UI_Inv_Detail = new UILabel("^B<Detail>", () => 20 + ItemsListSize, () => UI_Inv_Description.GetY() + UI_Inv_Description.GetHeight(), FontSets.Standard, () => Window.Width - (20 + ItemsListSize));
            UI_Inv_Description.BColor = "^r^7^i";
            UI_Inv_Detail.BColor = "^r^7^l";
            InventoryMenu.Add(UI_Inv_Displayname);
            InventoryMenu.Add(UI_Inv_Description);
            InventoryMenu.Add(UI_Inv_Detail);
        }

        public void InventorySelectItem(int slot)
        {
            ItemStack item = GetItemForSlot(slot);
            UI_Inv_Displayname.Text = "^B" + item.DisplayName;
            UI_Inv_Description.Text = "^r^7" + item.Name + (item.SecondaryName != null && item.SecondaryName.Length > 0 ? " [" + item.SecondaryName + "]" : "") + "\n>^B" + item.Description;
            UI_Inv_Detail.Text = "^BCount: " + item.Count + ", ColorCode: " + item.DrawColor + ", Texture: " + (item.Tex != null ? item.Tex.Name: "{NULL}")
                + ", Model: " + (item.Mod != null ? item.Mod.Name : "{NULL}") + ", Shared attributes: "+  item.SharedStr();
        }

        public void UpdateInventoryMenu()
        {
            UI_Inv_Items.Clear();
            string pref1 = "^0^e^7";
            string pref2 = "^7^e^0";
            UITextLink prev = new UITextLink(Textures.Clear, "Air", pref1 + "Air", pref2 + "Air", () =>
            {
                InventorySelectItem(0);
            }
            , () => 0, () => 0, FontSets.Standard);
            UI_Inv_Items.Add(prev);
            string filter = UI_Inv_Filter.Text;
            for (int i = 0; i < Items.Count; i++)
            {
                if (filter.Length == 0 || Items[i].ToString().ToLowerFast().Contains(filter.ToLowerFast()))
                {
                    string name = Items[i].DisplayName;
                    UITextLink p = prev;
                    int x = i;
                    UITextLink neo = new UITextLink(Items[i].Tex, name, pref1 + name, pref2 + name, () =>
                    {
                        InventorySelectItem(x + 1);
                    }
                    , () => p.GetX(), () => p.GetY() + p.GetHeight(), FontSets.Standard);
                    UI_Inv_Items.Add(neo);
                    prev = neo;
                }
            }
        }

        public void TickInvMenu()
        {
            if (CInvMenu != null)
            {
                CInvMenu.TickAll();
            }
        }

        public bool InvShown()
        {
            return CInvMenu != null;
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
