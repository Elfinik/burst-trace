using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;
using UnityEngine;

namespace Elfinik.BurstTrace.Internal
{
    internal unsafe struct ArrayOfAllocs<T> : IDisposable where T : unmanaged, IDisposable
    {
        public bool IsCreated => array.IsCreated;
        //[NativeDisableParallelForRestriction] //for IJobForeach index check skip
        private NativeArray<IntPtr> array;
        public int Length => array.Length;

        public ArrayOfAllocs(int length, Allocator allocator)
        {
            array = new NativeArray<IntPtr>(length, allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(int index)
        {
            var ptr = array[index];
            Assert.IsTrue(ptr != IntPtr.Zero);
            UnsafeUtility.CopyPtrToStructure<T>(ptr.ToPointer(), out var res);
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref T GetAsRef(int index)
        {
            var res = array[index];
            Assert.IsTrue(res != IntPtr.Zero);
            return ref UnsafeUtility.AsRef<T>(res.ToPointer());
        }

        public unsafe ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var res = array[index];
                Assert.IsTrue(res != IntPtr.Zero);
                return ref UnsafeUtility.AsRef<T>(res.ToPointer());
            }
        }

        public void Allocate(int index, ref T data)
        {
            Assert.IsTrue(array[index] == IntPtr.Zero, $"Is not null: {array[index].GetHashCode()}");
            array[index] = (IntPtr)CollectionsMemory.Allocate<T>(ref data);
        }
        public void Dispose()
        {
            foreach (var item in array)
            {
                //Assert.IsTrue(item != IntPtr.Zero);
                if (item == IntPtr.Zero)
                {
                    Debug.LogError($"BurstTrace: container already disposed!");
                    continue;
                }
                T* r = (T*)item.ToPointer();
                r->Dispose();
                CollectionsMemory.Release(r);
            }
            array.Dispose();
        }
    }
}