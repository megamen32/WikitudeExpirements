using System;
using System.Threading;
using Wooga.Lambda.Data;

namespace Wooga.Lambda.Control.Concurrent
{
    public sealed class ThreadPoolComputationQueue : AsyncComputationQueue
    {
        public Unit Enqueue(Async<Unit> a)
        {
            ThreadPool.QueueUserWorkItem(_=> {
                    try
                    {
                        a.RunSynchronously();
                    }catch(Exception e)
                    {
                        Async.DispatchException(e);
                    }
                
                });
            return Unit.Default;
        }
    }
}
