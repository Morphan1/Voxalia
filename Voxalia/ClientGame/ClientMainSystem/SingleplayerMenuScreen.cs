using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ClientGame.UISystem;
using Voxalia.ClientGame.UISystem.MenuSystem;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.Shared;

namespace Voxalia.ClientGame.ClientMainSystem
{
    class SingleplayerMenuScreen: Screen
    {
        public UIMenu Menus;

        public override void Init()
        {
            Menus = new UIMenu(TheClient);
            Menus.Add(new UIMenuButton("ui/menus/buttons/basic", "Back", () => {
                TheClient.ShowMainMenu();
            }, 10, TheClient.Window.Height - 100, 350, 70, TheClient.FontSets.SlightlyBigger));
        }

        public override void Tick()
        {
            Menus.TickAll();
        }

        public override void SwitchTo()
        {
            MouseHandler.ReleaseMouse();
        }

        public override void Render()
        {
            TheClient.Establish2D();
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0, 0.5f, 0.5f, 1 });
            GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1 });
            TheClient.FontSets.SlightlyBigger.DrawColoredText("^!^e^0  Voxalia\nSingleplayer",
                new Location(TheClient.Window.Width / 2 - TheClient.FontSets.SlightlyBigger.MeasureFancyText("Singleplayer") / 2, 0, 0));
            Menus.RenderAll(TheClient.gDelta);
        }
    }
}
