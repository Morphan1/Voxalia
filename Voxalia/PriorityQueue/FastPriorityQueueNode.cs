// mcmonkey: Got this off https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp

namespace Priority_Queue
{
    public class FastPriorityQueueNode
    {
        /// <summary>
        /// The Priority to insert this node at.  Must be set BEFORE adding a node to the queue
        /// </summary>
        public double Priority;

        /// <summary>
        /// <b>Used by the priority queue - do not edit this value.</b>
        /// Represents the order the node was inserted in
        /// </summary>
        public long InsertionIndex;

        /// <summary>
        /// <b>Used by the priority queue - do not edit this value.</b>
        /// Represents the current position in the queue
        /// </summary>
        public int QueueIndex;
    }
}
