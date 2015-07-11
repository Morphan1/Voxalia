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

        public void AddSyncTask(Task t)
        {
            lock (Locker)
            {
                Tasks.Add(new ScheduleItem() { task = t, time = 0 });
            }
        }

        public void AddSyncTask(Task t, double delay)
        {
            lock (Locker)
            {
                Tasks.Add(new ScheduleItem() { task = t, time = delay });
            }
        }

        public void RunAllSyncTasks(double time)
        {
            lock (Locker)
            {
                for (int i = 0; i < Tasks.Count; i++)
                {
                    Tasks[i].time -= time;
                    if (Tasks[i].time > 0)
                    {
                        continue;
                    }
                    if (Tasks[i].task.IsCompleted || Tasks[i].task.IsCanceled)
                    {
                        Tasks.RemoveAt(i--);
                        continue;
                    }
                    Tasks[i].task.RunSynchronously();
                    Tasks.RemoveAt(i--);
                }
            }
        }
    }

    public class ScheduleItem
    {
        public Task task;

        public double time = 0;
    }
}
