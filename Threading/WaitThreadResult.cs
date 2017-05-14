using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Threading
{
    public class WaitThreadResult
    {
        public WaitThreadResult(ThreadState threadState)
        {
            ThreadState = threadState;
        }

        public Thread Thread
        {
            get;
            private set;
        }
        public ThreadState ThreadState
        {
            get;
            private set;
        }

    }
}
