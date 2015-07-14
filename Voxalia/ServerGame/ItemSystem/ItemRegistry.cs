using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.ItemSystem.CommonItems;
using Voxalia.Shared;

namespace Voxalia.ServerGame.ItemSystem
{
    public class ItemRegistry
    {
        public Dictionary<string, BaseItemInfo> Infos;

        public BaseItemInfo Generic;

        public ItemRegistry()
        {
            Infos = new Dictionary<string, BaseItemInfo>();
            Register(new BowItem());
            Register(new FlashLightItem());
            Register(new HookItem());
            Register(new MinigunGunItem());
            Register(new OpenHandItem());
            Register(new PistolGunItem());
            Register(new RifleGunItem());
            Register(new ShotgunGunItem());
            Register(new BulletItem());
            Register(new FistItem());
            Register(new BlockItem());
            Register(new SunAnglerItem());
            Generic = new GenericItem();
            Register(Generic);
        }

        public void Register(BaseItemInfo info)
        {
            Infos.Add(info.Name.ToLower(), info);
        }

        public BaseItemInfo GetInfoFor(string name)
        {
            BaseItemInfo bii;
            if (Infos.TryGetValue(name.ToLower(), out bii))
            {
                return bii;
            }
            SysConsole.Output(OutputType.WARNING, "Using generic item for " + name);
            return Generic;
        }
    }
}
