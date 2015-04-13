using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace ShadowOperations.ClientGame.ClientMainSystem
{
    public partial class Client
    {
        public double Delta;

        void Window_UpdateFrame(object sender, FrameEventArgs e)
        {
            Delta = e.Time;
            TickWorld(Delta);
        }
    }
}
