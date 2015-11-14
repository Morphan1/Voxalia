using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Voxalia.Shared
{
    public class Scheduler
    {
        public List<SyncScheduleItem> Tasks = new List<SyncScheduleItem>();

        Object Locker = new Object();

        public SyncScheduleItem GetSyncTask(Action act, double delay = 0)
        {
            return new SyncScheduleItem() { MyAction = act, Time = delay, OwningEngine = this };
        }

        public void DescheduleSyncTask(SyncScheduleItem item)
        {
            lock (Locker)
            {
                Tasks.Remove(item);
            }
        }

        public SyncScheduleItem ScheduleSyncTask(Action act, double delay = 0)
        {
            SyncScheduleItem item = new SyncScheduleItem() { MyAction = act, Time = delay, OwningEngine = this };
            lock (Locker)
            {
                Tasks.Add(item);
            }
            return item;
        }

        public void RunAllSyncTasks(double time)
        {
            lock (Locker)
            {
                for (int i = 0; i < Tasks.Count; i++) // NOTE: *MUST* calculate in this order!
                {
                    Tasks[i].Time -= time;
                    if (Tasks[i].Time > 0)
                    {
                        continue;
                    }
                    Tasks[i].MyAction.Invoke();
                    Tasks.RemoveAt(i--);
                }
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

        public ScheduleItem FollowUp = null;

        Object Locker = new Object();

        bool Started = false;

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

        public void FollowWith(ScheduleItem item)
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
