using System;
using System.Collections.Generic;
using Voxalia.ServerGame.WorldSystem;
using FreneticScript;

namespace Voxalia.ServerGame.ServerMainSystem
{
    // TODO: Rename or scrap file?
    public partial class Server
    {
        // TODO: Dictionary?
        public List<Region> LoadedRegions = new List<Region>();

        /// <summary>
        /// Fired when a region is going to be loaded; can be cancelled.
        /// For purely listening to a region load after the fact, use <see cref="OnRegionLoadPostEvent"/>.
        /// TODO: Move to an event helper!
        /// </summary>
        public FreneticScriptEventHandler<RegionLoadPreEventArgs> OnRegionLoadPreEvent = new FreneticScriptEventHandler<RegionLoadPreEventArgs>();

        /// <summary>
        /// Fired when a region is loaded and is going to be added; can be cancelled.
        /// For purely listening to a region load after the fact, use <see cref="OnRegionLoadPostEvent"/>.
        /// TODO: Move to an event helper!
        /// </summary>
        public FreneticScriptEventHandler<RegionLoadEventArgs> OnRegionLoadEvent = new FreneticScriptEventHandler<RegionLoadEventArgs>();

        /// <summary>
        /// Fired when a region has been loaded; is purely informative.
        /// For cancelling a region load, use <see cref="OnRegionLoadPreEvent"/>.
        /// TODO: Move to an event helper!
        /// </summary>
        public FreneticScriptEventHandler<RegionLoadPostEventArgs> OnRegionLoadPostEvent = new FreneticScriptEventHandler<RegionLoadPostEventArgs>();

        public Region LoadRegion(string name)
        {
            string nl = name.ToLowerFast();
            for (int i = 0; i < LoadedRegions.Count; i++)
            {
                if (LoadedRegions[i].Name == nl)
                {
                    return LoadedRegions[i];
                }
            }
            RegionLoadPreEventArgs e = new RegionLoadPreEventArgs() { RegionName = name };
            OnRegionLoadPreEvent.Fire(e);
            if (e.Cancelled)
            {
                return null;
            }
            Region region = new Region();
            region.Name = nl;
            region.TheServer = this;
            region.BuildWorld();
            RegionLoadEventArgs e2 = new RegionLoadEventArgs() { TheRegion = region };
            OnRegionLoadEvent.Fire(e2);
            if (e.Cancelled)
            {
                region.UnloadFully();
                return null;
            }
            LoadedRegions.Add(region);
            OnRegionLoadPostEvent.Fire(new RegionLoadPostEventArgs() { TheRegion = region });
            return region;
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

        long cID = 1; // TODO: Save/load value!

        Object CIDLock = new Object();

        public long AdvanceCID()
        {
            lock (CIDLock)
            {
                return cID++;
            }
        }

        long CloudID = 1;

        Object CloudIDLock = new Object();

        public long AdvanceCloudID()
        {
            lock (CloudIDLock)
            {
                return CloudID++;
            }
        }
    }

    public class RegionLoadEventArgs : EventArgs
    {
        public bool Cancelled = false;

        public Region TheRegion = null;
    }

    public class RegionLoadPreEventArgs : EventArgs
    {
        public bool Cancelled = false;

        public string RegionName = null;
    }

    public class RegionLoadPostEventArgs : EventArgs
    {
        public Region TheRegion = null;
    }
}
