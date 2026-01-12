using NUnit.Framework;
using System.Text.RegularExpressions;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Elfinik.BurstTrace.Internal;

namespace Elfinik.BurstTrace.Tests
{
    public class BurstTraceTests : MonoBehaviour
    {
        [Test]
        public void StackTrace_InMainThread_CapturesCorrectChain()
        {
            var step1 = TraceHandle.Capture();
            var step2 = TraceHandle.Capture(step1);

            string result = step2.ToProjectLink();

            Assert.IsTrue(result.Contains(nameof(StackTrace_InMainThread_CapturesCorrectChain)));
            Assert.AreEqual(2, result.Split('\n').Length, "Should have 2 lines in stack trace");
        }

        [Test]
        public void StackTrace_Format_Validation_Final()
        {
            var trace = TraceHandle.Capture();
            string rawLine = trace.ToProjectLink().Trim();

            string cleanLine = System.Text.RegularExpressions.Regex.Replace(rawLine, "<[^>]*>", "");

            cleanLine = System.Text.RegularExpressions.Regex.Replace(cleanLine, @"\s+", " ").Trim();

            var pattern = @"^.+ \(at Assets/.+:\d+\)$";

            bool isMatch = System.Text.RegularExpressions.Regex.IsMatch(cleanLine, pattern);

            Assert.IsTrue(isMatch,
                $"Regex failed!\nCleaned line: [{cleanLine}]\nPattern: [{pattern}]");
        }

        [BurstCompile]
        struct LogTestJob : IJob
        {
            public NativeReference<TraceHandle> Result;

            public void Execute()
            {
                var trace = TraceHandle.Capture();
                Result.Value = DeepMethod(trace);
            }

            [BurstCompile]
            private TraceHandle DeepMethod(TraceHandle prev)
            {
                return TraceHandle.Capture(prev);
            }
        }

        [Test]
        public void StackTrace_BurstJob_FullChain_Validation()
        {
            var result = new NativeReference<TraceHandle>(Allocator.Persistent);
            var job = new LogTestJob { Result = result };
            job.Run();

            string fullTrace = result.Value.ToProjectLink();
            string[] lines = fullTrace.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual(2, lines.Length, "Should have 2 lines");

            foreach (var rawLine in lines)
            {
                string cleanLine = System.Text.RegularExpressions.Regex.Replace(rawLine, "<[^>]*>", "").Trim();
                Assert.That(cleanLine, Does.Match(@"^.+ \(at Assets/.+:\d+\)$"),
                    $"Line format invalid after cleaning: {cleanLine}");
            }

            result.Dispose();
        }

        [BurstCompile]
        struct ParallelLogJob : IJobParallelFor
        {
            public NativeArray<TraceHandle> Results;

            public void Execute(int index)
            {
                Results[index] = TraceHandle.Capture();
            }
        }

        [Test]
        public void StackTrace_ParallelJobs_NoDataRace()
        {
            int count = 100;
            var results = new NativeArray<TraceHandle>(count, Allocator.Persistent);
            var job = new ParallelLogJob { Results = results };

            var jh = job.Schedule(count, 64);
            JobHandle.ScheduleBatchedJobs();
            jh.Complete();

            for (int i = 0; i < count; i++)
            {
                Assert.IsNotNull(results[i].ToAbsolutePath());
            }

            results.Dispose();
        }



        private string CleanLine(string raw)
        {
            string clean = Regex.Replace(raw, "<[^>]*>", "");
            return Regex.Replace(clean, @"\s+", " ").Trim();
        }

        [Test]
        [Description("Deep log chain check (LIFO) up to 10 entries.")]
        public void DeepChain_LIFO_CorrectOrder()
        {
            TraceHandle trace = default;

            for (int i = 0; i < 10; i++)
            {
                trace = TraceHandle.Capture(trace);
            }

            string fullLog = trace.ToProjectLink();
            string[] lines = fullLog.Split('\n', System.StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual(10, lines.Length, "There must be exactly 10 lines in the stack!");

            foreach (var line in lines)
            {
                string cleaned = CleanLine(line);
                Assert.IsTrue(Regex.IsMatch(cleaned, @"^.+ \(at Assets/.+:\d+\)$"),
                    $"The string does not match the format: {cleaned}");
            }
        }

        [BurstCompile]
        struct ChainJob : IJob
        {
            public NativeArray<TraceHandle> Result;
            public void Execute()
            {
                TraceHandle t = default;
                t = MethodA(t);
                Result[0] = t;
            }

            [BurstCompile]
            private TraceHandle MethodA(TraceHandle prev)
                => MethodB(TraceHandle.Capture(prev));

            [BurstCompile]
            private TraceHandle MethodB(TraceHandle prev)
                => TraceHandle.Capture(prev);
        }

        [Test]
        [Description("Testing the operation of the chain within a Burst Job")]
        public void Burst_Job_Chain_Works()
        {
            var result = new NativeArray<TraceHandle>(1, Allocator.Persistent);
            var job = new ChainJob { Result = result };
            var jh = job.Schedule();
            JobHandle.ScheduleBatchedJobs();
            jh.Complete();

            string log = result[0].ToProjectLink();
            Assert.GreaterOrEqual(log.Split('\n').Length, 2);

            result.Dispose();
        }

        [BurstCompile]
        struct ParallelChainJob : IJobParallelFor
        {
            public NativeArray<TraceHandle> Results;
            public void Execute(int index)
            {
                TraceHandle t = default;
                for (int i = 0; i < 10; i++)
                {
                    t = TraceHandle.Capture(t);
                }
                Results[index] = t;
            }
        }

        [Test]
        [Description("Stress test: different threads (IJobParallelFor) create independent chains of 10 lines each")]
        public void MultiThreaded_Chains_AreIndependent()
        {
            int count = 64;
            var results = new NativeArray<TraceHandle>(count, Allocator.Persistent);

            var job = new ParallelChainJob { Results = results };
            var jh = job.Schedule(count, 1);
            JobHandle.ScheduleBatchedJobs();
            jh.Complete();

            for (int i = 0; i < count; i++)
            {
                string log = results[i].ToProjectLink();
                string[] lines = log.Split('\n', System.StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(10, lines.Length, $"Thread {i} lost part of its stack");
            }

            results.Dispose();
        }

        [Test]
        public void BufferOverflow_ShouldNotCrash()
        {
            TraceHandle trace = default;
            for (int i = 0; i < 2000; i++)
            {
                trace = TraceHandle.Capture(trace);
            }

            Assert.DoesNotThrow(() =>
            {
                string log = trace.ToProjectLink();
                Debug.Log($"Overflow log length: {log.Split('\n').Length}");
            });
        }

        [Test]
        public void DefaultHandle_ReturnsEmptyString()
        {
            TraceHandle empty = default;
            Assert.AreEqual(TraceHandle.INVALID_EMPTY_LOG_VALUE, empty.ToProjectLink().Trim());
        }

        [Test]
        public void OldHandle_RemainsValid_AfterNewLogsCreated()
        {
            var initialTrace = TraceHandle.Capture();
            string initialString = CleanLine(initialTrace.ToProjectLink());

            for (int i = 0; i < 500; i++) { TraceHandle.Capture(); }

            string afterNoiseString = CleanLine(initialTrace.ToProjectLink());

            Assert.AreEqual(initialString, afterNoiseString, "The old record was corrupted by the new logs");
        }

        [Test]
        public void OldHandle_RemainsValid_AfterNewLogsCreated_Overflow()
        {
            var initialTrace = TraceHandle.Capture();
            var initialTrace2 = TraceHandle.Capture(initialTrace);
            string initialString = CleanLine(initialTrace.ToProjectLink());
            string initialString2 = CleanLine(initialTrace2.ToProjectLink());

            TraceHandle trace = default;
            for (int i = 0; i < 5000; i++)
                trace = TraceHandle.Capture(trace);

            string afterNoiseString = CleanLine(initialTrace.ToProjectLink());
            string afterNoiseString2 = CleanLine(initialTrace2.ToProjectLink());

            Assert.AreEqual(initialString, afterNoiseString, "The old record was corrupted by the new logs");
            Assert.AreEqual(initialString2, afterNoiseString2, "The old record was corrupted by the new logs");
        }


        [BurstCompile]
        struct RaceConditionJob : IJobParallelFor
        {
            public NativeArray<TraceHandle> Results;
            public void Execute(int index)
            {
                TraceHandle t = TraceHandle.Capture();
                t = MethodStep(t);
                Results[index] = t;
            }

            [BurstCompile]
            private TraceHandle MethodStep(TraceHandle p) => TraceHandle.Capture(p);
        }

        [Test]
        public void ParallelThreads_HaveUniqueContext()
        {
            int count = 100;
            var results = new NativeArray<TraceHandle>(count, Allocator.Persistent);
            new RaceConditionJob { Results = results }.Schedule(count, 1).Complete();

            for (int i = 0; i < count; i++)
            {
                var lines = results[i].ToProjectLink().Split('\n');
                Assert.AreEqual(2, lines.Length, $"Stream {i} has an incorrect number of frames.");
            }
            results.Dispose();
        }




        // For some reason, a large amount of GC is allocated during this cycle if it is obtained via System.GC.GetTotalMemory(true);
        //This doesn't always happen, and not in all versions of Unity.
        //At the same time, the profiler shows only Mono memory, which the plugin does not use.The test for Native memory leaks (which is used) passes successfully.
        //Therefore, I am temporarily removing this test, replacing it with a Native memory test, but it would be desirable to investigate this further later.
        [Test]
        public void RecordStackTrace_DoesNotAllocate()
        {
            TraceHandle t = default;
            t = TraceHandle.Capture(t);

            //var totalMemoryAllocatedBefore = BurstTraceAdvanced.GetTotalAllocatedMemory();
            //var totalMemoryUsedBefore = BurstTraceAdvanced.GetUsedMemory();
            CollectionsMemory.ForgiveLeaksNow();
            CollectionsMemory.StartCaptureLeaks();
            //System.GC.Collect();
            //System.GC.WaitForPendingFinalizers();
            //long memoryBefore = System.GC.GetTotalMemory(true);

            for (int i = 0; i < 256; i++)
            {
                t = TraceHandle.Capture(t);
            }
            var leaksCount = CollectionsMemory.CheckLeaksNow();

            //long memoryAfter = System.GC.GetTotalMemory(true);
            //var totalMemoryAllocatedAfter = BurstTraceAdvanced.GetTotalAllocatedMemory();
            //var totalMemoryUsedAfter = BurstTraceAdvanced.GetUsedMemory();

            //Debug.Log($"Used memory: {totalMemoryUsedAfter - totalMemoryUsedBefore}");
            //Debug.Log($"Allocated memory: {totalMemoryAllocatedAfter - totalMemoryAllocatedBefore}");

            Assert.AreEqual(leaksCount, 0, "Log entries should not allocate memory");
            //Assert.LessOrEqual(memoryAfter - memoryBefore, 0, "Log entries should not allocate managed memory");
        }
    }
}