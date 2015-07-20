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
        public List<ScheduleItem> Tasks = new List<ScheduleItem>();

        Object Locker = new Object();

        public void ScheduleSyncTask(Action act)
        {
            lock (Locker)
            {
                Tasks.Add(new ScheduleItem() { MyAction = act, Time = 0 });
            }
        }

        public void ScheduleSyncTask(Action act, double delay)
        {
            lock (Locker)
            {
                Tasks.Add(new ScheduleItem() { MyAction = act, Time = delay });
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

    public class ScheduleItem
    {
        public Action MyAction;

        public double Time = 0;
    }

    public class ASyncScheduleItem
    {
        public Action MyAction;

        public ASyncScheduleItem FollowUp = null;

        Object Locker = new Object();

        bool Done = false;

        public bool IsDone()
        {
            lock (Locker)
            {
                return Done;
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

        public void RunMe()
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
