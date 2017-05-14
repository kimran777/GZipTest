using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Threading
{
    public static class ThreadsExtension
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
        

        public static IEnumerable<WaitThreadResult> WaitThreads(this IEnumerable<Thread> threads)
        {
            List<WaitThreadResult> waitThreadResult = new List<WaitThreadResult>();
            foreach (var thread in threads)
            {
                thread.Join();
                waitThreadResult.Add(new WaitThreadResult(thread.ThreadState));
            }
            
            return waitThreadResult;
        }

        public static void ContinueWith(this IEnumerable<WaitThreadResult> source, Action<WaitThreadResult> action)
        {
            foreach(var item in source)
            {
                action.Invoke(item);
            }
        }

        public static void ContinueWithOneTime(this IEnumerable<WaitThreadResult> source, Action action)
        {
            action.Invoke();
        }

    }
}
