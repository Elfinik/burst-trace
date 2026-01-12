using UnityEngine;
using NUnit.Framework;

#if !BURSTTRACE_DISABLE
using Elfinik.BurstTrace.Internal;
using System.Collections;
using System.Collections.Generic;

namespace Elfinik.BurstTrace.Tests
{
    [DefaultExecutionOrder(-1)]
    public class BurstTraceSessionController : MonoBehaviour
    {
        private static BurstTraceSessionController _instance;
        internal BurstTraceDictionary stackStorage;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            if (_instance != null) return;
            var clone = new GameObject() { name = "___BURST_STACKTRACE_CONTAINER___", hideFlags = HideFlags.HideInHierarchy };
            _instance = clone.AddComponent<BurstTraceSessionController>();
            DontDestroyOnLoad(clone);
        }


        private void Awake()
        {
            var allocSize = 512;
            if (BurstTraceConfig.TryGetSettings(out var settingsProfile))
                allocSize = Mathf.Clamp(settingsProfile.preallocRows,64, 4096);

            stackStorage = new BurstTraceDictionary(allocSize, Unity.Collections.Allocator.Persistent);
        }


        private bool alreadyDisposed = false;
        public void DestroyAndDispose()
        {
            if (_instance == this) _instance = null;
            alreadyDisposed = true;
            Destroy(gameObject);
            stackStorage.Dispose();

        }

        internal static void ForceInitialize()
        {
            Assert.IsTrue(_instance == null);
            OnAfterSceneLoad();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
            if(!alreadyDisposed) stackStorage.Dispose();
        }
    }
}
#else
namespace Elfinik.BurstTrace.Tests
{
    public class BurstTraceSessionController : MonoBehaviour 
    { 
        public void DestroyAndDispose() {}
        public static void ForceInitialize() {}
    
    }
}
#endif