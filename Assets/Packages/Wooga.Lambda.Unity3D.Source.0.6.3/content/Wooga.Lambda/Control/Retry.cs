using System;
using Wooga.Lambda.Control.Concurrent;
using Wooga.Lambda.Control.Monad;

namespace Wooga.Lambda.Control
{
    public static class Retry
    {
        private static readonly Func<Strategy, Strategy> NextRetry =
            s => new Strategy(Decrement(s.Retries), Increment(s.Attempts), s.DelayBase, s.delay, s.next);

        private static uint Decrement(this uint x)
        {
            return x == uint.MinValue ? x : x - 1;
        }

        private static uint Increment(this uint x)
        {
            return x == uint.MaxValue ? x : x + 1;
        }

        public static Strategy LimitRetries(this Strategy s, uint retries)
        {
            return new Strategy(Math.Min(retries, s.Retries), s.Attempts, s.DelayBase, s.delay, s.next);
        }

        public static Strategy DelayRetries(this Strategy s, uint delay)
        {
            return new Strategy(s.Retries, s.Attempts, delay, s.delay, s.next);
        }

        public static Strategy ExponentialBackoff(this Strategy s)
        {
            return new Strategy(s.Retries, s.Attempts, s.DelayBase, x => (x.Attempts*x.Attempts)*x.DelayBase, s.next);
        }

        public static Strategy Next(this Strategy s)
        {
            return s.next(NextRetry(s));
        }

        public static uint Delay(this Strategy s)
        {
            var d = s.delay(s);
            return d;
        }

        public struct Strategy
        {
            public static readonly Strategy Default = new Strategy(5, 0, 250, s => s.DelayBase, _ => _);
            public readonly uint Attempts;
            internal readonly Func<Strategy, uint> delay;
            public readonly uint DelayBase;
            internal readonly Func<Strategy, Strategy> next;
            public readonly uint Retries;

            public Strategy(uint retries, uint attempts, uint delayBase, Func<Strategy, uint> r,
                Func<Strategy, Strategy> n)
            {
                Retries = retries;
                Attempts = attempts;
                DelayBase = delayBase;
                delay = r;
                next = n;
            }
        }
    }

    public static class AsyncExtensions
    {
        public static Async<Either<T,Exception>> Retry<T>(this Async<T> m, Retry.Strategy s)
        {
            return DoRetry(m, new Retry.Strategy(0, 0, 0, _ => 0, _ => s));
        }

        private static Async<Either<T, Exception>> DoRetry<T>(Async<T> m, Retry.Strategy s)
        {
            return () => Async.Sleep((int) s.Delay())
                .Then(m)
                .Catch()
                .RunSynchronously()
                .From(
                    Either.Success<T, Exception>,
                    l =>
                    {
                        var next = s.Next();
                        return next.Retries > 0 
                            ? DoRetry(m, next)() 
                            : Either.Failure<T,Exception>(l);
                    });
        }
    }
}