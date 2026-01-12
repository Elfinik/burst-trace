#if !BURSTTRACE_DISABLE
using Elfinik.BurstTrace.Internal;
using System.Collections;
using UnityEngine;

namespace Elfinik.BurstTrace.Tests
{
    public class GCAllocStressTest : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1);
            var controller = FindAnyObjectByType<BurstTraceSessionController>();
            controller.DestroyAndDispose();

            yield return new WaitForSeconds(1);
            for (int k = 0; k < 5; k++)
            {

                yield return new WaitForSeconds(1);
                long memoryBefore = System.GC.GetTotalMemory(true);
                var stackStorage = new BurstTraceDictionary(512, Unity.Collections.Allocator.Persistent);

                stackStorage.Dispose();
                long memoryAfter = System.GC.GetTotalMemory(true);
                Debug.LogError($"[{k}] Total Memory 1: {(int)(memoryAfter - memoryBefore) / 1024}kb");


                yield return new WaitForSeconds(1);
                memoryBefore = System.GC.GetTotalMemory(true);
                stackStorage = new BurstTraceDictionary(512, Unity.Collections.Allocator.Persistent);
                var log = TraceHandle.Capture();
                for (int i = 0; i < 2048; i++)
                {
                    log = TraceHandle.Capture(log);
                }
                stackStorage.Dispose();
                memoryAfter = System.GC.GetTotalMemory(true);
                Debug.LogError($"[{k}] Total Memory 2: {(int)(memoryAfter - memoryBefore) / 1024}kb");
            }
            BurstTraceSessionController.ForceInitialize();
        }
    }
}
#endif