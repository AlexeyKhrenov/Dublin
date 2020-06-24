using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Dublin.ReaderWriterWorkers
{
    // todo - implement buffer so it's not collected
    public class ConcurrentQueue<T>
    {
        private readonly Queue<T> queue = new Queue<T>();
        private readonly int maxSize;
        private volatile bool closed;

        private int enqueueed;
        private int dequeued;

        public bool IsEmpty => queue.Count == 0;

        public ConcurrentQueue(int maxSize)
        {
            this.maxSize = maxSize;
        }

        public void Enqueue(T item)
        {
            lock (queue)
            {
                while (queue.Count > maxSize)
                {
                    Monitor.Wait(queue);
                }
                queue.Enqueue(item);
                enqueueed++;

                Monitor.PulseAll(queue);
            }
        }

        public bool TryDequeue(out T value)
        {
            lock (queue)
            {
                while (queue.Count == 0)
                {
                    if (closed)
                    {
                        value = default(T);
                        return false;
                    }
                    Monitor.Wait(queue);
                }

                value = queue.Dequeue();
                if (queue.Count == maxSize - 1)
                {
                    Monitor.PulseAll(queue);
                }

                dequeued++;
                
                return true;
            }
        }

        public void Close()
        {
            lock (queue)
            {
                closed = true;
                Monitor.PulseAll(queue);
            }
        }
    }
}
