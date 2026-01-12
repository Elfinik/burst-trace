using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

namespace Elfinik.BurstTrace.Internal
{
    /// <summary>
    /// Allows the use of BurstTrace from a custom multithreading framework.
    /// </summary>
    public static class BurstTraceCustomThreads
    {
        /// <summary>
        /// Returns the maximum number of threads
        /// </summary>
        public static int GetMaxThreadsCount => JobsUtility.ThreadIndexCount;

        /// <summary>
        /// Captures the current stack frame and appends it to an existing log chain.
        /// This method is safe to call from the Main Thread, C# Threads, and Burst-compiled Jobs.
        /// ThreadIndex must be unique for each concurrent thread. Passing the same index from multiple threads simultaneously will cause memory corruption.
        /// </summary>
#if !BURSTTRACE_DISABLE
        /// <param name="threadIndex">The index of the thread or task.</param>
        /// <param name="prev">The previous stack trace handle to extend.</param>
        /// <param name="memberName">Automatically populated by the compiler. Do not pass a value manually.</param>
        /// <param name="sourceFilePath">Automatically populated by the compiler. Do not pass a value manually.</param>
        /// <param name="sourceLineNumber">Automatically populated by the compiler. Do not pass a value manually.</param>
        /// <returns>A new <see cref="TraceHandle"/> containing the updated chain.</returns>
#endif
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
#if BURSTTRACE_DISABLE
        public static TraceHandle Capture(int threadIndex, TraceHandle prev) => default;
#else
        public static TraceHandle Capture(int threadIndex, TraceHandle prev, [CallerMemberName] string memberName = "",
    [CallerFilePath] string sourceFilePath = "",
    [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (!BurstTrace.IsLogEnabled()) return default;
#if BURSTTRACE_CAPTURE_PROFILER
            BurstTraceInternal.MarkerRecordCaller.Begin();
#endif
            CheckThreadIndex(threadIndex);
            var detailedLog = new DetailedLog
            {
                path = sourceFilePath,
                member = memberName,
                line = sourceLineNumber,
            };
#if !BURSTTRACE_NOT_USE_64
            var hashedKey = HashedKey.CreateHash(in detailedLog);
#else
            FixedString512Bytes fs = detailedLog.ConvertToStringBursted();
#endif
#if BURSTTRACE_CAPTURE_PROFILER
            BurstTraceInternal.MarkerRecordCaller.End();
#endif
#if !BURSTTRACE_NOT_USE_64
            var res = BurstTraceInternal.ApplyLogCustomThread(threadIndex, ref BurstTraceSharedStatic.InfoField.Data, in hashedKey, in detailedLog);
#else
            var res = BurstTraceInternal.ApplyLogCustomThread(threadIndex,ref BurstTraceSharedStatic.InfoField.Data, fs);
#endif
            return BurstTraceInternal.CombineLogCustomThread(threadIndex, ref BurstTraceSharedStatic.InfoField.Data, prev, res);
        }
#endif

        /// <summary>
        /// Captures the current stack frame as the start of a new log chain.
        /// This method is safe to call from the Main Thread, C# Threads, and Burst-compiled Jobs.
        /// ThreadIndex must be unique for each concurrent thread. Passing the same index from multiple threads simultaneously will cause memory corruption.
        /// </summary>
#if !BURSTTRACE_DISABLE
        /// <param name="threadIndex">The index of the thread or task.</param>
        /// <param name="memberName">Automatically populated by the compiler. Do not pass a value manually.</param>
        /// <param name="sourceFilePath">Automatically populated by the compiler. Do not pass a value manually.</param>
        /// <param name="sourceLineNumber">Automatically populated by the compiler. Do not pass a value manually.</param>
#endif
        /// <returns>A new <see cref="TraceHandle"/> representing the current frame.</returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
#if BURSTTRACE_DISABLE
        public static TraceHandle Capture(int threadIndex) => default;
#else
        public static TraceHandle Capture(int threadIndex, [CallerMemberName] string memberName = "",
     [CallerFilePath] string sourceFilePath = "",
     [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (!BurstTrace.IsLogEnabled()) return default;
#if BURSTTRACE_CAPTURE_PROFILER
            BurstTraceInternal.MarkerRecordCaller.Begin();
#endif
            CheckThreadIndex(threadIndex);
            var detailedLog = new DetailedLog
            {
                path = sourceFilePath,
                member = memberName,
                line = sourceLineNumber,
            };
#if !BURSTTRACE_NOT_USE_64
            var hashedKey = HashedKey.CreateHash(in detailedLog);
#else
            FixedString512Bytes fs = detailedLog.ConvertToStringBursted();
#endif
#if BURSTTRACE_CAPTURE_PROFILER
            BurstTraceInternal.MarkerRecordCaller.End();
#endif
#if !BURSTTRACE_NOT_USE_64
            var res = BurstTraceInternal.ApplyLogCustomThread(threadIndex, ref BurstTraceSharedStatic.InfoField.Data, in hashedKey, in detailedLog);
#else
            var res = BurstTraceInternal.ApplyLogCustomThread(threadIndex,ref BurstTraceSharedStatic.InfoField.Data, fs);
#endif
            return res;
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckThreadIndex(int threadIndex)
        {
            if (threadIndex < 0 || threadIndex >= GetMaxThreadsCount)
                throw new System.IndexOutOfRangeException($"ThreadIndex is not valid! It must be between zero and 'BurstTraceCustomThreads.GetMaxThreadsCount'!");
        }
    }
}