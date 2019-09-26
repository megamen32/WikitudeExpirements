using System;
using JetBrains.Annotations;
using Wooga.Lambda.Control;
using Wooga.Lambda.Control.Concurrent;
using Wooga.Lambda.Control.Monad;
using Wooga.Lambda.Data;
using static Wooga.Lambda.Control.Monad.Maybe;
using static Wooga.Lambda.Logging.Log.Msg;
using Handlers = Wooga.Lambda.Data.ImmutableList<Wooga.Lambda.Logging.Log.Handler>;

namespace Wooga.Lambda.Logging
{
    /// <summary>
    ///     An agent for logging with customizable handlers
    /// </summary>
    public class Log
    {
        public delegate Unit Handler(Msg x);

        public enum Level
        {
            Debug,
            Info,
            Warn,
            Error,
            Fatal
        }   

    [NotNull]    public static readonly Log Shared = new Log();
    [NotNull]    public static readonly Log LOG = Shared;
        private readonly Agent<AgentMsg, Unit> _agent;

        private Log()
        {
            _agent = Agent<AgentMsg, Unit>.Start(new Handlers(),
                (inbox, handlers) =>
                {
                    var msg = inbox.Receive().RunSynchronously();
                    return  Pattern<Handlers>
                            .Match(msg)
                            .Case<AddHandlerMsg>(h => handlers.Add(h.Handler))
                            .Case<Msg>(l =>
                            {
                                foreach (var handler in handlers)
                                {
                                    handler(l);
                                }
                                return handlers;
                            })
                            .Default(handlers)
                            .Run();
                });
        }

        private Unit PostLog(AgentMsg agentMsg)
        {
            return _agent.Post(agentMsg);
        }

        public Unit Debug(string msg)
        {
            return PostLog(With(msg,Level.Debug));
        }

        public Unit Debug(string msg, Object context)
        {
            return PostLog(With(msg, Level.Debug, context));
        }

        public Unit Info(string msg)
        {
            return PostLog(With(msg, Level.Info));
        }

        public Unit Info(string msg, Object context)
        {
            return PostLog(With(msg, Level.Info, context));
        }

        public Unit Warn(string msg)
        {
            return PostLog(With(msg, Level.Warn));
        }

        public Unit Warn(string msg, Object context)
        {
            return PostLog(With(msg, Level.Warn, context));
        }

        public Unit Error(string msg)
        {
            return PostLog(With(msg, Level.Error));
        }

        public Unit Error(string msg, Object context)
        {
            return PostLog(With(msg, Level.Error, context));
        }

        public Unit Fatal(string msg)
        {
            return PostLog(With(msg, Level.Fatal));
        }

        public Unit Fatal(string msg, Object context)
        {
            return PostLog(With(msg, Level.Fatal, context));
        }

        public Unit AddHandler(Handler handler)
        {
            return PostLog(new AddHandlerMsg {Handler = handler});
        }

        public struct Msg : AgentMsg
        {
            public Level Level { get; internal set; }
            public string Message { get; internal set; }
            public Maybe<object> Context { get; internal set; }

            public static Msg With(string message, Level level)
            {
                return new Msg {Message = message, Level = level, Context = Nothing<Object>()};
            }

            public static Msg With(string message, Level level, Object context)
            {
                return new Msg { Message = message, Level = level, Context = Just(context) };
            }
        }

        internal interface AgentMsg
        {
        }

        internal struct AddHandlerMsg : AgentMsg
        {
            public Handler Handler { get; internal set; }
        }
    }
}
