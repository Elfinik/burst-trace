![Header.png](Documentation~/Images/Header.png)
- [快速入门 (Quick Start)](#快速入门)
- [性能测试 (Performance Tests)](#性能测试)
- [ECS 集成 (可选)](#ecs-集成)
- [文档 (进阶用法)](Documentation~/Advanced_Usage-CN.md)
- [API](Documentation~/API-DOCS-CN.md) 
- [示例 (Unity 6+ 运行说明)](Documentation~/Samples-CN.md)
- [在 System.Threading.Tasks 中使用](Documentation~/Multithreading-No-Unity-Jobs-CN.md)

- [English](README.md)
>中文文档由 AI (Gemini) 自动翻译生成，如有疏漏或歧义，请以英文原版为准。
# 快速入门

<details>
<summary>Installation</summary>

## 通过包管理器 (Via Package Manager)
- 打开菜单 `Window` > `Package Manager`。
- 点击左上角的 `+` 按钮。
- 选择 `Add package from git URL...`。

![PM_1.png](Documentation~/Images/PM_1.png)

- 输入仓库链接：

```
https://github.com/Elfinik/burst-trace.git
```

![PM_2.png](Documentation~/Images/PM_2.png)
- 点击 `Add`。

## Via OpenUPM 
The package is available on the [openupm registry](https://openupm.com/packages/com.elfinik.burst-trace). It's recommended to install it via [openupm-cli](https://github.com/openupm/openupm-cli).

```
openupm add com.elfinik.burst-trace
```

## 或者通过 `manifest.json` (Or via manifest.json)
项目文件夹中打开 `Packages/manifest.json`，并将以下行添加到 `dependencies` 范围中：

```
"com.elfinik.burst-trace": "https://github.com/Elfinik/burst-trace.git"
```
---

</details>

在脚本开头添加命名空间：
```CSharp
using Elfinik.BurstTrace;
```
要捕获堆栈，请调用以下方法：
```CSharp
traceHandle = TraceHandle.Capture();
```
这将返回一个 `TraceHandle` 结构体。你可以随意传递和存储它。 在 MonoBehaviour 检视面板 (GameObject / ScriptableObject) 或 Entity 检视面板中，你将看到来自该变量的调用链：
![Header.png](Documentation~/Images/Screen_SingleLine.png)
> 每条日志都是可点击的：你可以点击该行，脚本将在相应的行打开。 有时在 Unity 中，需要双击或三击才能跳转超链接。你也可以复制日志（见下图）。

![Header.png](Documentation~/Images/Screen_Copy.png)

如果变量为空，你将看到 `Empty (Invalid) log`。 目前变量中只有一行。如果你想记录多行路径，只需调用函数并传入上一个值：
```CSharp
traceHandle = TraceHandle.Capture(traceHandle);
//或者
traceHandle = TraceHandle.Capture(prevTraceHandle);
```
现在你将看到 2 行。你可以存储任意数量的行：`TraceHandle` 始终只占用 4 个字节。 **但请注意：在显示和输出时，最多只显示 64 行！这是为了优化并防止无限循环。** 如果你想将调用链输出到控制台，请调用：
```CSharp
Debug.Log(traceHandle.ToProjectLink());
```
这将在控制台输出与检视面板中相同的文本。点击它也可以打开文件。
>[!NOTE] 
>请记住，控制台窗口不允许直接点击日志列表中的超链接。你需要先选中日志，然后在底部面板中点击文本。

如果由于某种原因，你需要在 Burst 代码内部将日志输出到控制台，请调用：
```CSharp
Debug.Log(traceHandle.ToConsoleToken());
```
此函数返回一个包含超链接的 `FixedString128Bytes`。控制台将输出 `CLICK TO PRINT LOG`。点击超链接将打印包含完整调用链的日志。
>[!TIP]
> 如果你停止了 PlayMode（播放模式），你仍然可以获取链接并在检视面板中查看日志，**直到下一次启动 PlayMode**。重新启动后，上一次运行的所有日志都将失效，其输出结果未定义。

# 在构建中工作 (Build)
BurstTrace 既可以在编辑器中工作，也可以在构建版本（Builds）中工作。在构建版本中，将显示文件的**完整路径**而不是超链接。如有必要，你可以将这些数据发送到分析或日志系统。
# 有代码生成器和 DLL 吗？
没有。所有代码完全开源，且本身不使用任何生成器（但它使用标准的 C# 编译器功能在编译时获取调用路径）。
# 限制
- **初始化：** 发生在 `Awake` 之后，第一个加载场景的 `Start` 之前。
- **唯一记录限制：** 1,048,575。
    * 唯一记录是指脚本文件中的函数调用。如果你在循环中多次调用函数，但在脚本中它是同一行，则算作一条唯一记录。
- **嵌套记录限制：** +1,048,575。
    - 唯一嵌套日志：特指唯一的日志链。也就是说，如果在循环内部创建一个包含 15 条日志的链，将创建 1 个唯一日志和 15 个唯一嵌套日志。
- 最大线程数：1 个主线程 + 2047（标准 Unity 限制为 128，所以这个数字为自定义框架留有了余量）。
- 你可以从任何地方调用记录（主线程、Burst、多线程 Jobs）：这对性能没有影响。但是，如果你使用自定义框架进行线程管理，则无法安全地调用函数。如果你不使用 JobSystem，请参阅“自定义多线程框架”部分。
- **序列化：** 目前不支持会话之间的 `TraceHandle` 序列化。应用程序重新启动后，旧的 `TraceHandle` 将失效。
# 在 System.Threading.Tasks 中使用
如果你想在 Unity JobSystem 之外（例如 C# Threading）多线程使用 BurstTrace，请遵循单独的说明：[在 Unity Jobs 之外使用](Documentation~/Multithreading-No-Unity-Jobs.md)

# 与 Debug.Log 和 StackTrace 的比较：
BurstTrace：这是一种极快的创建日志的方法，日志可以存储并在任何地方工作。但在作为交换，它不能自动创建整个调用链。

|                 | 性能 (Performance)                   | Burst     | StackTrace           |
| --------------- | ---------------------------------- | --------- | -------------------- |
| BurstTrace      | 非常快                                | 完全兼容      | 需要记录每一行              |
| Debug.Log       | 大约慢 200 倍，但在单次调用时也很快               | 部分兼容      | 在日志中输出完整的 StackTrace |
| StackTrace (C#) | 非常慢。需要调用 `ToString()`，这也是一项资源密集型操作 | 不兼容 (托管类) | 内部存储完整的 StackTrace   |

总结：
- 如果你需要将完整的日志输出到控制台，并且不需要将其存储在历史记录中，只需调用 Debug.Log：它没那么快，但它会立即捕获整个 StackTrace。
- 如果你需要在托管 C# 代码中存储完整的 StackTrace，你可以使用 StackTrace。它很慢，但会立即记录并存储整个 StackTrace。由于它是引用类型（class），频繁实例化会给垃圾回收器（GC）带来负载，并导致延迟的性能峰值。
- 如果你需要在 Burst 代码内部保存调用链或调用位置：只有 BurstTrace 适合你。
- 如果你想在实体（Unity ECS）中保存调用链或调用位置，只有 BurstTrace 适合你。
- 如果你需要在普通 C# 代码中获得最大性能，并且可以在每个需要的调用处调用函数来记录 StackTrace，请使用 BurstTrace。在记录数千条日志时，它将提供巨大的性能提升。
# 性能：
在标准 Unity 工具（主线程或 JobSystem）中调用是完全线程安全的。调用时没有竞争条件，没有 Interlock 调用，所有代码都与 Burst 兼容。

一条记录大约占用：0.002 - 0.005ms（启用了 Unity 安全检查）。

控制台输出非常快，但仍然是一个调试功能。

# 性能测试
调用 500 次日志事件（结果以 ms 为单位）：

|                                 | Min   | Median | Max   |
| ------------------------------- | ----- | ------ | ----- |
| Standard Debug.Log              | 177   | 274    | 552   |
| Debug.Log (Burst Job)*          | 488   | 664    | 1316  |
| BurstTrace (Mono)               | 0.88  | 0.92   | 1.17  |
| BurstTrace (Burst Job)          | 0.69  | 0.74   | 0.89  |
| BurstTrace (Parallel Job)       | 0.7   | 0.72   | 0.93  |
| BurstTrace (Parallel Job x32)** | 28.38 | 23.61  | 22.25 |

在测试中，不是调用 BurstTrace 日志 500 次，而是创建了一个由 500 条日志组成的链：也就是说，不使用缓存值，每条新日志都是新的，从而创建了一个链。

\* 结果因测试而异，波动很大：最大值大约在 800 到 1400 ms 之间。

\** 由于日志调用相当快，任务调度本身对测试有显著影响。因此，最后一个测试的迭代次数是其他的 32 倍。

![Header.png](Documentation~/Images/PerfTest_1.png)
![Header.png](Documentation~/Images/PerfTest_2.png)

Debug.Log 测试中的负载也是由于将日志输出到控制台造成的，这显然增加了巨大的负载。

# 内存
虽然 `TraceHandle` 指针本身只占用 4 个字节，但字符串本身需要以文本形式存储。

一条唯一记录平均消耗 1 到 20 KB。
> 如果你从主线程创建日志或只创建 1 次，记录将占用 1 KB。 如果你从多线程 Job 中创建日志，并且执行多次，它可能占用高达 20 KB，或者在拥有大量线程的设备上占用更多。

一条唯一嵌套日志记录（链）也大约占用 16 到 320 字节。

默认情况下分配 20MB 内存，这应该足以记录 1024 行日志。你可以在 `ProjectSettings > BurstTrace` 中更改此值（例如针对移动平台减小该值）。

# 单元测试
![Unit_Tests.png](Documentation~/Images/Unit_Tests.png)

你可以在项目中查看并执行单元测试。注意：有时测试可能会无限期运行。只需取消并重新运行即可。这似乎是与在单元测试中运行 JobSystem 相关的 bug。通常在这种情况下，你会在控制台中看到 Warning 消息。

有些测试很重，可能会导致编辑器卡顿几秒钟（如果设备非常弱，可能会卡顿几分钟）。

# ECS 集成

| ![ECS Inspector](Documentation~/Images/ECS_Inspector.png) | **自动支持：** 你可以在 ECS 中使用此包而无需额外设置。`TraceHandle` 可以正确显示在 Entity 检视面板中。<br> |
| --------------------------------------------------------- | ----------------------------------------------------------------------- |
# 许可证 (MIT) / License (MIT)
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