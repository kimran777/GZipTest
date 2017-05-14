using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GZipLib.Threading
{
    class DictionaryWithLock<T> where T : class
    {

        private readonly object _lockPoint = new object();
        private Dictionary<long, T> _dictionary = new Dictionary<long, T>();
        private bool _isStopped = false;
        private bool _isAbort = false;

        public long MaxSize
        {
            get;
            private set;
        }


        public DictionaryWithLock(long maxSize)
        {
            MaxSize = maxSize;
        }

        public void Add(long key, T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            lock (_lockPoint)
            {
                if (_isStopped)
                {
                    throw new InvalidOperationException("ProduceConsume is stopped");
                }

                while (_dictionary.Count >= MaxSize)
                {
                    Monitor.Wait(_lockPoint);
                }

                //queue.Add(task);
                _dictionary[key] = value;
                Monitor.Pulse(_lockPoint);
            }
        }

        public T GetValue(long key)
        {
            lock (_lockPoint)
            {
                while ((_dictionary.Count == 0 || !_dictionary.ContainsKey(key)) && !_isStopped && !_isAbort)
                {
                    Monitor.Wait(_lockPoint);
                }
                
                if (_dictionary.Count == 0 || _isAbort)
                {
                    return null;
                }


                T value = _dictionary[key];
                _dictionary.Remove(key);

                Monitor.Pulse(_lockPoint);

                return value;
            }
        }

        public void Stop()
        {
            lock (_lockPoint)
            {
                if(!_isStopped)
                {
                    _isStopped = true;
                    Monitor.PulseAll(_lockPoint);
                }
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
