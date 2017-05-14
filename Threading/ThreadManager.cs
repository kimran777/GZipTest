using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Threading
{
    public class ThreadManager
    {
        public static List<Thread> GetThreads(int numOfThread, ThreadStart start)
        {
            List<Thread> result = new List<Thread>();
            for(int i = 0; i < numOfThread; i++)
            {
                result.Add(new Thread(start));
            }
            return result;
        }
        public static IEnumerable<Thread> GetSafeThreads(int numOfThread, ThreadStart start, Action<Exception> handler)
        {
            List<Thread> result = new List<Thread>();
            for (int i = 0; i < numOfThread; i++)
            {
                result.Add(new Thread(() =>
                {
                    SafeExecute(() => start(), handler);
                }));
            }
            return result;
        }

        public static Thread GetSafeThread(ThreadStart start, Action<Exception> handler)
        {
            return new Thread(() =>
            {
                SafeExecute(() => start(), handler);
            });
        }


        public static List<Thread> GetThreads(int numOfThread, ParameterizedThreadStart start)
        {
            List<Thread> result = new List<Thread>();
            for (int i = 0; i < numOfThread; i++)
            {
                result.Add(new Thread(start));
            }
            return result;

        }

        public static List<Thread> GetThreads(int numOfThread, ThreadStart start, int maxStackSize)
        {
            List<Thread> result = new List<Thread>();
            for (int i = 0; i < numOfThread; i++)
            {
                result.Add(new Thread(start, maxStackSize));
            }
            return result;
        }

        public static List<Thread> GetThreads(int numOfThread, ParameterizedThreadStart start, int maxStackSize)
        {
            List<Thread> result = new List<Thread>();
            for (int i = 0; i < numOfThread; i++)
            {
                result.Add(new Thread(start, maxStackSize));
            }
            return result;
        }


        private static void SafeExecute(ThreadStart start, Action<Exception> handler)
        {
            try
            {
                start.Invoke();
            }
            catch (Exception ex)
            {
                handler(ex);
            }
        }
    }
}
