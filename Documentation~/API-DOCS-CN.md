## 📚 API 文档: BurstTrace

一个用于捕获和处理堆栈跟踪 (Stack Trace) 的插件，专为在 **Unity Burst Compiler** 和标准 C# 代码中工作而优化。

## 📌 目录  
  
- [BurstTrace](#class-bursttrace) — 全局管理。
- [TraceHandle](#struct-tracehandle) — 主要日志结构。
- [BurstTraceAdvanced](#class-bursttraceadvanced) — 手动注册和内存管理。
- [BurstTraceCustomThreads](#class-bursttracecustomthreads) — 在 System.Threading.Tasks 中使用插件。

---

## <a id="class-bursttrace"></a>Class: BurstTrace

`public static class BurstTrace`

提供管理日志系统的功能。允许全局启用或禁用堆栈捕获。

### 方法 (Methods)

#### SetLogDisabled

C#

```CSharp
public static void SetLogDisabled(bool disabled = false)
```

启用或禁用日志系统。

- **参数:**
    - `disabled` (`bool`): 如果为 `true`，则禁用日志记录。如果为 `false` (默认)，则启用日志记录。

#### IsLogEnabled

C#

```CSharp
public static bool IsLogEnabled()
```

检查当前的日志记录状态。

- **返回:** 如果启用了日志记录，则返回 `true`。如果是手动禁用的或在当前构建 (Build) 中全局禁用的，则返回 `false`。

---

## <a id="struct-tracehandle"></a>Struct: TraceHandle

`public struct TraceHandle`

表示特定日志记录或日志链的句柄 (handle)。此结构是 **线程安全** 的，可以在 Burst Jobs 和 Main Thread (主线程) 之间传递。

>[!WARNING] 
>目前不支持跨会话的持久化序列化 (Persistent Cross-session serialization)。

### 捕获方法 (Capture Methods)

#### Capture (Start New Chain)

C#

```CSharp
public static TraceHandle Capture(
    [CallerMemberName] string memberName = "",
    [CallerFilePath] string sourceFilePath = "",
    [CallerLineNumber] int sourceLineNumber = 0
)
```

捕获当前堆栈帧作为新日志链的 **起点 (start)**。

- **安全性:** Main Thread, Burst Jobs/Parallel Jobs (Unity Job System).
- **参数:**
    - `memberName`, `sourceFilePath`, `sourceLineNumber`: 由编译器自动填充。**请勿手动传递值。**
- **返回:** 一个表示当前帧的新 `TraceHandle`。

#### Capture (Append to Chain)

C#

```CSharp
public static TraceHandle Capture(
    TraceHandle prev,
    [CallerMemberName] string memberName = "",
    [CallerFilePath] string sourceFilePath = "",
    [CallerLineNumber] int sourceLineNumber = 0
)
```

捕获当前堆栈帧并将其 **追加 (append)** 到现有的日志链中。

- **安全性:** Main Thread, Burst Jobs/Parallel Jobs (Unity Job System).
- **参数:**
    - `prev`: 要继续的上一个句柄 (`TraceHandle`)。
    - `memberName`, `sourceFilePath`, `sourceLineNumber`: 由编译器自动填充。
- **返回:** 包含更新后链条的新 `TraceHandle`。

### 格式化方法 (Formatting Methods)

#### ToProjectLink

C#

```CSharp
public string ToProjectLink()
```

返回格式化后的日志链字符串，使用文件的 **相对路径** (相对于项目根目录)。

- ⚠️ **仅限 Main Thread** (在 Burst 内部会被忽略)。

#### ToAbsolutePath

C#

```CSharp
public string ToAbsolutePath()
```

返回格式化后的日志链字符串，使用文件的 **绝对路径**。

- ⚠️ **仅限 Main Thread** (在 Burst 内部会被忽略)。

#### ToConsoleToken

C#

```CSharp
public FixedString128Bytes ToConsoleToken()
```

以 `FixedString128Bytes` 格式生成控制台可点击的超链接。

> [!TIP] 
> 此方法专为在 Burst-jobs 内部使用而设计。它创建一个轻量级的 token，而不是格式化包含整个堆栈的繁重字符串。在 Unity 控制台中点击此 token 时，会触发一个特殊的回调，展开并显示完整的 Stack Trace。

---

## <a id="class-bursttraceadvanced"></a>Class: BurstTraceAdvanced

> 这是一个用于进阶用法的类，位于单独的命名空间中。

`namespace Elfinik.BurstTrace.Internal`

`public static class BurstTraceAdvanced`

用于扩展用法、手动日志注册和内存监控的类。

### 手动注册 (Manual Registration)

#### RegisterLog (Start New)

使用手动传递的 `FixedString` 数据捕获帧作为新链的起点。

#### RegisterLog (Append)

使用手动传递的 `FixedString` 数据将帧追加到现有链中。

### 内存管理 (Memory Management)

#### GetTotalAllocatedMemory

C#

```CSharp
[BurstDiscard]
public static int GetTotalAllocatedMemory()
```

返回为日志系统分配的总内存量 (以字节为单位)。

- ⚠️ **仅限 Main Thread** (在 Burst 内部会被忽略)。

#### GetUsedMemory

C#

```CSharp
[BurstDiscard]
public static int GetUsedMemory()
```

返回已被日志填充的内存量 (以字节为单位)。

- ⚠️ **仅限 Main Thread** (在 Burst 内部会被忽略)。

---

## <a id="class-bursttracecustomthreads"></a>Class: BurstTraceCustomThreads

> 这是一个用于进阶用法的类，位于单独的命名空间中。

`namespace Elfinik.BurstTrace.Internal`

`public static class BurstTraceCustomThreads`

用于扩展用法的类，允许从任何* 多线程框架中使用 BurstTrace。

\* 理论上在其他框架中使用没有障碍，但目前仅对 System.Threading.Tasks 进行了测试，且仅为部分测试。

#### GetMaxThreadsCount

C#

```CSharp
public static int GetMaxThreadsCount {get;}
```

返回可能的最大线程索引值。

### 捕获方法 (Capture Methods)

#### Capture (Start New Chain)

C#

```CSharp
public static TraceHandle Capture(int threadIndex, 
    [CallerMemberName] string memberName = "",
    [CallerFilePath] string sourceFilePath = "",
    [CallerLineNumber] int sourceLineNumber = 0
)
```

捕获当前堆栈帧作为新日志链的 **起点 (start)**。

- **安全性:** Main Thread, 任意 C# Threads
- **参数:**
    - `threadIndex`: 唯一的工作线程索引 (Worker Thread Index)。
    - `memberName`, `sourceFilePath`, `sourceLineNumber`: 由编译器自动填充。**请勿手动传递值。**
- **返回:** 一个表示当前帧的新 `TraceHandle`。
- ⚠️ 传递超过最大可用线程数 (`GetMaxThreadsCount`) 的 `threadIndex` 将抛出异常。
#### Capture (Append to Chain)

C#

```CSharp
public static TraceHandle Capture(int threadIndex, 
    TraceHandle prev,
    [CallerMemberName] string memberName = "",
    [CallerFilePath] string sourceFilePath = "",
    [CallerLineNumber] int sourceLineNumber = 0
)
```

捕获当前堆栈帧并将其 **追加 (append)** 到现有的日志链中。

- **安全性:** Main Thread, 任意 C# Threads
- **参数:**
    - `threadIndex`: 唯一的工作线程索引 (Worker Thread Index)。
    - `prev`: 要继续的上一个句柄 (`TraceHandle`)。
    - `memberName`, `sourceFilePath`, `sourceLineNumber`: 由编译器自动填充。
- **返回:** 包含更新后链条的新 `TraceHandle`。
- ⚠️ 传递超过最大可用线程数 (`GetMaxThreadsCount`) 的 `threadIndex` 将抛出异常。
---
