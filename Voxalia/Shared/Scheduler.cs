using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        public void ScheduleSyncTask(Action act, double delay = 0)
        {
            lock (Locker)
            {
                Tasks.Add(new SyncScheduleItem() { MyAction = act, Time = 0, OwningEngine = this });
            }
        }

        public void RunAllSyncTasks(double time)
        {
            lock (Locker)
            {
                for (int i = 0; i < Tasks.Count; i++)
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

        bool Done = false;

        public bool IsDone()
        {
            lock (Locker)
            {
                return Done;
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
                Done = false;
            }
            Task.Factory.StartNew(() => runInternal());
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
