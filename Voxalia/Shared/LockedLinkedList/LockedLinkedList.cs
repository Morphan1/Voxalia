using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Voxalia.Shared.LockedLinkedList
{
    /// <summary>
    /// This class isn't actually entirely locked/thread-safe, or even entirely functional as a linked-list, only done sufficiently for its first immediate use-case.
    /// Use with caution. Probably don't use beyond as a starting point reference really.
    /// </summary>
    public class LockedLinkedList<T>
    {
        public LockedLinkedListNode<T> First;

        public LockedLinkedListNode<T> Last;

        private Object lastLock = new Object();

        /// <summary>
        /// Do NOT input null. There is no safety check, and that will produce unexpected results!
        /// </summary>
        public void Remove(LockedLinkedListNode<T> itm)
        {
            // If the item or its follower is the last one, lock. Otherwise, execute as normal.
            // TODO: Evaluate this better. Is this actually guaranteed to not bork out on too much sudden usage?
            bool t = false;
            try
            {
                if (itm == Last || (itm.Next != null && itm.Next == Last))
                {
                    Monitor.Enter(lastLock, ref t);
                }
                if (itm.Previous != null)
                {
                    itm.Previous.Next = itm.Next;
                }
                if (itm.Next != null)
                {
                    itm.Next.Previous = itm.Previous;
                }
                if (itm == First)
                {
                    First = itm.Next;
                }
                if (itm == Last)
                {
                    Last = itm.Previous;
                }
            }
            finally
            {
                if (t)
                {
                    Monitor.Exit(lastLock);
                }
            }
        }

        public void AddAtEnd(T data)
        {
            // This always modifies the last entry, so lock on the last entry.
            lock (lastLock)
            {
                LockedLinkedListNode<T> temp = new LockedLinkedListNode<T>();
                temp.Data = data;
                temp.Previous = Last;
                if (Last != null)
                {
                    Last.Next = temp;
                }
                Last = temp;
                if (First == null)
                {
                    First = temp;
                }
            }
        }

        public void Clear()
        {
            // This always modifies the last entry, so lock on the last entry.
            lock (lastLock)
            {
                First = null;
                Last = null;
            }
        }
    }
}
