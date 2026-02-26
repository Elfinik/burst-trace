using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Unity.Collections;

namespace Unity.Collections.BurstTrace
{
    public unsafe static class BurstTrace_CollectionsMemory
    {
        public static void StartCaptureLeaks()
        {
#if UNITY_2022_3_OR_NEWER
            if (UnsafeUtility.GetLeakDetectionMode() == NativeLeakDetectionMode.Disabled)
                UnsafeUtility.SetLeakDetectionMode(NativeLeakDetectionMode.Enabled);
#else
            Debug.LogError($"Please, install 2022.3+ for support Unit Tests!");
#endif
        }
        public static void StartCaptureLeaksFullStackTrace()
        {
#if UNITY_2022_3_OR_NEWER
            UnsafeUtility.SetLeakDetectionMode(NativeLeakDetectionMode.EnabledWithStackTrace);
#else
            Debug.LogError($"Please, install 2022.3+ for support Unit Tests!");
#endif
        }
        public static int CheckLeaksNow()
        {
#if UNITY_2022_3_OR_NEWER
            var res = UnsafeUtility.CheckForLeaks();
            return res;
#else
            Debug.LogError($"Please, install 2022.3+ for support Unit Tests!");
            return 0;
#endif
        }
        public static int ForgiveLeaksNow()
        {
#if UNITY_2022_3_OR_NEWER
            return UnsafeUtility.ForgiveLeaks();
#else
            Debug.LogError($"Please, install 2022.3+ for support Unit Tests!");
            return 0;
#endif
        }
        public static T* Allocate<T>(ref T initialValue) where T : unmanaged
        {
            T* m_EntityDataAccess = (T*)Memory.Unmanaged.Allocate(UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
            UnsafeUtility.MemClear(m_EntityDataAccess, sizeof(T));
            UnsafeUtility.CopyStructureToPtr(ref initialValue, m_EntityDataAccess);
            return m_EntityDataAccess;
        }

        public static void Release<T>(T* pointer) where T : unmanaged
        {
            Memory.Unmanaged.Free<T>(pointer, Allocator.Persistent);
            pointer = null;
        }
    }
}
