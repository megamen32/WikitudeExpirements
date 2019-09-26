using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Wooga.Lambda.Control.Monad;
using Wooga.Lambda.Data;

namespace Wooga.Lambda.Control.Concurrent
{
    public static class ListExtensions
    {
        public static TE Dequeue<TE>(this List<TE> l)
        {
            if (l.Count == 0)
            {
                throw new InvalidOperationException("trying to dequeue from an empty List");
            }
            TE r = l[0];
            l.RemoveAt(0);
            return r;
        }

        public static Maybe<TE> PopElement<TE>(this List<TE> l, Func<TE, Boolean> p)
        {
            if (l.Count > 0 && p(l[l.Count - 1]))
            {
                var i = l.Count - 1;
                TE r = l[i];
                l.RemoveAt(i);
                return Maybe.Just<TE>(r);
            }
            return Maybe.Nothing<TE>();
        }

        public static Maybe<TE> DequeueFirst<TE>(this List<TE> l,Func<TE, Boolean> p)
        {
            for (int i = 0; i < l.Count; i++)
            {
                if (p(l[i]))
                {
                    TE r = l[i];
                    l.RemoveAt(i);
                    return Maybe.Just<TE>(r);
                }    
            }
            return Maybe.Nothing<TE>();
        }

        public static Unit Enqueue<TE>(this List<TE> l, TE e)
        {
            l.Add(e);
            return Unit.Default;
        }
    }
    

    /// <summary>
    ///     Actor/Agent implementation similar to Control.Async.MailboxProcessor in F#
    /// </summary>
    /// <typeparam name="TMessage">The type of the message consumed by the agent.</typeparam>
    /// <typeparam name="TReply">The type of the response produced by the agent.</typeparam>
    public class Agent<TMessage, TReply>
    {
        private readonly List<TMessage> _inbox = new List<TMessage>();
        private volatile bool _shouldCancel;
        private readonly Object _receiveLock = new Object();
        /// <summary>
        ///     Gets a value indicating whether this agent is running.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this agent is running; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunning { get; private set; }

        private static Async<Unit> Watchdog<TS>(Agent<TMessage, TReply> inbox, Func<Agent<TMessage, TReply>, TS, TS> body, TS state)
        {
            return () =>
            {
                var mstate = state;
                inbox.IsRunning = true;
                while (!inbox.CancellationRequested())
                {
                    mstate = body(inbox, mstate);
                }
                inbox.IsRunning = false;
                return Unit.Default;
            };
        }

        /// <summary>
        ///     Creates and starts an agent.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <param name="state">The initial state.</param>
        /// <param name="body">The body to be executed.</param>
        /// <returns>The agent.</returns>
        public static Agent<TMessage, TReply> Start<TState>(TState state, Func<Agent<TMessage, TReply>, TState, TState> body)
        {
            var agent = new Agent<TMessage, TReply>();
            new Thread(_ =>
            {
                try
                {
                    Watchdog(agent, body, state).RunSynchronously();
                }
                catch (Exception e)
                {
                    Async.DispatchException(e);
                }
                
            }).Start();
            return agent;
        }

        /// <summary>
        ///     Creates and starts an agent.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <param name="state">The initial state.</param>
        /// <param name="body">The body to be executed.</param>
        /// <returns>An async computation producing & starting the agent.</returns>
        public static Async<Agent<TMessage, TReply>> StartAsync<TState>(TState state, Func<Agent<TMessage, TReply>, TState, TState> body)
        {
            return () =>
            {
                var agent = new Agent<TMessage, TReply>();
                Watchdog(agent, body, state).RunSynchronously();
                return agent;
            };
        }

        /// <summary>
        ///     Indicates if the agent should cancel and stop
        /// </summary>
        public bool CancellationRequested()
        {
            return _shouldCancel;
        }

        /// <summary>
        ///     Cancels this agent.
        /// </summary>
        public void Cancel()
        {
            CancelAsync().RunSynchronously();
        }

        /// <summary>
        ///     Cancels this agent.
        /// </summary>
        /// <returns>Async computation that cancels the agent.</returns>
        public Async<Unit> CancelAsync()
        {
            return () =>
            {
                _shouldCancel = true;
                while (IsRunning)
                {
                }
                return Unit.Default;
            };
        }

        

        /// <summary>
        ///     Posts a message to the message queue of the Agent, asynchronously.
        /// </summary>
        /// <param name="msg">The message to post.</param>
        public Unit Post(TMessage msg)
        {
            new Thread(() =>
            {
                lock (_inbox)
                {
                    try
                    {
                        _inbox.Enqueue(msg);
                        Monitor.Pulse(_inbox);
                    }
                    catch (Exception e)
                    {
                        Async.DispatchException(e);
                    }
                    
                }
            })
            .Start();
            return Unit.Default;
        }

        /// <summary>
        ///     Posts a message to an agent and await a reply on the channel, synchronously.
        /// </summary>
        /// <param name="f">The lambda providing the message.</param>
        /// <returns>The agents reply</returns>
        public TReply PostAndReply(Func<AsyncReplyChannel<TReply>, TMessage> f)
        {
            var handle = new AsyncEventHandle<TReply>();
            Post(f(new AsyncReplyChannel<TReply>(handle.Complete)));
            handle.DoneEvent.WaitOne();
            return handle.Result();
        }

        /// <summary>
        ///     Posts a message to an agent and await a reply on the channel, asynchronously.
        /// </summary>
        /// <param name="f">The lambda providing the message.</param>
        /// <returns>An async computation providing the agents reply.</returns>
        public Async<TReply> PostAndAsyncReply(Func<AsyncReplyChannel<TReply>, TMessage> f)
        {
            return () => PostAndReply(f);
        }

        /// <summary>
        ///     Waits for a message. This will consume the first message in arrival order.
        /// </summary>
        /// <returns>An async computation providing a message.</returns>
        public Async<TMessage> Receive()
        {
            return () =>
            {
                lock (_receiveLock)
                {
                    lock (_inbox)
                    {
                        while (_inbox.Count == 0)
                        {
                            Monitor.Wait(_inbox);
                        }
                        return _inbox.Dequeue();
                    }
                }
            };
        }

        public Async<TMessage> Scan(Func<TMessage, bool> f)
        {
            return () =>
            {
                lock (_receiveLock)
                {
                    lock (_inbox)
                    {
                        Maybe<TMessage> msg = _inbox.DequeueFirst(f); 
                        while (msg.IsNothing())
                        {
                            Monitor.Wait(_inbox);
                            msg = _inbox.PopElement(f); 
                        }
                        return msg.ValueOr(() => { throw new Exception("shouldn't be nothing"); });
                    }
                }
                
            };
        }
    }
}