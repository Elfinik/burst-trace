using UnityEngine;
using UnityEngine.UI;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using System.Runtime.CompilerServices;
using System.Text;

namespace Elfinik.BurstTrace.Samples
{
    [System.Serializable]
    public class Nested
    {
        public TraceHandle stackTrace;
    }
    [System.Serializable]
    public class NestedOfNested
    {
        public Nested[] n;
    }
    public class BurstTraceSampleMono : MonoBehaviour
    {
        public static BurstTraceSampleMono Instance;
        public TraceHandle fieldInInspector;

        [Header("Any fields from UnityEditor Inspector")]
        [Header("Every row is hyperlink. Click to open the file at a specific line")]
        public TraceHandle[] arrayOfTraces;
        [Header("UI References")]
        [SerializeField] private Text logDisplay;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Button runTestsButton;
        public NestedOfNested[] nested;

        private StringBuilder _resultsAccumulator = new StringBuilder();

        public TraceHandle test;
        public TraceHandle test2;
        public TraceHandle test3;

        private void Awake()
        {
            Instance = this;
            if (runTestsButton != null)
                runTestsButton.onClick.AddListener(RunAllTests);
            LogToUI("System Initialized. Press 'Run Tests' to start.");
        }

        private void Start()
        {
            nested = new NestedOfNested[10];
            for (int i = 0; i < 10; i++)
            {
                var clone =new NestedOfNested();
                clone.n = new Nested[5];
                for (int k = 0; k < 5; k++)
                {
                    clone.n[k] = new Nested { stackTrace = TraceHandle.Capture()};
                }
                nested[i] = clone; 
            }
            test = TraceHandle.Capture();
            test2 = TraceHandle.Capture(test);
            test3 = TraceHandle.Capture(test2);
            for (int i = 0; i < 10; i++)
            {
                test3 = TraceHandle.Capture(test3);
            }
            Debug.LogError($"Delayed output: {test.ToConsoleToken()}");
            Debug.LogError($"Delayed output (chain): {test3.ToConsoleToken()}");
            fieldInInspector = TraceHandle.Capture();
            StoreHostiryArray(TraceHandle.Capture());
            Debug.LogError($"StackResult output: {test3.ToProjectLink()}");
        }

        public void StoreHostiryArray(TraceHandle traceHandle)
        {
            arrayOfTraces = new TraceHandle[3];
            arrayOfTraces[0] = TraceHandle.Capture(traceHandle);
            arrayOfTraces[1] = TraceHandle.Capture(traceHandle);
            arrayOfTraces[2] = TraceHandle.Capture(traceHandle);
        }

        public void RunAllTests()
        {
            _resultsAccumulator.Clear();
            LogToUI("=== STARTING INTEGRATION TESTS ===\n");

            TestMonoThread();
            TestSingleBurstJob();
            TestParallelBurstJob();
            TestNestedChain();

            LogToUI("\n=== ALL TESTS COMPLETED ===");
        }

        public void LogToUI(string message)
        {
            _resultsAccumulator.AppendLine($"[{Time.time:F2}] {message}");
            if (logDisplay != null)
            {
                logDisplay.text = _resultsAccumulator.ToString();
                if (scrollRect != null)
                {
                    Canvas.ForceUpdateCanvases();
                    scrollRect.verticalNormalizedPosition = 0f;
                }
            }
            Debug.Log(message);
        }

        #region Test 1: Mono Thread
        private void TestMonoThread()
        {
            LogToUI("Testing: Mono Thread...");
            var trace = TraceHandle.Capture();
            LogToUI($"Result:\n{trace.ToProjectLink()}");
        }
        #endregion

        #region Test 2: Single Burst Job
        [BurstCompile]
        struct SimpleLogJob : IJob
        {
            public NativeArray<TraceHandle> result;

            public void Execute()
            {
                result[0] = TraceHandle.Capture();
            }
        }

        private void TestSingleBurstJob()
        {
            LogToUI("Testing: Single Burst Job...");
            var container = new NativeArray<TraceHandle>(1, Allocator.TempJob);

            var job = new SimpleLogJob { result = container };
            job.Schedule().Complete();

            LogToUI($"Result from Burst:\n{container[0].ToProjectLink()}");
            container.Dispose();
        }
        #endregion

        #region Test 3: Parallel Job
        [BurstCompile]
        struct ParallelLogJob : IJobParallelFor
        {
            public NativeArray<TraceHandle> results;

            public void Execute(int index)
            {
                results[index] = TraceHandle.Capture();
            }
        }

        private void TestParallelBurstJob()
        {
            const int count = 4;
            LogToUI($"Testing: Parallel Burst Job ({count} threads)...");
            var container = new NativeArray<TraceHandle>(count, Allocator.TempJob);

            var job = new ParallelLogJob { results = container };
            job.Schedule(count, 1).Complete();

            for (int i = 0; i < count; i++)
            {
                LogToUI($"Thread {i} Stack:\n{container[i].ToProjectLink()}");
            }
            container.Dispose();
        }
        #endregion

        #region Test 4: Nested Chain
        private void TestNestedChain()
        {
            LogToUI("Testing: Nested Chain (Main Thread) Calling Level 1 -> 2 -> 3...");
            var finalTrace = Level1();
            LogToUI($"Full Chain Result:\n{finalTrace.ToProjectLink()}");
        }

        private TraceHandle Level1()
        {
            var trace = TraceHandle.Capture();
            return Level2(trace);
        }

        private TraceHandle Level2(TraceHandle prev)
        {
            var trace = TraceHandle.Capture(prev);
            return Level3(trace);
        }

        private TraceHandle Level3(TraceHandle prev)
        {
            return TraceHandle.Capture(prev);
        }
        #endregion
    }
}