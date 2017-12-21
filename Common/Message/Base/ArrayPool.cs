using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Base
{
    public static class ArrayPool<T>
    {
        static readonly Stack<T[]>[] Pool = new Stack<T[]>[31];
        static readonly Dictionary<int, Stack<T[]>> ExactPool = new Dictionary<int, Stack<T[]>>();
        static readonly HashSet<T[]> InPool = new HashSet<T[]>();
        
        public static T[] Claim(int minimumLength)
        {
            if (minimumLength <= 0)
            {
                return ClaimWithExactLength(0);
            }

            if (!IsPowerOfTwo(minimumLength))
                minimumLength++;

            var bucketIndex = 0;
            while ((1 << bucketIndex) < minimumLength && bucketIndex < 30)
            {
                bucketIndex++;
            }

            if (bucketIndex == 30)
                throw new ArgumentException("Too high minimum length");

            lock (Pool)
            {
                if (Pool[bucketIndex] == null)
                {
                    Pool[bucketIndex] = new Stack<T[]>();
                }

                if (Pool[bucketIndex].Count > 0)
                {
                    var array = Pool[bucketIndex].Pop();
                    InPool.Remove(array);
                    return array;
                }
            }
            return new T[1 << bucketIndex];
        }

        public static T[] ClaimWithExactLength(int length)
        {
            bool isPowerOfTwo = length != 0 && (length & (length - 1)) == 0;

            if (isPowerOfTwo)
            {
                // Will return the correct array length
                return Claim(length);
            }

            lock (Pool)
            {
                Stack<T[]> stack;
                if (!ExactPool.TryGetValue(length, out stack))
                {
                    stack = new Stack<T[]>();
                    ExactPool[length] = stack;
                }

                if (stack.Count > 0)
                {
                    var array = stack.Pop();
                    InPool.Remove(array);
                    return array;
                }
            }
            return new T[length];
        }

        /** Pool an array.
		 * If the array was got using the #ClaimWithExactLength method then the \a allowNonPowerOfTwo parameter must be set to true.
		 * The parameter exists to make sure that non power of two arrays are not pooled unintentionally which could lead to memory leaks.
		 */
        public static void Release(ref T[] array, bool allowNonPowerOfTwo = false)
        {
            if (array == null) return;

            if (array.GetType() != typeof(T[]))
            {
                throw new ArgumentException("Expected array type " + typeof(T[]).Name + " but found " + array.GetType().Name + "\nAre you using the correct generic class?\n");
            }

            bool isPowerOfTwo = LengthIsPowerOfTwo(array);
            if (!isPowerOfTwo && !allowNonPowerOfTwo && array.Length != 0)
                return;

            lock (Pool)
            {
                if (!InPool.Add(array))
                {
                    throw new InvalidOperationException("You are trying to pool an array twice. Please make sure that you only pool it once.");
                }
                if (isPowerOfTwo)
                {
                    int bucketIndex = 0;
                    while ((1 << bucketIndex) < array.Length && bucketIndex < 30)
                    {
                        bucketIndex++;
                    }

                    if (Pool[bucketIndex] == null)
                    {
                        Pool[bucketIndex] = new Stack<T[]>();
                    }

                    Pool[bucketIndex].Push(array);
                }
                else
                {
                    Stack<T[]> stack;
                    if (!ExactPool.TryGetValue(array.Length, out stack))
                    {
                        stack = new Stack<T[]>();
                        ExactPool[array.Length] = stack;
                    }

                    stack.Push(array);
                }
            }
            array = null;
        }

        private static bool LengthIsPowerOfTwo(T[] array)
        {
            return IsPowerOfTwo(array.Length);
        }

        private static bool IsPowerOfTwo(int val)
        {
            return val != 0 && (val & (val - 1)) == 0;
        }
    }

    public static class ArrayPoolExtensions
    {
        public static T[] ToArrayFromPool<T>(this List<T> list)
        {
            var arr = ArrayPool<T>.ClaimWithExactLength(list.Count);

            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = list[i];
            }
            return arr;
        }
    }
}
