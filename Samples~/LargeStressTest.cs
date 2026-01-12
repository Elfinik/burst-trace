using Elfinik.BurstTrace.Internal;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

namespace Elfinik.BurstTrace.Samples
{
    public class LargeStressTest : MonoBehaviour
    {
        public NativeArray<TraceHandle> results;
        public Slider slider;
        public int totalIterations = 1024 * 1024;
        public int currentIteration;
        public Text txt_report;

        void Start()
        {
            currentIteration = 0;
            slider.minValue = 0;
            slider.maxValue = totalIterations;
            results = new NativeArray<TraceHandle>(32, Allocator.Persistent);
        }

        void Update()
        {
            currentIteration++;
            slider.value = currentIteration;
            if (currentIteration >= totalIterations)
            {
                enabled = false;
                txt_report.text = $"==COMPLETED==\r\n" + txt_report.text;
            }
            else
            {
                txt_report.text = $"{currentIteration} / {totalIterations}\r\nAlloc: {BurstTraceAdvanced.GetTotalAllocatedMemory()} bytes, used: {BurstTraceAdvanced.GetUsedMemory()} bytes";
                var trace = TraceHandle.Capture();
                for (int i = 0; i < 100; i++)
                {
                    trace = TraceHandle.Capture(trace);
                }
                if (txt_report.text.Length < 10000)
                    txt_report.text += $"\r\n{trace.ToProjectLink()}";
                var job = new ParallelChainJob { results = results };
                var jh = job.Schedule(results.Length, 1);
                jh.Complete();
                foreach (var item in results)
                {
                    if (txt_report.text.Length < 10000)
                        txt_report.text += $"\r\n{item.ToConsoleToken()}";
                }
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
                PrintLog(TraceHandle.Capture());
            }
        }

        public void PrintLog(TraceHandle handle)
        {
            var res = TraceHandle.Capture(handle).ToProjectLink();
            if (txt_report.text.Length < 10000)
                txt_report.text += $"\r\n{res}";
        }
        public void PrintLog2(TraceHandle handle)
        {
            if (txt_report.text.Length < 10000)
                txt_report.text += $"\r\n{handle.ToProjectLink()}";
        }

        private void OnDestroy()
        {
            results.Dispose();
        }
    }
    [BurstCompile]
    struct ParallelChainJob : IJobParallelFor
    {
        public NativeArray<TraceHandle> results;
        public void Execute(int index)
        {
            TraceHandle t = default;
            for (int i = 0; i < 64; i++)
            {
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
                t = TraceHandle.Capture(t);
            }
            results[index] = t;
        }
    }
}