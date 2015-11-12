using System;

namespace Voxalia.ClientGame.AudioSystem
{
    public class SoundEffect
    {
        public string Name;

        public int Internal;

        public EventHandler<EventArgs> Loaded;
    }
}
