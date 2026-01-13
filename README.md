![Header.png](Documentation~/Images/Header.png)
![Unity 2022.3+](https://img.shields.io/badge/Unity-2022.3%2B-black?logo=unity) ![Version](https://img.shields.io/badge/version-v1.0.0-blue) ![License: MIT](https://img.shields.io/badge/License-MIT-g.svg)  [![openupm](https://img.shields.io/npm/v/com.elfinik.burst-trace?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.elfinik.burst-trace/)

- [Quick Start](#quick-start)
- [Performance Tests](#performance-tests)
- [ECS Integration (Optional)](#ecs-integration)
- [Documentation (Advanced Usage)](Documentation~/Advanced_Usage.md)
- [API](Documentation~/API-DOCS.md)
- [Samples (Read to run samples on Unity 6+)](Documentation~/Samples.md)
- [Usage in System.Threading.Tasks](Documentation~/Multithreading-No-Unity-Jobs.md)
# Translations
>Translations into other languages ​​may contain outdated information
- [中文](README-CN.md)
- [日本語](README-JP.md)
- [На Русском](README-RU.md)
# Quick Start

<details>
<summary>Installation</summary>

## Via Package Manager
1. Open `Window/Package Manager`.
2. Click the `+` button in the top-left corner.
3. Select `Add package from git URL...`.

![PM_1.png](Documentation~/Images/PM_1.png)

4. Enter the repository URL:

```
https://github.com/Elfinik/burst-trace.git
```

![PM_2.png](Documentation~/Images/PM_2.png)

5. Click `Add`.

## Via OpenUPM 
The package is available on the [openupm registry](https://openupm.com/packages/com.elfinik.burst-trace). It's recommended to install it via [openupm-cli](https://github.com/openupm/openupm-cli).

```
openupm add com.elfinik.burst-trace
```

## Or via `manifest.json`
Open `Packages/manifest.json` in your project folder and add the following line to the `dependencies` scope:

```
"com.elfinik.burst-trace": "https://github.com/Elfinik/burst-trace.git"
```
---

</details>

Add the namespace to the beginning of the script
```CSharp
using Elfinik.BurstTrace;
```
To capture the stack, call the method:
```CSharp
traceHandle = TraceHandle.Capture();
```
This returns a `TraceHandle` struct. You can pass and store it anywhere. In the MonoBehaviour Inspector (GameObject / ScriptableObject) or in the Entity Inspector, you will see the call chain from this variable:
![Header.png](Documentation~/Images/Screen_SingleLine.png)
> Each log is clickable: you can click on the line, and the script will open at the specific line. Sometimes in Unity, a double or triple click is required to follow the hyperlink. You can also copy logs (see screenshot below).

![Header.png](Documentation~/Images/Screen_Copy.png)

If the variable is empty, you will see `Empty (Invalid) log`. Currently, the variable holds one line. If you want to record a path of multiple lines, simply call the function passing the previous value:
```CSharp
traceHandle = TraceHandle.Capture(traceHandle);
//or
traceHandle = TraceHandle.Capture(prevTraceHandle);
```
Now you will see 2 lines. You can store as many lines as you like: `TraceHandle` will always occupy 4 bytes. **But note: only up to 64 lines are shown during display and output! This is done for optimization and to prevent infinite loops.** If you want to output the call chain to the console, call:
```CSharp
Debug.Log(traceHandle.ToProjectLink());
```
This will output the same text to the console as in the Inspector. It can also be clicked to open the file.
>[!NOTE] 
>Remember that the console window does not allow clicking hyperlinks in the log list. You must first select the log, and then click the text in the bottom panel.

If for some reason you need to output the log to the console inside Burst code, call:
```CSharp
Debug.Log(traceHandle.ToConsoleToken());
```
This function returns a `FixedString128Bytes` with a hyperlink inside. `CLICK TO PRINT LOG` will be output to the console. Clicking the hyperlink will print the log with the full call chain.
>[!TIP] 
>If you stop PlayMode, you can still retrieve the link and see logs in the Inspector **until a new PlayMode starts**. After restarting, all logs from the previous run will be invalid, and their output undefined.

# Work in Build
BurstTrace works in both the Editor and Builds. In Builds, the full **path** to files is displayed instead of hyperlinks. If necessary, you can send this data to an analytics or logging system.
# Are there code generators and dll?
No. All code is fully available and does not use any generators itself (however, it uses standard C# compiler functions to obtain the call path during compilation).
# Limitations
- **Initialization:** Occurs after `Awake` and before `Start` of the first loaded scene.
- **Unique records limit:** 1,048,575.
    * A unique record is considered a function call in a script file. If you call a function multiple times in a loop, but it is the same line in the script, it counts as one unique record.
- **Nested records limit:** +1,048,575.
    - Unique nested log: this is specifically a unique chain of logs. Meaning, if creating a chain of 15 logs inside a loop, 1 unique log and 15 unique nested logs will be created.
- Maximum number of Threads: 1 main + 2047 (standard Unity is limited to 128, so this number provides a margin for custom frameworks).
- You can call recording from anywhere (Main Thread, Burst, Multithreaded Jobs): this will not affect performance. However, you cannot call functions safely if using a custom framework for thread management. If you are not using JobSystem, see the 'Custom Multithread framework' section.
    
- **Serialization:** Currently, serialization of `TraceHandle` between sessions is not supported. Upon application restart, old `TraceHandle`s become invalid.
# Usage in System.Threading.Tasks
If you want to use BurstTrace multithreaded outside of the Unity JobSystem (e.g., C# Threading), follow the separate instructions: [Usage outside Unity Jobs](Documentation~/Multithreading-No-Unity-Jobs.md)

# Comparison with Debug.Log and StackTrace
BurstTrace: This is an extremely fast way to create logs that can be stored and work anywhere. However, in return, it cannot automatically create the entire call chain.

|                 | Performance                                                                            | Burst                                           | StackTrace                        |
| --------------- | -------------------------------------------------------------------------------------- | ----------------------------------------------- | --------------------------------- |
| BurstTrace      | Very fast                                                                              | Fully compatible                                | Must record each line             |
| Debug.Log       | Approx. 200x slower, but fast for single calls                                         | Partially compatible                            | Outputs full StackTrace to log    |
| StackTrace (C#) | Very slow. Requires calling `ToString()`, which is also a resource-intensive operation | Not compatible (Managed Object / Non-Blittable) | Stores full StackTrace internally |

To summarize:
- If you need full log output to the console and do not need to store it in history, simply call `Debug.Log`: it is not as fast, but it immediately captures the entire StackTrace.
- If you need to store the full StackTrace inside managed C# code, you can use `StackTrace`. It is slow but immediately records and stores the entire StackTrace. Since it is a reference type (class), frequent instantiation creates load on the Garbage Collector (GC) and leads to delayed performance spikes.
- If you need to save a chain or call site inside Burst code: only BurstTrace is suitable.
- If you want to save a chain or call site in Entities (Unity ECS), only BurstTrace is suitable.
- If you need maximum performance in regular C# code, and you can call a function at every call you need to record the StackTrace, use BurstTrace. It provides a colossal performance boost when recording thousands of logs.

# Performance:
Calling inside standard Unity tools (Main Thread or JobSystem) is fully thread-safe. There are no race conditions, no Interlock calls, and all code is Burst-compatible.

One record takes approximately: 0.002 - 0.005ms (Unity safety checks enabled). And ~0.000 in Build (Burst)

Console output is a very fast, but still a debug function.

# Performance Tests
Calling 500 logging events (result in ms):

|                                 | Min   | Median | Max   |
| ------------------------------- | ----- | ------ | ----- |
| Standard Debug.Log              | 177   | 274    | 552   |
| Debug.Log (Burst Job)*          | 488   | 664    | 1316  |
| BurstTrace (Mono)               | 0.88  | 0.92   | 1.17  |
| BurstTrace (Burst Job)          | 0.69  | 0.74   | 0.89  |
| BurstTrace (Parallel Job)       | 0.7   | 0.72   | 0.93  |
| BurstTrace (Parallel Job x32)** | 28.38 | 23.61  | 22.25 |

Instead of calling BurstTrace log 500 times, the tests create a chain of 500 logs: meaning instead of using cached values, every new log is a new one, creating a chain.

\* The result varies significantly from test to test: maximum approx. from 800 to 1400 ms.

\** Since the log call is quite fast, job scheduling itself has a significant impact on tests. Therefore, the last test is run with an iteration count 32 times higher than the others.

![Header.png](Documentation~/Images/PerfTest_1.png)
![Header.png](Documentation~/Images/PerfTest_2.png)

The load in `Debug.Log` tests is also caused by log output to the console, which obviously adds a huge load.

# Memory
Although the `TraceHandle` pointer takes only 4 bytes, the string itself needs to be stored as text.

One unique record consumes on average from 1 to 20 KB.
> If you create a log from the Main Thread or only once, the record will occupy 1 KB. If you create a log from a multithreaded Job, and do it many times, it may occupy up to 20 KB, or more on devices with a very large number of Threads.

One unique nested log record (chain) also occupies approximately from 16 to 320 bytes.

By default, 20MB of memory is allocated, which should be enough to record 1024 log lines. You can change this value (e.g., decrease for mobile platforms) in `ProjectSettings > BurstTrace`.

# Unit Tests
![Unit_Tests.png](Documentation~/Images/Unit_Tests.png)

You can view and execute unit tests in the project. Note: sometimes a test may run indefinitely. Simply cancel it and run it again. This seems to be a bug related to running the JobSystem inside unit tests. Usually, you see a Warning message in the console in this case.

Some tests are heavy and may cause the Editor to freeze for a few seconds (or minutes if the device is very weak).

# ECS Integration

| ![ECS Inspector](Documentation~/Images/ECS_Inspector.png) | **Automatic support:** You can use this package with ECS without additional settings. `TraceHandle` displays correctly in the Entity Inspector.<br> |
| --------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
# License (MIT)
This project is licensed under the **MIT License**.

You are free to use this library in personal and commercial projects.

**⚠️ Restriction on Resale:** While the MIT license permits commercial use, **you are not allowed to resell this source code as a standalone asset** (e.g., on the Unity Asset Store or similar platforms) without substantial modification or added value. The intent of this license is to allow developers to use the tool in their games/apps, not to enable asset flipping.

See the [LICENSE](LICENSE) file for the full text.
# Credits
**Core Implementation** The core code is **hand-written** and based on my personal R&D over the last year. It utilizes internal APIs from `Unity.Collections` and `Unity.Entities` (for the Inspector integration).

**AI Assistance** The following components were generated with the assistance of **Google Gemini** (with manual review and refinement):

- Unit Tests
- Demo Scripts
- Documentation & Code Comments
- Logo design