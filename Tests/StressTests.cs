using Elfinik.BurstTrace.Internal;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Elfinik.BurstTrace.Tests
{
    public class StressTests
    {
        [UnityTest]
        public IEnumerator MassiveRecording_ShouldNotCrash()
        {
            int iterations = 100_000;
            TraceHandle lastHandle = default;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
            {
                lastHandle = TraceHandle.Capture(lastHandle);

                if (i % 1000 == 0 && stopwatch.ElapsedMilliseconds > 16)
                {
                    yield return null; 
                    stopwatch.Restart();
                }
            }

            Assert.AreNotEqual(0, lastHandle.Value, "Handle should be valid even after stress");

            string logResult = lastHandle.ToAbsolutePath();

            Assert.IsNotEmpty(logResult);
            Debug.Log("Stress Test Finished. Log Length: " + logResult.Length);
        }

        [Test]
        public void CircularDependency_ShouldNotHang()
        {
            var handleA = TraceHandle.Capture();
            var handleB = TraceHandle.Capture(handleA);

            TraceHandle current = handleA;
            for (int i = 0; i < BurstTraceInternal.MAX_STACK_DEPTH * 3 + 10; i++)
            {
                current = TraceHandle.Capture(current);
            }

            string output = current.ToAbsolutePath();

            int lines = output.Split('\n').Length;
            Assert.Less(lines, BurstTraceInternal.MAX_STACK_DEPTH + 5, "Should truncate output to prevent infinite loops");
        }
    }
}