
> 您可以在 `Samples > CustomThreadSample.cs` 或 `Samples > CustomThreads.unity` 中查看实现示例。

**注意！这部分尚未包含在单元测试中，尽管它具有内部安全检查。**

>[!WARNING] 
>注意！目前不允许并行使用 JobSystem 和您自己的线程系统（在 BurstTrace 上下文中），因为会导致索引冲突！我将在下一个版本中修复此问题，但如果此功能对您至关重要，请提交请求：我会优先添加。

**警告：** 此功能是实验性的。它**不**适用于 Unity Job System（Job 请使用标准的 `TraceHandle.Capture`）。仅将其用于自定义线程框架（例如 System.Threading.Tasks）。


使用 `Elfinik.BurstTrace.Internal` 命名空间中的特殊类 `BurstTraceCustomThreads`。

代替：
```CSharp
using Elfinik.BurstTrace
...
traceHandle = TraceHandle.Capture();
```

您必须调用：

```CSharp
using Elfinik.BurstTrace.Internal
...
traceHandle = BurstTraceCustomThreads.Capture(threadIndex);
//or
traceHandle = BurstTraceCustomThreads.Capture(threadIndex, traceHandle);
```

您的主要任务是传递 `threadIndex`。这是您的线程索引。在主线程中，它为零。如何获取它取决于您的框架。如果您并行运行多个任务并尝试使用相同的线程索引调用该函数，将导致竞争条件和不可预见的后果。

**重要：** 对于每个同时执行的线程/任务，`threadIndex` 值必须是唯一的，以避免竞争条件 (Race Condition)。

要获取最大线程数，请调用：
```CSharp
BurstTraceCustomThreads.GetMaxThreadsCount
```

> 该值反映了系统对并行索引的最大容量，而不是硬件线程的确切数量。

`threadIndex` 的最大值 = `BurstTraceCustomThreads.GetMaxThreadsCount - 1`。它也等于 `JobsUtility.ThreadIndexCount`。通常最大线程数 = 20，因此您可以指定 0 到 20 之间的索引。

如果您运行超过 20 个并行任务，您可以通过在 GitHub 上创建请求来联系开发者，说明您的框架和最大并行运行数量。

此函数具有内部安全检查：如果您的索引小于零或大于等于 `BurstTraceCustomThreads.GetMaxThreadsCount`，您将收到异常。

**注意！此实现尚未在所有框架中进行测试，可能会导致与 Unity 安全系统发生冲突。如果您遇到任何错误，请在 GitHub 上创建请求，并说明错误、您的框架和示例实现代码！**