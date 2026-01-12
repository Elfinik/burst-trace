
> You can see an implementation example in `Samples > CustomThreadSample.cs` or `Samples > CustomThreads.unity`

**Attention! This part is not covered by unit tests yet, although it has internal safety checks.**

>[!WARNING] 
>Attention! The parallel use of JobSystem and your own threading system (in the context of BurstTrace) is currently not allowed due to index collisions! I will fix this in the next version, but if this function is critical for you, please create a request: I will add it as a priority.

**Warning:** This feature is experimental. It is NOT intended for the Unity Job System (use standard` TraceHandle.Capture `for Jobs). Use this only for custom threading frameworks (e.g., System.Threading.Tasks).


Use the special class `BurstTraceCustomThreads` in the `Elfinik.BurstTrace.Internal` namespace.

Instead of:
```CSharp
using Elfinik.BurstTrace
...
traceHandle = TraceHandle.Capture();
```

You must call:

```CSharp
using Elfinik.BurstTrace.Internal
...
traceHandle = BurstTraceCustomThreads.Capture(threadIndex);
//or
traceHandle = BurstTraceCustomThreads.Capture(threadIndex, traceHandle);
```

Your main task is to pass `threadIndex`. This is the index of your Thread. In the Main Thread, it equals zero. How to obtain it depends on your framework. If you run multiple tasks in parallel and try to call the function with the same Thread index, this will lead to a race condition and unforeseen consequences.

**Important:** The `threadIndex` value must be unique for each simultaneously executing Thread/Task to avoid a Race Condition.

To get the maximum number of Threads, call:
```CSharp
BurstTraceCustomThreads.GetMaxThreadsCount
```

> This value reflects the system's maximum capacity for parallel indices, not the exact number of hardware Threads.

The maximum value of `threadIndex` = `BurstTraceCustomThreads.GetMaxThreadsCount - 1`. It is also equal to `JobsUtility.ThreadIndexCount`. Usually, the maximum number of Threads = 20, so you can specify an index from 0 to 20.

If you run more than 20 parallel tasks, you can write to the developer by creating a request on GitHub, specifying your framework and the maximum number of parallel runs.

This function has an internal safety check: if your index is less than zero or greater than or equal to `BurstTraceCustomThreads.GetMaxThreadsCount`, you will receive an exception.

**Attention! This implementation has not been tested with all frameworks and may cause a conflict with the Unity safety system. If you encounter any error, please create a request on GitHub specifying the error, your framework, and example implementation code!**