using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ClientGame.ClientMainSystem;

namespace ShadowOperations.ClientGame.GraphicsSystems.ParticleSystem
{
    public abstract class ParticleEffect
    {
        public Client TheClient;

        public ParticleEffect(Client tclient)
        {
            TheClient = tclient;
        }

        public abstract void Render();

        public abstract ParticleEffect Clone();
    }
}
