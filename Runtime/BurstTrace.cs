using Elfinik.BurstTrace.Internal;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;

namespace Elfinik.BurstTrace
{
    /// <summary>
    /// Provides functionality for capturing stack traces within Burst-compiled jobs and regular C# code.
    /// </summary>
    public static class BurstTrace
    {
        /// <summary>
        /// Turn off (or turn on) logging.
        /// </summary>
        /// <param name="disabled"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLogDisabled(bool disabled = false)
        {
            BurstTraceSharedStaticDisabled.SetDisabled(disabled);
        }

        /// <summary>
        /// Returns true if logging is enabled, otherwise false. If logging is disabled in this build, it also returns false.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if BURSTTRACE_DISABLE
        public static bool IsLogEnabled() => false;
#else
        public static bool IsLogEnabled() => !BurstTraceSharedStaticDisabled.IsDisabled;
#endif
    }


    public partial struct TraceHandle
    {
        /// <summary>
        /// Captures the current stack frame and appends it to an existing log chain.
        /// This method is safe to call from the Main Thread, C# Threads, and Burst-compiled Jobs.
        /// </summary>
#if !BURSTTRACE_DISABLE
        /// <param name="prev">The previous stack trace handle to extend.</param>
        /// <param name="memberName">Automatically populated by the compiler. Do not pass a value manually.</param>
        /// <param name="sourceFilePath">Automatically populated by the compiler. Do not pass a value manually.</param>
        /// <param name="sourceLineNumber">Automatically populated by the compiler. Do not pass a value manually.</param>
        /// <returns>A new <see cref="TraceHandle"/> containing the updated chain.</returns>
#endif
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
#if BURSTTRACE_DISABLE
        public static TraceHandle Capture(TraceHandle prev) => default;
#else
        public static TraceHandle Capture(TraceHandle prev, [CallerMemberName] string memberName = "",
    [CallerFilePath] string sourceFilePath = "",
    [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (!BurstTrace.IsLogEnabled()) return default;
#if BURSTTRACE_CAPTURE_PROFILER
            BurstTraceInternal.MarkerRecordCaller.Begin();
#endif
            var detailedLog = new DetailedLog
            {
#if BURSTTRACE_OPTIMIZE_MEMORY
                path = BurstTraceInternal.Optimize(sourceFilePath),
#else
                path = sourceFilePath,
#endif
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
            var res = BurstTraceInternal.ApplyLog(ref BurstTraceSharedStatic.InfoField.Data, in hashedKey, in detailedLog);
#else
            var res = BurstTraceInternal.ApplyLog(ref BurstTraceSharedStatic.InfoField.Data, fs);
#endif
            return BurstTraceInternal.CombineLog(ref BurstTraceSharedStatic.InfoField.Data, prev, res);
        }
#endif

        /// <summary>
        /// Captures the current stack frame as the start of a new log chain.
        /// This method is safe to call from the Main Thread, C# Threads, and Burst-compiled Jobs.
        /// </summary>
#if !BURSTTRACE_DISABLE
        /// <param name="memberName">Automatically populated by the compiler. Do not pass a value manually.</param>
        /// <param name="sourceFilePath">Automatically populated by the compiler. Do not pass a value manually.</param>
        /// <param name="sourceLineNumber">Automatically populated by the compiler. Do not pass a value manually.</param>
#endif
        /// <returns>A new <see cref="TraceHandle"/> representing the current frame.</returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
#if BURSTTRACE_DISABLE
        public static TraceHandle Capture() => default;
#else
        public static TraceHandle Capture([CallerMemberName] string memberName = "",
     [CallerFilePath] string sourceFilePath = "",
     [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (!BurstTrace.IsLogEnabled()) return default;
#if BURSTTRACE_CAPTURE_PROFILER
            BurstTraceInternal.MarkerRecordCaller.Begin();
#endif
            var detailedLog = new DetailedLog
            {
#if BURSTTRACE_OPTIMIZE_MEMORY
                path = BurstTraceInternal.Optimize(sourceFilePath),
#else
                path = sourceFilePath,
#endif
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
            var res = BurstTraceInternal.ApplyLog(ref BurstTraceSharedStatic.InfoField.Data, in hashedKey, in detailedLog);
#else
            var res = BurstTraceInternal.ApplyLog(ref BurstTraceSharedStatic.InfoField.Data, fs);
#endif
            return res;
        }
#endif
    }

    /// <summary>
    /// Contains complete information about a single log call line. Compatible with Burst.
    /// </summary>
    public struct DetailedLog
    {
        /// <summary>
        /// Path to the file (CS script)
        /// </summary>
#if BURSTTRACE_OPTIMIZE_MEMORY
        public FixedString128Bytes path;
#else
        public FixedString512Bytes path;
#endif
        /// <summary>
        /// The method that requested the log
        /// </summary>
        public FixedString64Bytes member;
        /// <summary>
        /// line number in the file (C# script)
        /// </summary>
        public int line;

        public string ConvertToString()
        {
#if BURSTTRACE_OPTIMIZE_MEMORY
            return $"{member} (at {BurstTraceInternal.TryRestorePath(path)}:{line})";
#else
            return $"{member} (at {path}:{line})";
#endif
        }

        public FixedString512Bytes ConvertToStringBursted()
        {
#if BURSTTRACE_OPTIMIZE_MEMORY
            return $"{member} (at {BurstTraceInternal.TryRestorePath(path)}:{line})";
#else
            return $"{member} (at {path}:{line})";
#endif
        }
    }
}
