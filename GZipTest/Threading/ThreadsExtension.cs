using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GZipTest.Threading
{
    static class ThreadsExtension
    {
        public static void StartThreadsWithIndex(this IEnumerable<Thread> threads)
        {
            int index = 0;
            foreach (var thread in threads)
            {
                thread.Start(index);
                index++;
            }
        }

        public static void StartThreads(this IEnumerable<Thread> threads)
        {
            foreach (var thread in threads)
            {
                thread.Start();
            }
        }

        public static WaitThreadResult WaitThreads(this IEnumerable<Thread> threads)
        {
            foreach (var thread in threads)
            {
                thread.Join();
            }

            return new WaitThreadResult();
        }

        public static void ContinueWith(this WaitThreadResult waitResult, Action actionToRun)
        {
            actionToRun.Invoke();
        }

    }
}
