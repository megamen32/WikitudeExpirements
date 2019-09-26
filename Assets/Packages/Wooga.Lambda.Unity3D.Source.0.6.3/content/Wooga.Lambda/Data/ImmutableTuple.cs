using System.Collections.Generic;

namespace Wooga.Lambda.Data
{
    public static class ImmutableTuple
    {
        public static ImmutableTuple<T1, T2> Tuple<T1, T2>(T1 x, T2 y)
        {
            return new ImmutableTuple<T1, T2>(x, y);
        }

        public static ImmutableTuple<T1, T2, T3> Tuple<T1, T2, T3>(T1 x, T2 y, T3 z)
        {
            return new ImmutableTuple<T1, T2, T3>(x, y, z);
        }

        public static ImmutableTuple<T1, T2, T3, T4> Tuple<T1, T2, T3, T4>(T1 x, T2 y, T3 z, T4 w)
        {
            return new ImmutableTuple<T1, T2, T3, T4>(x, y, z, w);
        }
    }

    /// <summary>
    ///     Represent a value pair.
    /// </summary>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    public struct ImmutableTuple<T1, T2>
    {
        private static readonly IEqualityComparer<ImmutableTuple<T1, T2>> Item1Item2ComparerInstance =
            new Item1Item2EqualityComparer();

        public readonly T1 Item1;
        public readonly T2 Item2;

        public ImmutableTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public static IEqualityComparer<ImmutableTuple<T1, T2>> Item1Item2Comparer
        {
            get { return Item1Item2ComparerInstance; }
        }

        public bool Equals(ImmutableTuple<T1, T2> other)
        {
            return EqualityComparer<T1>.Default.Equals(Item1, other.Item1) &&
                   EqualityComparer<T2>.Default.Equals(Item2, other.Item2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ImmutableTuple<T1, T2> && Equals((ImmutableTuple<T1, T2>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T1>.Default.GetHashCode(Item1)*397) ^
                       EqualityComparer<T2>.Default.GetHashCode(Item2);
            }
        }

        private sealed class Item1Item2EqualityComparer : IEqualityComparer<ImmutableTuple<T1, T2>>
        {
            public bool Equals(ImmutableTuple<T1, T2> x, ImmutableTuple<T1, T2> y)
            {
                return EqualityComparer<T1>.Default.Equals(x.Item1, y.Item1) &&
                       EqualityComparer<T2>.Default.Equals(x.Item2, y.Item2);
            }

            public int GetHashCode(ImmutableTuple<T1, T2> obj)
            {
                unchecked
                {
                    return (EqualityComparer<T1>.Default.GetHashCode(obj.Item1)*397) ^
                           EqualityComparer<T2>.Default.GetHashCode(obj.Item2);
                }
            }
        }
    }

    /// <summary>
    ///     Represent a value triplet.
    /// </summary>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <typeparam name="T3">The type of the third value.</typeparam>
    public struct ImmutableTuple<T1, T2, T3>
    {
        private static readonly IEqualityComparer<ImmutableTuple<T1, T2, T3>> Item1Item2Item3ComparerInstance =
            new Item1Item2Item3EqualityComparer();

        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;

        public ImmutableTuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }

        public static IEqualityComparer<ImmutableTuple<T1, T2, T3>> Item1Item2Item3Comparer
        {
            get { return Item1Item2Item3ComparerInstance; }
        }

        public bool Equals(ImmutableTuple<T1, T2, T3> other)
        {
            return EqualityComparer<T1>.Default.Equals(Item1, other.Item1) &&
                   EqualityComparer<T2>.Default.Equals(Item2, other.Item2) &&
                   EqualityComparer<T3>.Default.Equals(Item3, other.Item3);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ImmutableTuple<T1, T2, T3> && Equals((ImmutableTuple<T1, T2, T3>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EqualityComparer<T1>.Default.GetHashCode(Item1);
                hashCode = (hashCode*397) ^ EqualityComparer<T2>.Default.GetHashCode(Item2);
                hashCode = (hashCode*397) ^ EqualityComparer<T3>.Default.GetHashCode(Item3);
                return hashCode;
            }
        }

        private sealed class Item1Item2Item3EqualityComparer : IEqualityComparer<ImmutableTuple<T1, T2, T3>>
        {
            public bool Equals(ImmutableTuple<T1, T2, T3> x, ImmutableTuple<T1, T2, T3> y)
            {
                return EqualityComparer<T1>.Default.Equals(x.Item1, y.Item1) &&
                       EqualityComparer<T2>.Default.Equals(x.Item2, y.Item2) &&
                       EqualityComparer<T3>.Default.Equals(x.Item3, y.Item3);
            }

            public int GetHashCode(ImmutableTuple<T1, T2, T3> obj)
            {
                unchecked
                {
                    var hashCode = EqualityComparer<T1>.Default.GetHashCode(obj.Item1);
                    hashCode = (hashCode*397) ^ EqualityComparer<T2>.Default.GetHashCode(obj.Item2);
                    hashCode = (hashCode*397) ^ EqualityComparer<T3>.Default.GetHashCode(obj.Item3);
                    return hashCode;
                }
            }
        }
    }

    /// <summary>
    ///     Represent a value triplet.
    /// </summary>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <typeparam name="T3">The type of the third value.</typeparam>
    /// <typeparam name="T4">The type of the third value.</typeparam>
    public struct ImmutableTuple<T1, T2, T3, T4>
    {
        private static readonly IEqualityComparer<ImmutableTuple<T1, T2, T3, T4>> ImmutableTupleComparerInstance =
            new ImmutableTupleEqualityComparer();

        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;

        public ImmutableTuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        }

        public static IEqualityComparer<ImmutableTuple<T1, T2, T3, T4>> ImmutableTupleComparer
        {
            get { return ImmutableTupleComparerInstance; }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ImmutableTuple<T1, T2, T3, T4> && Equals((ImmutableTuple<T1, T2, T3, T4>) obj);
        }

        public bool Equals(ImmutableTuple<T1, T2, T3, T4> other)
        {
            return EqualityComparer<T1>.Default.Equals(Item1, other.Item1) &&
                   EqualityComparer<T2>.Default.Equals(Item2, other.Item2) &&
                   EqualityComparer<T3>.Default.Equals(Item3, other.Item3) &&
                   EqualityComparer<T4>.Default.Equals(Item4, other.Item4);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EqualityComparer<T1>.Default.GetHashCode(Item1);
                hashCode = (hashCode*397) ^ EqualityComparer<T2>.Default.GetHashCode(Item2);
                hashCode = (hashCode*397) ^ EqualityComparer<T3>.Default.GetHashCode(Item3);
                hashCode = (hashCode*397) ^ EqualityComparer<T4>.Default.GetHashCode(Item4);
                return hashCode;
            }
        }

        private sealed class ImmutableTupleEqualityComparer : IEqualityComparer<ImmutableTuple<T1, T2, T3, T4>>
        {
            public bool Equals(ImmutableTuple<T1, T2, T3, T4> x, ImmutableTuple<T1, T2, T3, T4> y)
            {
                return EqualityComparer<T1>.Default.Equals(x.Item1, y.Item1) &&
                       EqualityComparer<T2>.Default.Equals(x.Item2, y.Item2) &&
                       EqualityComparer<T3>.Default.Equals(x.Item3, y.Item3) &&
                       EqualityComparer<T4>.Default.Equals(x.Item4, y.Item4);
            }

            public int GetHashCode(ImmutableTuple<T1, T2, T3, T4> obj)
            {
                unchecked
                {
                    var hashCode = EqualityComparer<T1>.Default.GetHashCode(obj.Item1);
                    hashCode = (hashCode*397) ^ EqualityComparer<T2>.Default.GetHashCode(obj.Item2);
                    hashCode = (hashCode*397) ^ EqualityComparer<T3>.Default.GetHashCode(obj.Item3);
                    hashCode = (hashCode*397) ^ EqualityComparer<T4>.Default.GetHashCode(obj.Item4);
                    return hashCode;
                }
            }
        }
    }
}