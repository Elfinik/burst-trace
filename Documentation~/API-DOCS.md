## 📚 API Documentation: BurstTrace

A plugin for capturing and processing the Stack Trace, optimized for working inside the **Unity Burst Compiler** and standard C# code.

## 📌 Table of Contents  
  
- [BurstTrace](#class-bursttrace) — Global management.  
- [TraceHandle](#struct-tracehandle) — Main log structure.  
- [BurstTraceAdvanced](#class-bursttraceadvanced) — Manual registration and memory.  
- [BurstTraceCustomThreads](#class-bursttracecustomthreads) — Working with the plugin from System.Threading.Tasks.

---

## <a id="class-bursttrace"></a>Class: BurstTrace

`public static class BurstTrace`

Provides functionality for managing the logging system. Allows globally enabling or disabling stack capture.

### Methods

#### SetLogDisabled

C#

```CSharp
public static void SetLogDisabled(bool disabled = false)
```

Enables or disables the logging system.

- **Parameters:**
	- `disabled` (`bool`): If `true`, logging is disabled. If `false` (default), logging is enabled.

#### IsLogEnabled

C#

```CSharp
public static bool IsLogEnabled()
```

Checks the current logging status.

- **Returns:** `true` if logging is enabled. Returns `false` if it is disabled manually or globally for the current Build.

---

## <a id="struct-tracehandle"></a>Struct: TraceHandle

`public struct TraceHandle`

Represents a handle for a specific log record or log chain. This struct is **thread-safe** and can be passed between Burst Jobs and the Main Thread.

>[!WARNING] 
>Persistent Cross-session serialization is currently not supported.

### Capture Methods

#### Capture (Start New Chain)

C#

```CSharp
public static TraceHandle Capture(
    [CallerMemberName] string memberName = "",
    [CallerFilePath] string sourceFilePath = "",
    [CallerLineNumber] int sourceLineNumber = 0
)
```

Captures the current stack frame as the **start** of a new log chain.

- **Safety:** Main Thread, Burst Jobs/Parallel Jobs (Unity Job System).
- **Parameters:**
	- `memberName`, `sourceFilePath`, `sourceLineNumber`: Filled automatically by the compiler. **Do not pass values manually.**
- ****Returns:** A new `TraceHandle` representing the current frame.

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

Captures the current stack frame and **appends** it to an existing log chain.

- **Safety:** Main Thread, Burst Jobs/Parallel Jobs (Unity Job System).
- **Parameters:**
	- `prev`: Previous handle (`TraceHandle`) to continue.
	- `memberName`, `sourceFilePath`, `sourceLineNumber`: Filled automatically by the compiler.
- **Returns:** A new `TraceHandle` containing the updated chain.

### Formatting Methods

#### ToProjectLink

C#

```CSharp
public string ToProjectLink()
```

Returns the formatted log chain string using **relative paths** to files (relative to the project root).

- ⚠️ **Main Thread Only** (ignored inside Burst).

#### ToAbsolutePath

C#

```CSharp
public string ToAbsolutePath()
```

Returns the formatted log chain string using **absolute paths** to files.

- ⚠️ **Main Thread Only** (ignored inside Burst).

#### ToConsoleToken

C#

```CSharp
public FixedString128Bytes ToConsoleToken()
```

Generates a clickable hyperlink for the console in `FixedString128Bytes` format.

> [!TIP] 
> This method is designed for use inside Burst-jobs. Instead of formatting a heavy string with the entire stack, it creates a lightweight token. When clicking this token in the Unity Console, a special callback is triggered that expands and displays the full Stack Trace.

---

## <a id="class-bursttraceadvanced"></a>Class: BurstTraceAdvanced

> This is a class for advanced usage and is located in a separate namespace

`namespace Elfinik.BurstTrace.Internal`

`public static class BurstTraceAdvanced`

Class for extended usage, manual log registration, and memory monitoring.

### Manual Registration

#### RegisterLog (Start New)

Captures a frame as the start of a new chain using manually passed `FixedString` data.

#### RegisterLog (Append)

Appends a frame to an existing chain using manually passed `FixedString` data.

### Memory Management

#### GetTotalAllocatedMemory

C#

```CSharp
[BurstDiscard]
public static int GetTotalAllocatedMemory()
```

Returns the total memory volume (in bytes) allocated for the logging system.

- ⚠️ **Main Thread Only** (ignored inside Burst).

#### GetUsedMemory

C#

```CSharp
[BurstDiscard]
public static int GetUsedMemory()
```

Returns the memory volume (in bytes) that is already filled with logs.

- ⚠️ **Main Thread Only** (ignored inside Burst).

---

## <a id="class-bursttracecustomthreads"></a>Class: BurstTraceCustomThreads

> This is a class for advanced usage and is located in a separate namespace

`namespace Elfinik.BurstTrace.Internal`

`public static class BurstTraceCustomThreads`

Class for extended usage, allows using BurstTrace from any* multithreaded framework.

\* Theoretically, there are no obstacles for use in other frameworks, but currently only System.Threading.Tasks has been tested, and only partially.

#### GetMaxThreadsCount

C#

```CSharp
public static int GetMaxThreadsCount {get;}
```

Returns the maximum possible thread index value.

### Capture Methods

#### Capture (Start New Chain)

C#

```CSharp
public static TraceHandle Capture(int threadIndex, 
    [CallerMemberName] string memberName = "",
    [CallerFilePath] string sourceFilePath = "",
    [CallerLineNumber] int sourceLineNumber = 0
)
```

Captures the current stack frame as the **start** of a new log chain.

- **Safety:** Main Thread, Any C# Threads
- **Parameters:**
	- `threadIndex`: Unique Worker Thread Index.
	- `memberName`, `sourceFilePath`, `sourceLineNumber`: Filled automatically by the compiler. **Do not pass values manually.**
- **Returns:** A new `TraceHandle` representing the current frame.

- ⚠️ Passing a `threadIndex` exceeding the maximum number of available threads (`GetMaxThreadsCount`) will throw an exception.
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

Captures the current stack frame and **appends** it to an existing log chain.

- **Safety:** Main Thread, Any C# Threads
- **Parameters:**
    - `threadIndex`: Unique Worker Thread Index.
    - `prev`: Previous handle (`TraceHandle`) to continue.
    - `memberName`, `sourceFilePath`, `sourceLineNumber`: Filled automatically by the compiler.
- **Returns:** A new `TraceHandle` containing the updated chain.
    
- ⚠️ Passing a `threadIndex` exceeding the maximum number of available threads (`GetMaxThreadsCount`) will throw an exception.
---
