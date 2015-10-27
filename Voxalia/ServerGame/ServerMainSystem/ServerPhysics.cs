using System;
using System.Collections.Generic;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.ServerMainSystem
{
    // TODO: Rename or scrap file?
    public partial class Server
    {
        public List<Region> LoadedRegions = new List<Region>();

        /// <summary>
        /// Fired when a region is going to be loaded; can be cancelled.
        /// For purely listening to a region load after the fact, use <see cref="OnRegionLoaded"/>.
        /// </summary>
        public EventHandler<RegionLoadingEventArgs> OnRegionLoading;

        /// <summary>
        /// Fired when a region has been loaded; is purely informative.
        /// For cancelling a region load, use <see cref="OnRegionLoading"/>.
        /// </summary>
        public EventHandler<RegionLoadedEventArgs> OnRegionLoaded;

        public void LoadRegion(string name)
        {
            if (OnRegionLoading != null)
            {
                RegionLoadingEventArgs e = new RegionLoadingEventArgs() { RegionName = name };
                OnRegionLoading(this, e);
                if (e.Cancelled)
                {
                    return;
                }
            }
            Region region = new Region();
            region.Name = name.ToLower();
            region.TheServer = this;
            region.BuildWorld();
            LoadedRegions.Add(region);
            if (OnRegionLoaded != null)
            {
                OnRegionLoaded(this, new RegionLoadedEventArgs() { TheRegion = region });
            }
        }

        /// <summary>
        /// Ticks the physics world.
        /// </summary>
        public void TickWorlds(double delta)
        {
            for (int i = 0; i < LoadedRegions.Count; i++)
            {
                LoadedRegions[i].Tick(delta);
            }
        }
    }

    public class RegionLoadingEventArgs : EventArgs
    {
        public bool Cancelled = false;

        public string RegionName = null;
    }

    public class RegionLoadedEventArgs : EventArgs
    {
        public Region TheRegion = null;
    }
}
