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
using Voxalia.ClientGame.UISystem;
using Voxalia.ClientGame.UISystem.MenuSystem;
using Voxalia.ClientGame.GraphicsSystems;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using FreneticScript;
using Voxalia.ClientGame.OtherSystems;
using Voxalia.Shared;
using Voxalia.ClientGame.GraphicsSystems.LightingSystem;

namespace Voxalia.ClientGame.ClientMainSystem
{
    public partial class Client
    {
        public UIScrollBox UI_Inv_Items;
        public UIInputBox UI_Inv_Filter;

        public UIGroup InventoryMenu;
        public UIGroup EquipmentMenu;
        public UIGroup BuilderItemsMenu;

        private UIGroup CInvMenu = null;

        public View3D MainItemView = new View3D();

        UITextLink InventoryExitButton()
        {
            return new UITextLink(null, "Exit", "^0^e^7Exit", "^7^e^0Exit", FontSets.SlightlyBigger, HideInventory, UIAnchor.BOTTOM_RIGHT,
                () => -(int)FontSets.SlightlyBigger.MeasureFancyText("Exit") - 20, () => -(int)FontSets.SlightlyBigger.font_default.Height - 20);
        }

        UIColoredBox InventoryBackground()
        {
            return new UIColoredBox(new Vector4(0.5f, 0.5f, 0.5f, 0.7f), UIAnchor.TOP_LEFT, TheGameScreen.GetWidth, TheGameScreen.GetHeight, () => 0, () => 0);
        }

        public void RenderMainItem(View3D view)
        {
            if (InvCurrent != null)
            {
                InvCurrent.Render3D(Location.Zero, (float)GlobalTickTimeLocal * 0.5f, new Location(6));
            }
        }

        public void FixInvRender()
        {
            MainItemView.Render3D = RenderMainItem;
            foreach (LightObject light in MainItemView.Lights)
            {
                foreach (Light li in light.InternalLights)
                {
                    li.Destroy();
                }
            }
            MainItemView.Lights.Clear();
            MainItemView.RenderClearAlpha = 0f;
            SkyLight tlight = new SkyLight(new Location(0, 0, 10), 64, Location.One, new Location(0, -1, -1).Normalize(), 64, false);
            MainItemView.Lights.Add(tlight);
            MainItemView.GenerateFBO();
            MainItemView.Generate(this, Window.Width, Window.Height);
        }

        public void InitInventory()
        {
            FixInvRender();
            CInvMenu = null;
            // Inventory Menu
            InventoryMenu = new UIGroup(UIAnchor.TOP_LEFT, TheGameScreen.GetWidth, TheGameScreen.GetHeight, () => 0, () => 0);
            UILabel inv_inventory = new UILabel("^(Inventory", FontSets.SlightlyBigger, UIAnchor.TOP_LEFT, () => 20, () => 20);
            UITextLink inv_equipment = new UITextLink(null, "Equipment", "^0^e^7Equipment", "^7^e^0Equipment", FontSets.SlightlyBigger,
                () => SetCurrent(EquipmentMenu), UIAnchor.TOP_LEFT, () => (int)(inv_inventory.GetX() + inv_inventory.GetWidth()) + 20, () => inv_inventory.GetY());
            UITextLink inv_builderitems = new UITextLink(null, "Builder-Items", "^0^e^&Builder-Items", "^7^e^0Builder-Items", FontSets.SlightlyBigger,
                () => SetCurrent(BuilderItemsMenu), UIAnchor.TOP_LEFT, () => (int)(inv_equipment.GetX() + inv_equipment.GetWidth()) + 20, () => inv_equipment.GetY());
            InventoryMenu.AddChild(InventoryBackground());
            InventoryMenu.AddChild(inv_inventory);
            InventoryMenu.AddChild(inv_equipment);
            InventoryMenu.AddChild(inv_builderitems);
            InventoryMenu.AddChild(InventoryExitButton());
            Func<int> height = () => inv_inventory.GetY() + (int)inv_inventory.GetHeight() + 20 + (int)FontSets.Standard.font_default.Height + 20;
            UI_Inv_Items = new UIScrollBox(UIAnchor.TOP_LEFT, () => ItemsListSize, () => Window.Height - (height() + 20), () => 20, height);
            UI_Inv_Filter = new UIInputBox("", "Item Filter", FontSets.Standard, UIAnchor.TOP_LEFT, () => ItemsListSize, () => 20, () => (int)(inv_inventory.GetY() + inv_inventory.GetHeight() + 20));
            UI_Inv_Filter.TextModified += (o, e) => UpdateInventoryMenu();
            InventoryMenu.AddChild(UI_Inv_Items);
            InventoryMenu.AddChild(UI_Inv_Filter);
            GenerateItemDescriptors();
            UpdateInventoryMenu();
            // Equipment Menu
            EquipmentMenu = new UIGroup(UIAnchor.TOP_LEFT, TheGameScreen.GetWidth, TheGameScreen.GetHeight, () => 0, () => 0);
            UITextLink equ_inventory = new UITextLink(null, "Inventory", "^0^e^7Inventory", "^7^e^0Inventory", FontSets.SlightlyBigger, () => SetCurrent(InventoryMenu), UIAnchor.TOP_LEFT, () => 20, () => 20);
            UILabel equ_equipment = new UILabel("^(Equipment", FontSets.SlightlyBigger, UIAnchor.TOP_LEFT, () => (int)(equ_inventory.GetX() + equ_inventory.GetWidth()) + 20, () => equ_inventory.GetY());
            UITextLink equ_builderitems = new UITextLink(null, "Builder-Items", "^0^e^7Builder-Items", "^7^e^0Builder-Items", FontSets.SlightlyBigger,
                () => SetCurrent(BuilderItemsMenu), UIAnchor.TOP_LEFT, () => (int)(equ_equipment.GetX() + equ_equipment.GetWidth()) + 20, () => equ_equipment.GetY());
            EquipmentMenu.AddChild(InventoryBackground());
            EquipmentMenu.AddChild(equ_inventory);
            EquipmentMenu.AddChild(equ_equipment);
            EquipmentMenu.AddChild(equ_builderitems);
            EquipmentMenu.AddChild(InventoryExitButton());
            // Builder-Items Menu
            BuilderItemsMenu = new UIGroup(UIAnchor.TOP_LEFT, TheGameScreen.GetWidth, TheGameScreen.GetHeight, () => 0, () => 0);
            UITextLink bui_inventory = new UITextLink(null, "Inventory", "^0^e^7Inventory", "^7^e^0Inventory", FontSets.SlightlyBigger, () => SetCurrent(InventoryMenu), UIAnchor.TOP_LEFT, () => 20, () => 20);
            UITextLink bui_equipment = new UITextLink(null, "Equipment", "^0^e^7Equipment", "^7^e^0Equipment", FontSets.SlightlyBigger,
                () => SetCurrent(EquipmentMenu), UIAnchor.TOP_LEFT, () => (int)(bui_inventory.GetX() + bui_inventory.GetWidth()) + 20, () => bui_inventory.GetY());
            UILabel bui_builderitems = new UILabel("^(Builder Items", FontSets.SlightlyBigger, UIAnchor.TOP_LEFT, () => (int)(bui_equipment.GetX() + bui_equipment.GetWidth()) + 20, () => bui_equipment.GetY());
            BuilderItemsMenu.AddChild(InventoryBackground());
            BuilderItemsMenu.AddChild(bui_inventory);
            BuilderItemsMenu.AddChild(bui_equipment);
            BuilderItemsMenu.AddChild(bui_builderitems);
            BuilderItemsMenu.AddChild(InventoryExitButton());
        }

        private void SetCurrent(UIGroup menu)
        {
            if (CInvMenu == menu)
            {
                return;
            }
            if (CInvMenu != null)
            {
                TheGameScreen.RemoveChild(CInvMenu);
            }
            CInvMenu = menu;
            if (menu != null)
            {
                TheGameScreen.AddChild(menu);
            }
        }

        int ItemsListSize = 250;

        UILabel UI_Inv_Displayname;
        UILabel UI_Inv_Description;
        UILabel UI_Inv_Detail;

        void GenerateItemDescriptors()
        {
            UI_Inv_Displayname = new UILabel("^B<Display name>", FontSets.SlightlyBigger, UIAnchor.CENTER_LEFT, () => 20 + ItemsListSize, () => 0, () => Window.Width - (20 + ItemsListSize));
            UI_Inv_Description = new UILabel("^B<Description>", FontSets.Standard, UIAnchor.TOP_LEFT, () => 20 + ItemsListSize,
                () => (int)(UI_Inv_Displayname.GetY() + UI_Inv_Displayname.GetHeight()), () => (int)TheGameScreen.GetWidth() - (20 + ItemsListSize));
            UI_Inv_Detail = new UILabel("^B<Detail>", FontSets.Standard, UIAnchor.TOP_LEFT, () => 20 + ItemsListSize,
                () => (int)(UI_Inv_Description.GetY() + UI_Inv_Description.GetHeight()), () => (int)TheGameScreen.GetWidth() - (20 + ItemsListSize));
            UI_Inv_Description.BColor = "^r^7^i";
            UI_Inv_Detail.BColor = "^r^7^l";
            InventoryMenu.AddChild(UI_Inv_Displayname);
            InventoryMenu.AddChild (UI_Inv_Description);
            InventoryMenu.AddChild(UI_Inv_Detail);
        }

        ItemStack InvCurrent = null;

        public void InventorySelectItem(int slot)
        {
            ItemStack item = GetItemForSlot(slot);
            InvCurrent = item;
            UI_Inv_Displayname.Text = "^B" + item.DisplayName;
            UI_Inv_Description.Text = "^r^7" + item.Name + (item.SecondaryName != null && item.SecondaryName.Length > 0 ? " [" + item.SecondaryName + "]" : "") + "\n>^B" + item.Description;
            UI_Inv_Detail.Text = "^BCount: " + item.Count + ", Color: " + item.DrawColor + ", Texture: " + (item.Tex != null ? item.Tex.Name: "{NULL}")
                + ", Model: " + (item.Mod != null ? item.Mod.Name : "{NULL}") + ", Shared attributes: "+  item.SharedStr();
        }

        public void UpdateInventoryMenu()
        {
            UI_Inv_Items.RemoveAllChildren();
            string pref1 = "^0^e^7";
            string pref2 = "^7^e^0";
            UITextLink prev = new UITextLink(Textures.Clear, "Air", pref1 + "Air", pref2 + "Air", FontSets.Standard, () => InventorySelectItem(0), UIAnchor.TOP_LEFT, () => 0, () => 0);
            UI_Inv_Items.AddChild(prev);
            string filter = UI_Inv_Filter.Text;
            for (int i = 0; i < Items.Count; i++)
            {
                if (filter.Length == 0 || Items[i].ToString().ToLowerFast().Contains(filter.ToLowerFast()))
                {
                    string name = Items[i].DisplayName;
                    UITextLink p = prev;
                    int x = i;
                    UITextLink neo = new UITextLink(Items[i].Tex, name, pref1 + name, pref2 + name, FontSets.Standard, () => InventorySelectItem(x + 1), UIAnchor.TOP_LEFT, p.GetX, () => (int)(p.GetY() + p.GetHeight()));
                    neo.IconColor = Items[i].DrawColor;
                    UI_Inv_Items.AddChild(neo);
                    prev = neo;
                }
            }
        }

        Location Forw = new Location(0, 0, -1);

        public void TickInvMenu()
        {
            if (CInvMenu != null)
            {
                MainItemView.CameraPos = -Forw * 10;
                MainItemView.ForwardVec = Forw;
                MainItemView.CameraUp = () => Location.UnitY; // TODO: Should this really be Y? Probably not...
                View3D temp = MainWorldView;
                MainWorldView = MainItemView;
                MainItemView.Render();
                MainWorldView = temp;
                View3D.CheckError("ItemRender");
            }
        }

        public bool InvShown()
        {
            return CInvMenu != null;
        }
        
        public void ShowInventory()
        {
            SetCurrent(InventoryMenu);
            FixMouse();
        }

        public void HideInventory()
        {
            SetCurrent(null);
            FixMouse();
        }
    }
}
