using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.PerformanceTesting;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.TestTools;

namespace Elfinik.BurstTrace.Tests
{
    public class BurstStackTraceLoggerPerformanceTests : MonoBehaviour
    {
        [Test, Performance]
        public void Performance_Burst_InternalLoop()
        {
            var result = new NativeArray<TraceHandle>(1, Allocator.Persistent);

            new PerfJob { Result = result, Iterations = 10 }.Run();

            Measure.Method(() =>
            {
            var job = new PerfJob { Result = result, Iterations = 100 };
                job.Run();
            })
            .MeasurementCount(50) 
            .Run();

            result.Dispose();
        }

        [BurstCompile(CompileSynchronously = true)]
        struct PerfJob : IJob
        {
            public int Iterations;
            public NativeArray<TraceHandle> Result;

            public void Execute()
            {
                TraceHandle t = default;
                for (int i = 0; i < Iterations; i++)
                {
                    t = TraceHandle.Capture(t);
                }
                Result[0] = t;
            }
        }






        private const int Iterations = 500;
        [Test, Performance]
        public void Compare_Loggers()
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < Iterations; i++)
                {
                    Debug.Log($"Log message {i}");
                }
            })
            .SampleGroup("1. Standard Debug.Log (Main)")
            .MeasurementCount(50)
            .Run();

            var logJob = new DebugLogJob { Count = Iterations };
            logJob.Run();

            Measure.Method(() =>
            {
                logJob.Run();
            })
            .SampleGroup("2. Debug.Log (Burst Job)")
            .MeasurementCount(50)
            .Run();

            //    yield return null;

            Measure.Method(() =>
            {
                TraceHandle trace = default;
                for (int i = 0; i < Iterations; i++)
                {
                    trace = TraceHandle.Capture(trace);
                }
            })
            .SampleGroup("3. BurstStackLogger (Mono)")
            .MeasurementCount(50)
            .Run();

            //  yield return null;

            var result = new NativeArray<TraceHandle>(1, Allocator.Persistent);
            var perfJob = new PerfJob { Result = result, Iterations = Iterations };
            perfJob.Run();
            Measure.Method(() =>
            {
                perfJob.Run();
            })
            .SampleGroup("4. BurstTrace (Burst Job)")
            .MeasurementCount(50)
            .Run();

            result.Dispose();

            int jobCount = 32;
            var results = new NativeArray<TraceHandle>(jobCount, Allocator.Persistent);
            var job = new ParallelPerfJob { Results = results, Count = Iterations / jobCount };
            job.Run(1);

            //job.Run(jobCount);
            //yield return null;

            Measure.Method(() =>
            {
                job.Run(jobCount);
            })
            .SampleGroup("5. BurstTrace Parallel Stress (Burst)")
            .MeasurementCount(50)
            .Run();

            results.Dispose();



             results = new NativeArray<TraceHandle>(jobCount, Allocator.Persistent);
             job = new ParallelPerfJob { Results = results, Count = Iterations};
            job.Run(1);

            //job.Run(jobCount);
            //yield return null;

            Measure.Method(() =>
            {
                job.Run(jobCount);
            })
            .SampleGroup("6. BurstTrace Parallel Stress x32 (Burst)")
            .MeasurementCount(50)
            .Run();

            results.Dispose();
            //yield return null;
        }

        [BurstCompile(CompileSynchronously = true)]
        struct DebugLogJob : IJob
        {
            public int Count;
            public void Execute()
            {
                for (int i = 0; i < Count; i++)
                {
                    Debug.Log("Burst Job Log Item");
                }
            }
        }


        //[UnityTest, Performance]
        //public IEnumerator Parallel_Burst_Stress_Fixed()
        //{
        //    int jobCount = 32;
        //    var results = new NativeArray<TraceHandle>(jobCount, Allocator.Persistent);
        //    var job = new ParallelPerfJob { Results = results, Count = 100 };

        //    //job.Run(jobCount);
        //    //yield return null;

        //    Measure.Method(() =>
        //    {
        //        job.Run(jobCount);
        //    })
        //    .SampleGroup("5. Parallel Stress (Burst)")
        //    .MeasurementCount(50)
        //    .Run();

        //    results.Dispose();
        //    yield return null;
        //}

        [BurstCompile(CompileSynchronously = true)]
        struct ParallelPerfJob : IJobParallelFor
        {
            public int Count;
            public NativeArray<TraceHandle> Results;
            public void Execute(int index)
            {
                TraceHandle t = default;
                for (int i = 0; i < Count; i++)
                {
                    t = TraceHandle.Capture(t);
                }
                Results[index] = t;
            }
        }
    }
}