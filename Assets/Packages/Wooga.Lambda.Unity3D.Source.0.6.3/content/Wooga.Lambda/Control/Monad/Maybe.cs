using System;

namespace Wooga.Lambda.Control.Monad
{
    /// <summary>
    /// The Maybe type encapsulates an optional value. A value of type Maybe 'T either contains a value of type 'T, or it is empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Maybe<T>
    {
        internal readonly T Value;
        internal readonly bool HasValue;

        internal Maybe(T value, bool hasValue)
        {
            Value = value;
            HasValue = hasValue;
        }
    }

    public static class Maybe
    {
        // Monad functions

        /// <summary>
        /// When x is not null returns Maybe.Just with x, otherwise Maybe.Nothing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Maybe<T> Return<T>(T x)
        {
            return x == null ? new Maybe<T>(default(T), false) : new Maybe<T>(x, true);
        }

        /// <summary>
        /// When m is Maybe.Just it applies f to the value, otherwise Maybe.Nothing 
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="m"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Maybe<TOutput> Bind<TInput,TOutput>(this Maybe<TInput> m, Func<TInput, Maybe<TOutput>> f)
        {
            return m.HasValue ? f(m.Value) : Maybe.Nothing<TOutput>();
        }
        
        /// <summary>
        /// When m is Maybe.Just it returns n, otherwise Maybe.Nothing
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static Maybe<TOutput> Then<TInput,TOutput>(this Maybe<TInput> m, Maybe<TOutput> n)
        {
            return m.HasValue ? n : Maybe.Nothing<TOutput>();
        }

        // Functor functions

        /// <summary>
        /// When m is Maybe.Just it applies f to the value, otherwise Maybe.Nothing
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="m"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Maybe<TOutput> Map<TInput,TOutput>(this Maybe<TInput>m, Func<TInput, TOutput> f)
        {
            return m.HasValue ? Maybe.Just(f(m.Value)) : Maybe.Nothing<TOutput>();
        }

        // Maybe functions

        /// <summary>
        /// Constructs Maybe.Just with v
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Maybe<T> Just<T>(T v)
        {
            return new Maybe<T>(v,true);
        }

        /// <summary>
        /// Constructs Maybe.Nothing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Maybe<T> Nothing<T>()
        {
            return new Maybe<T>(default(T),false);
        }

        /// <summary>
        /// When m is Maybe.Just returns value, otherwise dflt
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="m"></param>
        /// <param name="dflt"></param>
        /// <returns></returns>
        public static T ValueOr<T>(this Maybe<T> m, T dflt)
        {
            return m.HasValue ? m.Value : dflt;
        }

        /// <summary>
        /// When m is Maybe.Just returns value, otherwise result of dflt
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="m"></param>
        /// <param name="dflt"></param>
        /// <returns></returns>
        public static T ValueOr<T>(this Maybe<T> m, Func<T> dflt)
        {
            return m.HasValue ? m.Value : dflt();
        }
        
        /// <summary>
        /// True when m is Maybe.Just
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="m"></param>
        /// <returns></returns>
        public static bool IsJust<T>(this Maybe<T> m)
        {
            return m.HasValue;
        }

        /// <summary>
        /// True when m is Maybe.Nothing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="m"></param>
        /// <returns></returns>
        public static bool IsNothing<T>(this Maybe<T> m)
        {
            return !m.IsJust();
        }
    }
}