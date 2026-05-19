using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using Unity.Collections;
using UnityEngine;
using Elfinik.BurstTrace.Internal;
using UnityEngine.Profiling;
using Unity.Collections.BurstTrace;

namespace Elfinik.BurstTrace.Tests
{

    public class MemoryLeakTests
    {
        [Test]
        public void Recording_And_Disposing_ShouldNotLeak()
        {
            BurstTrace_CollectionsMemory.StartCaptureLeaks();
            var leaks = BurstTrace_CollectionsMemory.ForgiveLeaksNow();
            var controller = GameObject.FindAnyObjectByType<BurstTraceSessionController>();
            controller.DestroyAndDispose();
            leaks = BurstTrace_CollectionsMemory.CheckLeaksNow();
            Assert.AreEqual(leaks, 0, "Container not disposed!");

            var stackStorage = new BurstTraceDictionary(512, Unity.Collections.Allocator.Persistent);
            var log = TraceHandle.Capture();
            for (int i = 0; i < 256; i++)
            {
                log = TraceHandle.Capture(log);
            }

            stackStorage.Dispose();
            leaks = BurstTrace_CollectionsMemory.CheckLeaksNow();

            Assert.AreEqual(leaks, 0, "Log entries should not allocate managed memory");
            BurstTraceSessionController.ForceInitialize();
        }
    }
}