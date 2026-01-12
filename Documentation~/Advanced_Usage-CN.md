# 进阶用法 (Advanced Usage)
## 通过脚本临时禁用日志记录
你可以临时禁用日志创建。这不会从构建（Build）中剔除代码，但会排除日志创建的执行，将其调用的影响几乎降至为零。为此，请调用：
```CSharp
BurstTrace.SetLogDisabled(true);
```
传入 `true` 以禁用日志，传入 `false` 以启用日志。你可以使用以下函数检查日志是否已启用：
```CSharp
BurstTrace.IsLogEnabled();
```
如果日志被禁用，或者你从发布构建（release Build）中排除了插件代码，该函数将返回 `false`。如果日志已启用，它将返回 `true`。
## 在发布版本中禁用 (Disabling in Release)
你可以通过勾选 `ProjectSettings > BurstTrace > Disable logs` 来从发布构建中完全排除代码。

这将排除日志创建代码，但不会使 `TraceHandle` 结构体为空。它仍然占用 4 个字节，但其值将始终为空。这样做是为了避免运行之间序列化和内存结构的理论问题。

创建代码本身将变成空方法，因此在 Burst 内部调用它不会有任何成本，因为 Burst 只会简单地将其剔除。在 Burst 外部调用也非常廉价，因为它只是对空方法的调用。
## 无需在调用处编写代码即可自动创建日志
如果你想创建一个自动记录调用行日志的函数，而不是手动调用并传递 `TraceHandle`，请使用以下模式：
```CSharp
    public void WriteDamageEvent(DamageEvent damageEvent, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
    {
        this.damageSenderTraceHandle = BurstTraceAdvanced.RegisterLog(damageEvent.burstTrace, memberName, sourceFilePath, sourceLineNumber);
    }
```
在这个例子中，`DamageEvent` 被传递给函数，但你也可以不传递任何内容。这里的关键是这 3 个参数：
```CSharp
[CallerMemberName] string memberName = "",
[CallerFilePath] string sourceFilePath = "",
[CallerLineNumber] int sourceLineNumber = 0
```
将它们添加到函数中，然后将它们传递给自定义日志注册调用：
```CSharp
        TraceHandle customTraceHandle = BurstTraceAdvanced.RegisterLog(memberName, sourceFilePath, sourceLineNumber);
```
你也可以将另一个 `TraceHandle` 作为第一个参数传递以创建一个链。然后像往常一样简单地调用你的函数：
```CSharp
        WriteDamageEvent(damageEvent);
```
在调用函数时，请勿将数据赋值给这些服务参数！不要手动将值传递给这些参数；编译器会自动填充它们。

因此，你可以（例如）创建一个函数，该函数在被调用时记录其被调用的位置，而在调用期间不需要编写额外的代码。你也可以在 Burst 内部和并行 Jobs 内部使用此功能。
> 注意：此方法比标准的 `TraceHandle.Capture()` 稍慢。仅当你认为在方法内部隐藏日志记录调用很重要时才使用它。它在性能上仅**略微**逊色。
## 输出日志以供进一步分析
如果你需要获取完整的日志字符串，请调用：
```CSharp
traceHandle.ToAbsolutePath();
```
这将返回日志链的逐行输出。

如果你调用：
```CSharp
traceHandle.ToProjectLink();
```
你将获得以下格式（取决于 Unity 版本）：
```CSharp
#if UNITY_6000_0_OR_NEWER
string link = $"<color=#40a0ff><link=\"href='{relativePath}' line='{lineNum}'\">{relativePath}:{lineNum}</link></color>";
#else
string link = $"<a href=\"{relativePath}\" line=\"{lineNum}\">{relativePath}:{lineNum}</a>";
#endif
```
>[!NOTE] 
>注意：在构建（Build）内部调用此函数实际上与调用 `ToAbsolutePath` 相同。由于构建中不存在脚本文件，因此不会添加超链接。你将看到正确的文件和行输出，但带有绝对路径且不可点击。
## 配置 ProjectSettings

你可以在 `ProjectSettings > BurstTrace` 菜单中配置插件的各个方面。

⚠️ Do not change the file name or path! It must have the path `Resources/Burst Trace Config.asset`
### Disable 64-hash optimization (禁用 64 位哈希优化)
>插件将字符串替换为哈希以进行优化。默认情况下，使用 64 位哈希。碰撞的几率微乎其微，但理论上是存在的。如果你遇到哈希碰撞（请在仓库中创建一个 Issue），请启用此选项。它会稍微降低性能并增加内存使用量，但会完全消除碰撞的可能性。

### Capture profiler (捕获分析器)
>勾选此框以在 Unity Profiler 中显示日志创建和记录的时刻。它也会显示在 Jobs 内部。这样你就可以找出在 Jobs 内部使用的确切成本。建议在发布版本中禁用此功能，因为写入 Profiler 理论上可能比创建日志本身更昂贵。

### Disable logs (禁用日志)
>这将从构建中排除日志创建代码，但不会使 `TraceHandle` 结构体为空。它仍然占用 4 个字节，但其值将始终为空。这样做是为了避免运行之间序列化和内存结构的理论问题。创建代码本身将变成空方法，因此在 Burst 内部调用它不会有任何成本，因为 Burst 只会简单地将其剔除。在 Burst 外部调用也非常廉价，因为它只是对空方法的调用。

