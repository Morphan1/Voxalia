//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Voxalia.Shared.LockedLinkedList;

namespace Voxalia.Shared
{
    public class Scheduler
    {
        public LockedLinkedList<SyncScheduleItem> Tasks = new LockedLinkedList<SyncScheduleItem>();
        
        public SyncScheduleItem GetSyncTask(Action act, double delay = 0)
        {
            return new SyncScheduleItem() { MyAction = act, Time = delay, OwningEngine = this };
        }

        public SyncScheduleItem ScheduleSyncTask(Action act, double delay = 0)
        {
            SyncScheduleItem item = new SyncScheduleItem() { MyAction = act, Time = delay, OwningEngine = this };
            Tasks.AddAtEnd(item);
            return item;
        }

        public void RunAllSyncTasks(double time)
        {
            LockedLinkedListNode<SyncScheduleItem> node = Tasks.First;
            while (node != null)
            {
                node.Data.Time -= time;
                if (node.Data.Time > 0)
                {
                    node = node.Next;
                    continue;
                }
                try
                {
                    node.Data.MyAction.Invoke();
                }
                catch (Exception ex)
                {
                    if (ex is ThreadAbortException)
                    {
                        throw ex;
                    }
                    SysConsole.Output("Handling sync task", ex);
                }
                LockedLinkedListNode<SyncScheduleItem> torem = node;
                node = node.Next;
                Tasks.Remove(torem);
            }
        }

        public ASyncScheduleItem StartASyncTask(Action a)
        {
            ASyncScheduleItem asyncer = new ASyncScheduleItem();
            asyncer.MyAction = a;
            asyncer.RunMe();
            return asyncer;
        }

        public ASyncScheduleItem AddASyncTask(Action a, ASyncScheduleItem followUp = null)
        {
            ASyncScheduleItem asyncer = new ASyncScheduleItem();
            asyncer.MyAction = a;
            asyncer.FollowUp = followUp;
            return asyncer;
        }
    }

    public abstract class ScheduleItem
    {
        public abstract void RunMe();

        public Scheduler OwningEngine;
    }

    public class SyncScheduleItem: ScheduleItem
    {
        public Action MyAction;

        public double Time = 0;

        public override void RunMe()
        {
            OwningEngine.ScheduleSyncTask(MyAction);
        }
    }

    public class ASyncScheduleItem : ScheduleItem
    {
        public Action MyAction;

        public ASyncScheduleItem FollowUp = null;

        Object Locker = new Object();

        public bool Started = false;

        bool Done = false;

        public bool HasStarted()
        {
            lock (Locker)
            {
                return Started;
            }
        }

        public bool IsDone()
        {
            lock (Locker)
            {
                return Done;
            }
        }

        public ASyncScheduleItem ReplaceOrFollowWith(ASyncScheduleItem item)
        {
            lock (Locker)
            {
                if (FollowUp != null)
                {
                    return FollowUp.ReplaceOrFollowWith(item);
                }
                if (Started)
                {
                    if (Done)
                    {
                        item.RunMe();
                        return item;
                    }
                    else
                    {
                        FollowUp = item;
                        return item;
                    }
                }
                else
                {
                    MyAction = item.MyAction;
                    FollowUp = item.FollowUp;
                    return this;
                }
            }
        }

        public void FollowWith(ASyncScheduleItem item)
        {
            lock (Locker)
            {
                if (Done)
                {
                    item.RunMe();
                }
                else
                {
                    FollowUp = item;
                }
            }
        }

        public override void RunMe()
        {
            lock (Locker)
            {
                if (Started && !Done)
                {
                    return;
                }
                Started = true;
                Done = false;
            }
            Task.Factory.StartNew(runInternal);
        }

        private void runInternal()
        {
            MyAction.Invoke();
            lock (Locker)
            {
                Done = true;
            }
            if (FollowUp != null)
            {
                FollowUp.RunMe();
            }
        }
    }
}
