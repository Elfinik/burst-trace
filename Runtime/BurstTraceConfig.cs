using UnityEngine;

namespace Elfinik.BurstTrace.Internal
{
    public class BurstTraceConfig : ScriptableObject
    {
        [Range(64, 4096)]
        public int preallocRows = 1024;
        internal const string FILE_NAME = "Burst Trace Config";
        internal const string FILE_PATH = "Burst Trace Config.asset";


        public static bool TryGetSettings(out BurstTraceConfig settings)
        {
            settings = Resources.Load<BurstTraceConfig>(FILE_NAME);
            return settings != null;
        }
    }
}