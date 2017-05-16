using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Threading
{
    public class QueueWithLock<T> where T : class
    {
        private readonly object _lockPoint = new object();
        private Queue<T> _queue = new Queue<T>();
        private bool _isStopped = false;
        private bool _isAbort = false;

        public long MaxSize
        {
            get;
            private set;
        }


        public QueueWithLock(long maxSize)
        {
            MaxSize = maxSize;
        }

        public void Enqueue(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            lock (_lockPoint)
            {
                if (_isStopped)
                {
                    throw new InvalidOperationException("Queue is stopped");
                }
                if (_isAbort)
                {
                    throw new InvalidOperationException("Queue is aborted");
                }

                while (_queue.Count >= MaxSize)
                {
                    Monitor.Wait(_lockPoint);

                    if (_isStopped)
                    {
                        throw new InvalidOperationException("Queue is stopped");
                    }
                    if (_isAbort)
                    {
                        throw new InvalidOperationException("Queue is aborted");
                    }
                }

                //queue.Add(task);
                _queue.Enqueue(value);
                Monitor.Pulse(_lockPoint);
            }
        }

        public T Dequeue()
        {
            lock (_lockPoint)
            {
                while (_queue.Count == 0 && !_isStopped && !_isAbort)
                {
                    Monitor.Wait(_lockPoint);
                }

                if (_queue.Count == 0 || _isAbort)
                {
                    return null;
                }


                T value = _queue.Dequeue();

                Monitor.Pulse(_lockPoint);

                return value;
            }
        }


        public long Count()
        {
            lock (_lockPoint)
            {
                return _queue.Count;
            }
        }
        public void Stop()
        {
            lock (_lockPoint)
            {
                _isStopped = true;
                Monitor.PulseAll(_lockPoint);
            }
        }

        public void Abort()
        {
            lock (_lockPoint)
            {
                _isAbort = true;
                Monitor.PulseAll(_lockPoint);
            }
        }

    }

}
