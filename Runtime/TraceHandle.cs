using Elfinik.BurstTrace.Internal;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace Elfinik.BurstTrace
{
    /// <summary>
    /// Represents a handle to a specific log entry or a chain of logs. 
    /// This struct is thread-safe and can be passed between Burst jobs and the Main Thread.
    /// <remarks>Note: Persistent cross-session serialization is not currently supported.</remarks>
    /// </summary>
    [System.Serializable]
    public partial struct TraceHandle
    {
#if BURSTTRACE_DISABLE
        public static string INVALID_EMPTY_LOG_VALUE => "Empty (invalid) log. WARNING! Logging is Disabled!";
#else
        public static string INVALID_EMPTY_LOG_VALUE => "Empty (invalid) log";
#endif
        public static TraceHandle Null => default;
        public bool IsValid => RowIndex > 0 && ThreadIndex >= 0;


        internal uint Value;

        private const int RowIdShift = 0;
        private const int ThreadIdShift = 20;
        private const int CombinedFlagShift = 31;

        private const uint RowIdMask = 0xFFFFF;
        private const uint ThreadIdMask = 0x7FF;
        private const uint CombinedFlagMask = 0x1;

        public int RowIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)((Value >> RowIdShift) & RowIdMask);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal set => Value = (Value & ~(RowIdMask << RowIdShift)) | ((uint)value & RowIdMask) << RowIdShift;
        }

        public int ThreadIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)((Value >> ThreadIdShift) & ThreadIdMask);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal set => Value = (Value & ~(ThreadIdMask << ThreadIdShift)) | ((uint)value & ThreadIdMask) << ThreadIdShift;
        }

        public bool IsCombined
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ((Value >> CombinedFlagShift) & CombinedFlagMask) == 1;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal set => Value = (Value & ~(CombinedFlagMask << CombinedFlagShift)) | (value ? 1u : 0u) << CombinedFlagShift;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint Pack(int rowIndex, int threadIndex, bool isCombined)
        {
            return ((uint)rowIndex & RowIdMask) << RowIdShift |
                    ((uint)threadIndex & ThreadIdMask) << ThreadIdShift |
                    (isCombined ? 1u : 0u) << CombinedFlagShift;
        }

        internal TraceHandle(int rowIndex, int threadIndex)
        {
            this.Value = Pack(rowIndex, threadIndex, false);
        }
        internal TraceHandle(int rowIndex, int threadIndex, bool isCombined)
        {
            this.Value = Pack(rowIndex, threadIndex, isCombined);
        }


        /// <summary>
        /// Returns a formatted string of the log chain using relative file paths (relative to the Project Root).
        /// </summary>
        [BurstDiscard]
        public string ToProjectLink()
        {
            var res = BurstTraceInternal.GetManagedLog(this);
            res = MakePathsRelative(res);
            return res;
        }

        /// <summary>
        /// Returns a formatted string of the log chain using absolute file paths.
        /// </summary>
        [BurstDiscard]
        public string ToAbsolutePath()
        {
            return BurstTraceInternal.GetManagedLog(this);
        }

        /// <summary>
        /// Generates a clickable console hyperlink as a <see cref="FixedString128Bytes"/>.
        /// </summary>
        /// <remarks>
        /// This method is designed for use within Burst-compiled jobs. Instead of formatting the entire 
        /// log chain (which is string-intensive), it outputs a lightweight token. When clicked in the 
        /// Unity Console, it triggers a custom callback to resolve and display the full stack trace.
        /// </remarks>
        /// <returns>A fixed-size string containing the formatted hyperlink.</returns>
        public FixedString128Bytes ToConsoleToken()
        {
            //BurstStackTraceLogger Delayed
#if UNITY_6000_0_OR_NEWER
           return $"<color=#40a0ff><link=\"bstld='{Value}'\">CLICK TO PRINT LOG</link></color>";
#else
            return $"<a bstld=\"{Value}\">CLICK TO PRINT LOG</a>";
#endif
        }

        public override string ToString()
        {
            return $"Thread: {ThreadIndex}, Row: {RowIndex}, IsNested: {IsCombined}";
        }

        internal static string MakePathsRelative(string input)
        {
            // Get the absolute path to the project (e.g., C:/Users/Name/Project/Assets)
            // We normalize slashes to ensure they match (Unity uses / even on Windows)
            string assetsPath = Application.dataPath.Replace("\\", "/");

            // This regex looks for patterns like (C:/Path/To/Assets/File.cs:123)
            // It captures the full path inside the parentheses
            return System.Text.RegularExpressions.Regex.Replace(input, @"\(at (.*?\.cs):(\d+)\)", (match) =>
            {
                string fullPath = match.Groups[1].Value.Replace("\\", "/");
                string lineNum = match.Groups[2].Value;

            // If the full path contains our Assets folder path, remove the prefix
            if (fullPath.Contains(assetsPath))
                {
                // We keep "Assets" by finding its index
                int assetsIndex = fullPath.IndexOf("Assets/");
                    if (assetsIndex != -1)
                    {
                        string relativePath = fullPath.Substring(assetsIndex);
                    //return $"(at {relativePath}.cs:{lineNum})";
#if UNITY_6000_0_OR_NEWER
                        string link = $"<color=#40a0ff><link=\"href='{relativePath}' line='{lineNum}'\">{relativePath}:{lineNum}</link></color>";
#else
                    string link = $"<a href=\"{relativePath}\" line=\"{lineNum}\">{relativePath}:{lineNum}</a>";
#endif
                    return $"(at {link})";
                    }
                }

                return match.Value; // Return original if no match found
        });
        }
    }
}