using System;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.ClientGame.UISystem;
using Voxalia.ClientGame.UISystem.MenuSystem;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.Shared;

namespace Voxalia.ClientGame.ClientMainSystem
{
    public class ChunkWaitingScreen: Screen
    {
        public UIMenu Menus;
        
        public LoadAllChunksSystem LACS = null;

        public Object WaitLock = new Object();

        public int ChunksStillWaiting = 0;

        public override void Init()
        {
            Menus = new UIMenu(TheClient);
            Menus.Add(new UIMenuButton("ui/menus/buttons/basic", "Cancel", () =>
            {
                UIConsole.WriteLine("Cancel!"); // TODO
            }, 10, 300, 350, 70, TheClient.FontSets.SlightlyBigger));
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
            Menus.RenderAll(TheClient.gDelta);
            if (LACS == null)
            {
                lock (TheClient.TheRegion.LoadedChunks)
                {
                    TheClient.FontSets.SlightlyBigger.DrawColoredText("^!^e^0Chunks Loaded: " + TheClient.TheRegion.LoadedChunks.Count
                        + "\nChunks that still need parsing: " + ChunksStillWaiting, new Location(20, 20, 0));
                }
            }
            else
            {
                lock (LACS.Locker)
                {
                    TheClient.FontSets.SlightlyBigger.DrawColoredText("^!^e^0Chunks Loaded: " + TheClient.TheRegion.LoadedChunks.Count
                        + "\nChunks Registered: " + LACS.Count
                        + "\nChunks Rendered: " + LACS.rC + ": " + ((LACS.rC / (float)LACS.Count) * 100f) + "%", new Location(20, 20, 0));
                }
            }
        }
    }
}
