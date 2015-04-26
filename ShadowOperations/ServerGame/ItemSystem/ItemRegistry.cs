using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.ItemSystem.CommonItems;

namespace ShadowOperations.ServerGame.ItemSystem
{
    public class ItemRegistry
    {
        public Dictionary<string, BaseItemInfo> Infos;

        public BaseItemInfo Generic;

        public ItemRegistry()
        {
            Infos = new Dictionary<string, BaseItemInfo>();
            Register(new PistolGunItem());
            Register(new OpenHandItem());
            Generic = new GenericItem();
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
            return Generic;
        }
    }
}
