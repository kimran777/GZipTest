using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GZipTest.Threading
{
    class ThreadManager
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

        public static void StartThreadsWithIndex(List<Thread> threads)
        {
            int index = 0;
            foreach(var thread in threads)
            {
                thread.Start(index);
                index++;
            }
        }

        public static void StartThreads(List<Thread> threads)
        {
            foreach (var thread in threads)
            {
                thread.Start();
            }
        }

        public static void WaitThreads(List<Thread> threads)
        {
            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

    }
}
