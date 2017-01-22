using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxalia.Shared.LockedLinkedList
{
    public class LockedLinkedListNode<T>
    {
        public LockedLinkedListNode<T> Previous;

        public LockedLinkedListNode<T> Next;

        public T Data;
    }
}
