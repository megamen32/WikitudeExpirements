using System;
using Wooga.Lambda.Control.Concurrent;
using Wooga.Lambda.Data;

namespace Wooga.Lambda.Log
{
    /// <summary>
    /// An agent for logging with customizable handlers
    /// </summary>
    public class LogAgent
    {
        private readonly Agent<LogMsg, Unit> _agent ;

        public LogAgent(Func<LogMsg, Unit>[] handlers)
        {
            _agent = Agent<LogMsg, Unit>.Start(Unit.Default, (inbox, u) =>
            {
                var msg = inbox.Receive().RunSynchronously();

                foreach (var handler in handlers)
                    handler(msg);

                return Unit.Default;
            });
            SharedAgent = this;
        }

        public static LogAgent SharedAgent { get; private set; }

        private Unit PostLog(LogMsg msg)
        {
            _agent.Post(msg);
            return Unit.Default;
        }

        public Unit Debug(String msg)
        {
            PostLog(new LogMsg.Debug {Message = msg});
            return Unit.Default;
        }

        public Unit Info(String msg)
        {
            PostLog(new LogMsg.Info {Message = msg});
            return Unit.Default;
        }

        public Unit Warn(String msg)
        {
            PostLog(new LogMsg.Warn {Message = msg});
            return Unit.Default;
        }

        public Unit Error(String msg)
        {
            PostLog(new LogMsg.Error {Message = msg});
            return Unit.Default;
        }

        public Unit Fatal(String msg)
        {
            PostLog(new LogMsg.Fatal {Message = msg});
            return Unit.Default;
        }

        public abstract class LogMsg
        {
            public String Message { get; internal set; }

            public override string ToString()
            {
                return Message;
            }

            internal sealed class Debug : LogMsg
            {
            };

            internal sealed class Info : LogMsg
            {
            };

            internal sealed class Warn : LogMsg
            {
            };

            internal sealed class Error : LogMsg
            {
            };

            internal sealed class Fatal : LogMsg
            {
            };
        };
    }
}