using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using Unity.Collections;
using UnityEngine;
using Elfinik.BurstTrace.Internal;
using UnityEngine.Profiling;

namespace Elfinik.BurstTrace.Tests
{

    public class MemoryLeakTests
    {
        [Test]
        public void Recording_And_Disposing_ShouldNotLeak()
        {
            CollectionsMemory.StartCaptureLeaks();
            var leaks = CollectionsMemory.ForgiveLeaksNow();
            var controller = GameObject.FindAnyObjectByType<BurstTraceSessionController>();
            controller.DestroyAndDispose();
            leaks = CollectionsMemory.CheckLeaksNow();
            Assert.AreEqual(leaks, 0, "Container not disposed!");

            var stackStorage = new BurstTraceDictionary(512, Unity.Collections.Allocator.Persistent);
            var log = TraceHandle.Capture();
            for (int i = 0; i < 256; i++)
            {
                log = TraceHandle.Capture(log);
            }

            stackStorage.Dispose();
            leaks = CollectionsMemory.CheckLeaksNow();

            Assert.AreEqual(leaks, 0, "Log entries should not allocate managed memory");
            BurstTraceSessionController.ForceInitialize();
        }
    }
}