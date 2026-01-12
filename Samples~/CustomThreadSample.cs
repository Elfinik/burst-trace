using System.Threading.Tasks;
using Elfinik.BurstTrace.Internal;
using UnityEngine;

namespace Elfinik.BurstTrace.Samples
{
    public class CustomThreadSample : MonoBehaviour
    {
        public TraceHandle[] results = new TraceHandle[10];

        async void Start()
        {
            results = new TraceHandle[10];
            Debug.Log("Launching threads...");

            await RunMultipleThreads(10);

            Debug.Log("All threads have completed. The array is full!");
            foreach (var res in results)
            {
                Debug.Log($"Result: {res.ToProjectLink()}");
            }
        }

        async Task RunMultipleThreads(int count)
        {
            Task<TraceHandle>[] tasks = new Task<TraceHandle>[count];

            for (int i = 0; i < count; i++)
            {
                int threadIndex = i; // Local variable for closure

                // We run the task in a background thread.
                tasks[i] = Task.Run(() =>
                {
                    // Imitation of complex work
                    TraceHandle result = ComplexCalculation(threadIndex);
                    return result;
                });
            }

            // We wait for all tasks to complete without blocking the main Unity thread.
            TraceHandle[] completedResults = await Task.WhenAll(tasks);

            // We are transferring the data to our main array (this code will be executed on the Main Thread).
            for (int i = 0; i < completedResults.Length; i++)
            {
                results[i] = completedResults[i];
            }
        }

        TraceHandle ComplexCalculation(int index)
        {
            // This code is executed OUTSIDE the main thread.
            System.Threading.Thread.Sleep(500);
            int threadIndex = index;
            var res = BurstTraceCustomThreads.Capture(threadIndex);
            res = BurstTraceCustomThreads.Capture(threadIndex, res);
            return res;
        }
    }
}