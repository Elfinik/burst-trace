using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
#if BURSTTRACE_CAPTURE_PROFILER
using Unity.Profiling;
#endif
using UnityEngine;

namespace Elfinik.BurstTrace.Internal
{
    /// <summary>
    /// Use this class to register logs manually.
    /// </summary>
    public static class BurstTraceAdvanced
    {
        /// <summary>
        /// Captures the current stack frame and appends it to an existing log chain.
        /// This method is safe to call from the Main Thread, C# Threads, and Burst-compiled Jobs.
        /// </summary>
        /// <param name="prev">The previous stack trace handle to extend.</param>
        /// <param name="memberName">[CallerMemberName] string memberName = ""</param>
        /// <param name="sourceFilePath">[CallerFilePath] string sourceFilePath = ""</param>
        /// <param name="sourceLineNumber">[CallerLineNumber] int sourceLineNumber = 0</param>
        /// <returns></returns>
        public static TraceHandle RegisterLog(TraceHandle prev, in FixedString64Bytes memberName, in FixedString512Bytes sourceFilePath, int sourceLineNumber)
        {
            if (!BurstTrace.IsLogEnabled()) return default;
#if BURSTTRACE_CAPTURE_PROFILER
            BurstTraceInternal.MarkerRecordCaller.Begin();
#endif
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
            var res = BurstTraceInternal.ApplyLog(ref BurstTraceSharedStatic.InfoField.Data, in hashedKey, in detailedLog);
#else
            var res = BurstTraceInternal.ApplyLog(ref BurstTraceSharedStatic.InfoField.Data, fs);
#endif
            return BurstTraceInternal.CombineLog(ref BurstTraceSharedStatic.InfoField.Data, prev, res);
        }

        /// <summary>
        /// Captures the current stack frame as the start of a new log chain.
        /// This method is safe to call from the Main Thread, C# Threads, and Burst-compiled Jobs.
        /// </summary>
        /// <param name="memberName">[CallerMemberName] string memberName = ""</param>
        /// <param name="sourceFilePath">[CallerFilePath] string sourceFilePath = ""</param>
        /// <param name="sourceLineNumber">[CallerLineNumber] int sourceLineNumber = 0</param>
        /// <returns></returns>
        public static TraceHandle RegisterLog(in FixedString64Bytes memberName, in FixedString512Bytes sourceFilePath, int sourceLineNumber)
        {
            if (!BurstTrace.IsLogEnabled()) return default;
#if BURSTTRACE_CAPTURE_PROFILER
            BurstTraceInternal.MarkerRecordCaller.Begin();
#endif
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
            var res = BurstTraceInternal.ApplyLog(ref BurstTraceSharedStatic.InfoField.Data, in hashedKey, in detailedLog);
#else
            var res = BurstTraceInternal.ApplyLog(ref BurstTraceSharedStatic.InfoField.Data, fs);
#endif
            return res;
        }

        /// <summary>
        /// Returns the total amount of memory (in bytes) allocated for logging.
        /// </summary>
        [BurstDiscard]
        public static int GetTotalAllocatedMemory()
        {
#if BURSTTRACE_DISABLE
            return 0;
#else
            return BurstTraceSharedStatic.InfoField.Data.CalculateTotalMemoryAllocated();
#endif
        }

        /// <summary>
        /// Returns the amount of memory (in bytes) that is already filled with logs.
        /// </summary>
        [BurstDiscard]
        public static int GetUsedMemory()
        {
#if BURSTTRACE_DISABLE
            return 0;
#else
            return BurstTraceSharedStatic.InfoField.Data.CalculateTotalMemoryUsed();
#endif
        }
    }



    internal static class BurstTraceInternal
    {
        public const int MAX_STACK_DEPTH = 64;
#if BURSTTRACE_CAPTURE_PROFILER
        public readonly static ProfilerMarker MarkerRecordCaller = new ProfilerMarker("CreatedLog");
        public readonly static ProfilerMarker MarkerSaveLog = new ProfilerMarker("SaveLog");
        public readonly static ProfilerMarker MarkerNestedSaveLog = new ProfilerMarker("SaveNestedLog");
#endif


        internal static string GetManagedLog(TraceHandle log)
        {
            if (!log.IsValid) return TraceHandle.INVALID_EMPTY_LOG_VALUE;
            if (Application.isPlaying)
            {
                ref var map = ref BurstTraceSharedStatic.InfoField.Data;
                if (!map.rows.IsCreated)
                    return $"Log Container not created! RawValue: {log}";
                if (log.IsCombined)
                {
                    string resString = "";
                    int antistack = MAX_STACK_DEPTH;
                    while (log.IsCombined && --antistack > 0)
                    {
                        if (map.nestedRows.Length <= log.ThreadIndex)
                            return $"Invalid log. Perhaps it was recorded during a previous launch?";
                        var threadRow = map.nestedRows[log.ThreadIndex];
                        if (threadRow.Length <= log.RowIndex)
                            return $"Invalid log. Perhaps it was recorded during a previous launch?";
                        var res = threadRow[log.RowIndex];
                        log = new TraceHandle { Value = res.x };
                        {
                            var prevLog = new TraceHandle { Value = res.y };
                            if (map.rows.Length <= prevLog.ThreadIndex)
                                return $"Invalid log. Perhaps it was recorded during a previous launch?";
                            var _threadRow = map.rows[prevLog.ThreadIndex];
                            if (_threadRow.Length <= prevLog.RowIndex)
                                return $"Invalid log. Perhaps it was recorded during a previous launch?";
                            var _res = _threadRow[prevLog.RowIndex];
                            resString += _res.ConvertToString() + "\r\n";
                        }
                    }
                    if (log.IsCombined)
                    {
                        return resString + $"stack overflow {log}";
                    }
                    else if (log.IsValid)
                    {
                        return resString + GetManagedLog(log);
                    }
                    else
                    {
                        return resString;
                    }
                }
                else
                {
                    var threadRow = map.rows[log.ThreadIndex];
                    var res = threadRow[log.RowIndex];
                    return res.ConvertToString();
                }
            }
            else
            {
                if (log.IsCombined)
                {
                    string resString = "";
                    int antistack = MAX_STACK_DEPTH;
                    while (log.IsCombined && --antistack > 0)
                    {
                        if (!BurstTraceDictionary.SerializedMap.Instance.nestedValues.TryGetValue(log.Value, out var __res))
                        {
                            resString += "Unknown (nested)\r\n";
                            break;
                        }
                        log = new TraceHandle { Value = __res.x };
                        {
                            if (!BurstTraceDictionary.SerializedMap.Instance.values.TryGetValue(__res.y, out var _res))
                            {
                                _res = "Unknown\r\n";
                            }
                            resString += _res + "\r\n";
                        }
                    }
                    if (log.IsCombined)
                        return resString + $"stack overflow {log}";
                    else if (log.IsValid)
                        return resString + GetManagedLog(log);
                    else
                        return resString;
                }
                else
                {
                    if (BurstTraceDictionary.SerializedMap.Instance.values.TryGetValue(log.Value, out var res))
                    {
                        return res;
                    }
                    else
                    {
                        return $"Unknown log [{log}]";
                    }
                }
            }
        }

#if !BURSTTRACE_NOT_USE_64
        internal static TraceHandle ApplyLog(
           ref BurstTraceDictionary input, in HashedKey hashedKey,
           in DetailedLog log)
        {
#if BURSTTRACE_CAPTURE_PROFILER
            MarkerSaveLog.Begin();
#endif
            if (!input.rows.IsCreated)
            {
                return default;
#if BURSTTRACE_CAPTURE_PROFILER
            MarkerSaveLog.End();
#endif
            }
            var threadIndex = input.GetThreadIndex;
            var map = input.hashedMap.Get(threadIndex);

            if (map.TryGetValue(hashedKey, out var index))
            {
#if BURSTTRACE_CAPTURE_PROFILER
                MarkerSaveLog.End();
#endif
                return new TraceHandle(index, threadIndex);
            }
            var rows = input.rows.Get(threadIndex);
            map.Add(hashedKey, rows.Length);
            rows.Add(log);
#if BURSTTRACE_CAPTURE_PROFILER
            MarkerSaveLog.End();
#endif
            return new TraceHandle(rows.Length - 1, threadIndex);
        }
#else

    internal static TraceHandle ApplyLog(
        ref BurstTraceDictionary input,
        in FixedString512Bytes log)
    {
#if BURSTTRACE_CAPTURE_PROFILER
        MarkerSaveLog.Begin();
#endif
            if (!input.rows.IsCreated)
            {
                return default;
#if BURSTTRACE_CAPTURE_PROFILER
            MarkerSaveLog.End();
#endif
            }
        var threadIndex = input.GetThreadIndex;
        var map = input.map.Get(threadIndex);


        if (map.TryGetValue(log, out var index))
        {
#if BURSTTRACE_CAPTURE_PROFILER
            MarkerSaveLog.End();
#endif
            return new TraceHandle(index, threadIndex);
        }
        var rows = input.rows.Get(threadIndex);
        map.Add(log, rows.Length);
        rows.Add(log);
#if BURSTTRACE_CAPTURE_PROFILER
        MarkerSaveLog.End();
#endif
        return new TraceHandle(rows.Length - 1, threadIndex);
    }
#endif


#if !BURSTTRACE_NOT_USE_64
        internal static TraceHandle ApplyLogCustomThread(int customThread,
           ref BurstTraceDictionary input, in HashedKey hashedKey,
           in DetailedLog log)
        {
#if BURSTTRACE_CAPTURE_PROFILER
            MarkerSaveLog.Begin();
#endif
            if (!input.rows.IsCreated)
            {
                return default;
#if BURSTTRACE_CAPTURE_PROFILER
            MarkerSaveLog.End();
#endif
            }
            var threadIndex = customThread;
            var map = input.hashedMap.Get(threadIndex);


            if (map.TryGetValue(hashedKey, out var index))
            {
#if BURSTTRACE_CAPTURE_PROFILER
                MarkerSaveLog.End();
#endif
                return new TraceHandle(index, threadIndex);
            }
            var rows = input.rows.Get(threadIndex);
            map.Add(hashedKey, rows.Length);
            rows.Add(log);
#if BURSTTRACE_CAPTURE_PROFILER
            MarkerSaveLog.End();
#endif
            return new TraceHandle(rows.Length - 1, threadIndex);
        }
#else

    internal static TraceHandle ApplyLogCustomThread(int customThread,
        ref BurstTraceDictionary input,
        in FixedString512Bytes log)
    {
#if BURSTTRACE_CAPTURE_PROFILER
        MarkerSaveLog.Begin();
#endif
            if (!input.rows.IsCreated)
            {
                return default;
#if BURSTTRACE_CAPTURE_PROFILER
            MarkerSaveLog.End();
#endif
            }
        var threadIndex = customThread;
        var map = input.map.Get(threadIndex);


        if (map.TryGetValue(log, out var index))
        {
#if BURSTTRACE_CAPTURE_PROFILER
            MarkerSaveLog.End();
#endif
            return new TraceHandle(index, threadIndex);
        }
        var rows = input.rows.Get(threadIndex);
        map.Add(log, rows.Length);
        rows.Add(log);
#if BURSTTRACE_CAPTURE_PROFILER
        MarkerSaveLog.End();
#endif
        return new TraceHandle(rows.Length - 1, threadIndex);
    }
#endif



        internal static TraceHandle CombineLog(ref BurstTraceDictionary input, TraceHandle preview, TraceHandle r)
        {
            if (r.IsCombined)
            {
                Debug.LogError($"Exception! First log is Nested! Is not allowed!");
                return r;
            }
#if BURSTTRACE_CAPTURE_PROFILER
            MarkerNestedSaveLog.Begin();
#endif
            if (!input.rows.IsCreated)
            {
                return default;
#if BURSTTRACE_CAPTURE_PROFILER
            MarkerNestedSaveLog.End();
#endif
            }
            var key = new uint2(preview.Value, r.Value);

            var threadIndex = input.GetThreadIndex;
            var map = input.nestedMap.Get(threadIndex);
            var rows = input.nestedRows.Get(threadIndex);


            if (map.TryGetValue(key, out var index))
            {
#if BURSTTRACE_CAPTURE_PROFILER
                MarkerNestedSaveLog.End();
#endif
                return new TraceHandle(index, threadIndex, true);
            }
            map.Add(key, rows.Length);
            rows.Add(key);
#if BURSTTRACE_CAPTURE_PROFILER
            MarkerNestedSaveLog.End();
#endif
            return new TraceHandle(rows.Length - 1, threadIndex, true);
        }


        internal static TraceHandle CombineLogCustomThread(int customThread, ref BurstTraceDictionary input, TraceHandle preview, TraceHandle r)
        {
            if (r.IsCombined)
            {
                Debug.LogError($"Exception! First log is Nested! Is not allowed!");
                return r;
            }
#if BURSTTRACE_CAPTURE_PROFILER
            MarkerNestedSaveLog.Begin();
#endif
            if (!input.rows.IsCreated)
            {
                return default;
#if BURSTTRACE_CAPTURE_PROFILER
            MarkerNestedSaveLog.End();
#endif
            }
            var key = new uint2(preview.Value, r.Value);

            var threadIndex = customThread;
            var map = input.nestedMap.Get(threadIndex);
            var rows = input.nestedRows.Get(threadIndex);


            if (map.TryGetValue(key, out var index))
            {
#if BURSTTRACE_CAPTURE_PROFILER
                MarkerNestedSaveLog.End();
#endif
                return new TraceHandle(index, threadIndex, true);
            }
            map.Add(key, rows.Length);
            rows.Add(key);
#if BURSTTRACE_CAPTURE_PROFILER
            MarkerNestedSaveLog.End();
#endif
            return new TraceHandle(rows.Length - 1, threadIndex, true);
        }
    }



    internal abstract class BurstTraceSharedStatic
    {
        internal static readonly SharedStatic<BurstTraceDictionary> InfoField = SharedStatic<BurstTraceDictionary>.GetOrCreate<BurstTraceSharedStatic, SharedStaticKey>();

        private class SharedStaticKey { }
    }




    internal struct BurstTraceDictionary : System.IDisposable
    {
#if !BURSTTRACE_NOT_USE_64
        internal ArrayOfAllocs<NativeList<DetailedLog>> rows;
#else
    public ArrayOfAllocs<NativeList<FixedString512Bytes>> rows;
#endif
        internal ArrayOfAllocs<NativeHashMap<uint2, int>> nestedMap;
        internal ArrayOfAllocs<NativeList<uint2>> nestedRows;
#if !BURSTTRACE_NOT_USE_64
        internal ArrayOfAllocs<NativeHashMap<HashedKey, int>> hashedMap;
#else
    internal ArrayOfAllocs<NativeHashMap<FixedString512Bytes, int>> map;
#endif

        public BurstTraceDictionary(int rowsCount, Allocator allocator)
        {
            var max = JobsUtility.ThreadIndexCount;
            if (JobsUtility.ThreadIndexCount > 2047)
                Debug.LogError("BurstTrace: Too many threads! TraceHandle format limit exceeded.");

#if !BURSTTRACE_NOT_USE_64
            hashedMap = new ArrayOfAllocs<NativeHashMap<HashedKey, int>>(max, allocator);
            for (int i = 0; i < max; i++)
            {
                var _map = new NativeHashMap<HashedKey, int>(rowsCount, allocator);
                hashedMap.Allocate(i, ref _map);
            }
#else
        map = new ArrayOfAllocs<NativeHashMap<FixedString512Bytes, int>>(max, allocator);
        for (int i = 0; i < max; i++)
        {
            var _map = new NativeHashMap<FixedString512Bytes, int>(rowsCount, allocator);
            map.Allocate(i, ref _map);
        }
#endif
#if !BURSTTRACE_NOT_USE_64
            rows = new ArrayOfAllocs<NativeList<DetailedLog>>(max, allocator);
            for (int i = 0; i < max; i++)
            {
                var _map = new NativeList<DetailedLog>(rowsCount, allocator);
                rows.Allocate(i, ref _map);
                _map.Add(new DetailedLog { path = "Empty", member = "Null" });
            }
#else
        rows = new ArrayOfAllocs<NativeList<FixedString512Bytes>>(max, allocator);
        for (int i = 0; i < max; i++)
        {
            var _map = new NativeList<FixedString512Bytes>(rowsCount, allocator);
            rows.Allocate(i, ref _map);
            _map.Add("Empty (Null)");
        }
#endif
            nestedMap = new ArrayOfAllocs<NativeHashMap<uint2, int>>(max, allocator);
            for (int i = 0; i < max; i++)
            {
                var _map = new NativeHashMap<uint2, int>(rowsCount, allocator);
                nestedMap.Allocate(i, ref _map);
            }
            nestedRows = new ArrayOfAllocs<NativeList<uint2>>(max, allocator);
            for (int i = 0; i < max; i++)
            {
                var _map = new NativeList<uint2>(rowsCount, allocator);
                nestedRows.Allocate(i, ref _map);
                _map.Add(0);
            }
            BurstTraceSharedStatic.InfoField.Data = this;
        }

        //public static string GetFilePath => $"{Application.dataPath}/stackTraceMap.txt";
        public static string GetFilePath => $"{Application.persistentDataPath}/stackTraceMap.txt";

        public void SaveToFile()
        {
            var res = Serialize();
            System.IO.File.WriteAllText(GetFilePath, JsonUtility.ToJson(res));
        }
        public SerializedRoot Serialize()
        {
            List<SerializableStackTrace> serializableStackTrace = new List<SerializableStackTrace>();
            List<SerializableStackTraceNested> serializableStackTraceNested = new List<SerializableStackTraceNested>();
            for (int i = 0; i < rows.Length; i++)
            {
                var row = rows.Get(i);
                for (int k = 0; k < row.Length; k++)
                {
                    var item = row[k];
                    serializableStackTrace.Add(new SerializableStackTrace { key = new int2(i, k), value = item.ConvertToString() });
                }
            }
            for (int i = 0; i < nestedRows.Length; i++)
            {
                var row = nestedRows.Get(i);
                for (int k = 0; k < row.Length; k++)
                {
                    var item = row[k];
                    serializableStackTraceNested.Add(new SerializableStackTraceNested { key = new int2(i, k), value = item });
                }
            }
            var res = new SerializedRoot
            {
                serializableStackTrace = serializableStackTrace,
                serializableStackTraceNested = serializableStackTraceNested,
            };
            return res;
        }

        public int CalculateTotalMemoryAllocated()
        {
            int totalBytes = 0;
            var nestedMapSize = UnsafeUtility.SizeOf<uint2>() + UnsafeUtility.SizeOf<uint>();
            var nestedRowsSize = UnsafeUtility.SizeOf<uint2>();
#if !BURSTTRACE_NOT_USE_64
            var detailedLogSize = UnsafeUtility.SizeOf<DetailedLog>();
            var hashedMapKeySize = UnsafeUtility.SizeOf<HashedKey>() + UnsafeUtility.SizeOf<int>();
#else
            var detailedLogSize = UnsafeUtility.SizeOf<FixedString512Bytes>();
            var hashedMapKeySize = UnsafeUtility.SizeOf<FixedString512Bytes>() + UnsafeUtility.SizeOf<int>();
#endif
            for (int i = 0; i < rows.Length; i++)
            {
                ref var _row = ref rows[i];
                totalBytes += _row.Capacity * detailedLogSize;
            }
            for (int i = 0; i < nestedMap.Length; i++)
            {
                ref var _map = ref nestedMap[i];
                totalBytes += _map.Capacity * nestedMapSize;
            }
            for (int i = 0; i < nestedRows.Length; i++)
            {
                ref var _map = ref nestedRows[i];
                totalBytes += _map.Capacity * nestedRowsSize;
            }
#if !BURSTTRACE_NOT_USE_64
            for (int i = 0; i < hashedMap.Length; i++)
            {
                ref var _map = ref hashedMap[i];
                totalBytes += _map.Capacity * hashedMapKeySize;
            }
#else
            for (int i = 0; i < map.Length; i++)
            {
                ref var _map = ref map[i];
                totalBytes += _map.Capacity * hashedMapKeySize;
            }
#endif
            return totalBytes;
        }
        public int CalculateTotalMemoryUsed()
        {
            int totalBytes = 0;
            var nestedMapSize = UnsafeUtility.SizeOf<uint2>() + UnsafeUtility.SizeOf<uint>();
            var nestedRowsSize = UnsafeUtility.SizeOf<uint2>();
#if !BURSTTRACE_NOT_USE_64
            var detailedLogSize = UnsafeUtility.SizeOf<DetailedLog>();
            var hashedMapKeySize = UnsafeUtility.SizeOf<HashedKey>() + UnsafeUtility.SizeOf<int>();
#else
            var detailedLogSize = UnsafeUtility.SizeOf<FixedString512Bytes>();
            var hashedMapKeySize = UnsafeUtility.SizeOf<FixedString512Bytes>() + UnsafeUtility.SizeOf<int>();
#endif
            for (int i = 0; i < rows.Length; i++)
            {
                ref var _row = ref rows[i];
                totalBytes += _row.Length * detailedLogSize;
            }
            for (int i = 0; i < nestedMap.Length; i++)
            {
                ref var _map = ref nestedMap[i];
                totalBytes += _map.Count * nestedMapSize;
            }
            for (int i = 0; i < nestedRows.Length; i++)
            {
                ref var _map = ref nestedRows[i];
                totalBytes += _map.Length * nestedRowsSize;
            }
#if !BURSTTRACE_NOT_USE_64
            for (int i = 0; i < hashedMap.Length; i++)
            {
                ref var _map = ref hashedMap[i];
                totalBytes += _map.Count * hashedMapKeySize;
            }
#else
            for (int i = 0; i < map.Length; i++)
            {
                ref var _map = ref map[i];
                totalBytes += _map.Count * hashedMapKeySize;
            }
#endif
            return totalBytes;
        }

        public int GetThreadIndex => JobsUtility.ThreadIndex;

        public void Dispose()
        {
            SaveToFile();
#if !BURSTTRACE_NOT_USE_64
            hashedMap.Dispose();
#else
        map.Dispose();
#endif
            rows.Dispose();
            nestedMap.Dispose();
            nestedRows.Dispose();
            BurstTraceSharedStatic.InfoField.Data = default;
        }

        [System.Serializable]
        public class SerializedRoot
        {
            public List<SerializableStackTrace> serializableStackTrace;
            public List<SerializableStackTraceNested> serializableStackTraceNested;
        }
        [System.Serializable]
        public class SerializedMap
        {
            public static SerializedMap Instance
            {
                get
                {
                    if (_instance == null)
                        _instance = Create();
                    return _instance;
                }
            }
            private static SerializedMap _instance;


            public Dictionary<uint, string> values;
            public Dictionary<uint, uint2> nestedValues;

            public static SerializedMap Create()
            {
                var res = new SerializedMap();
                res.values = new Dictionary<uint, string>();
                res.nestedValues = new Dictionary<uint, uint2>();
                if (System.IO.File.Exists(BurstTraceDictionary.GetFilePath))
                {
                    var json = System.IO.File.ReadAllText(BurstTraceDictionary.GetFilePath);
                    var _deserialized = JsonUtility.FromJson<SerializedRoot>(json);
                    foreach (var item in _deserialized.serializableStackTrace)
                    {
                        res.values.Add(TraceHandle.Pack(item.key.y, item.key.x, false), item.value);
                    }
                    foreach (var item in _deserialized.serializableStackTraceNested)
                    {
                        res.nestedValues.Add(TraceHandle.Pack(item.key.y, item.key.x, true), item.value);
                    }
                }
                return res;
            }
        }
        [System.Serializable]
        public struct SerializableStackTrace
        {
            public int2 key;
            public string value;
        }
        [System.Serializable]
        public struct SerializableStackTraceNested
        {
            public int2 key;
            public uint2 value;
        }
    }




    internal abstract class BurstTraceSharedStaticDisabled
    {
        internal static readonly SharedStatic<bool> BoolField = SharedStatic<bool>.GetOrCreate<BurstTraceSharedStaticDisabled, SharedStaticKey>();
        public static bool IsDisabled => BoolField.Data;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SetDisabled(bool disabled) => BoolField.Data = disabled;

        private class SharedStaticKey { }
    }

    internal struct HashedKey : System.IEquatable<HashedKey>
    {
        public ulong a;
        public ulong b;
        public uint c;

        public bool Equals(HashedKey other) => a == other.a && b == other.b && c == other.c;

        public static HashedKey CreateHash(in DetailedLog log)
        {
            return new HashedKey
            {
                a = GenerateHash(in log.path),
                b = GenerateHash(in log.member),
                c = (uint)(log.line),
            };
        }
        public static ulong GenerateHash(in FixedString512Bytes path)
        {
            ulong hash = 14695981039346656037UL;
            for (int i = 0; i < path.Length; i++)
            {
                hash ^= path[i];
                hash *= 1099511628211UL;
            }
            return hash;
        }
        public static ulong GenerateHash(in FixedString64Bytes path)
        {
            ulong hash = 14695981039346656037UL;
            for (int i = 0; i < path.Length; i++)
            {
                hash ^= path[i];
                hash *= 1099511628211UL;
            }
            return hash;
        }

        public override int GetHashCode()
        {
            return (int)math.hash(new uint3(
                (uint)(a ^ (a >> 32)),
                (uint)(b ^ (b >> 32)),
                c));
        }
    }
}