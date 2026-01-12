# Advanced Usage
## Temporarily Disabling Logging via Script
You can temporarily disable log creation. This will not strip the code from the Build, but it will exclude log creation execution, reducing the impact of their call to almost zero. To do this, call:
```CSharp
BurstTrace.SetLogDisabled(true);
```
Pass `true` to disable logs, pass `false` to enable logs. You can check if logs are enabled using the function:
```CSharp
BurstTrace.IsLogEnabled();
```
If logs are disabled OR you excluded the plugin code from the release Build, the function returns `false`. If logs are enabled, it returns `true`.
## Disabling in Release
You can completely exclude the code from the release Build by checking `ProjectSettings > BurstTrace > Disable logs`.

This will exclude log creation code but will not make the `TraceHandle` struct empty. It will still occupy 4 bytes, but its value will always be empty. This is done to avoid theoretical issues with serialization and memory structure between runs.

The creation code itself will turn into empty methods, so calling it inside Burst will cost nothing, as Burst will simply strip it. Calling outside Burst is also extremely cheap, as it is just a call to an empty method.
## Automatic Log Creation Without Writing Code at the Call Site
If you want to create a function that automatically creates a log of the calling line instead of manually calling and passing `TraceHandle`, use the following pattern:
```CSharp
    public void WriteDamageEvent(DamageEvent damageEvent, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
    {
        this.damageSenderTraceHandle = BurstTraceAdvanced.RegisterLog(damageEvent.burstTrace, memberName, sourceFilePath, sourceLineNumber);
    }
```
In this example, `DamageEvent` is passed to the function, but you can also pass nothing. The main thing here is the 3 arguments:
```CSharp
[CallerMemberName] string memberName = "",
[CallerFilePath] string sourceFilePath = "",
[CallerLineNumber] int sourceLineNumber = 0
```
Add them to the function, and then pass them to the custom log registration call:
```CSharp
        TraceHandle customTraceHandle = BurstTraceAdvanced.RegisterLog(memberName, sourceFilePath, sourceLineNumber);
```
You can also pass another `TraceHandle` as the first argument to create a chain. Then simply call your function as usual:
```CSharp
        WriteDamageEvent(damageEvent);
```
**DO NOT ASSIGN DATA TO THESE SERVICE ARGUMENTS WHEN CALLING THE FUNCTION! Do not pass values to these arguments manually; the compiler will fill them automatically.** 

Thus, you can, for example, create a function that, when called, records the place from which it was called without requiring additional code during its call. You can also use this inside Burst and inside parallel Jobs.
> Note: this method is slightly slower than the standard `TraceHandle.Capture()`. Use it only if it is important for you to hide the logging call inside the method. It is only **slightly** inferior in performance.
## Outputting Logs for Further Analysis
If you need to get the full log string, call:
```CSharp
traceHandle.ToAbsolutePath();
```
This returns a line-by-line output of the log chain. 

If you call:
```CSharp
traceHandle.ToProjectLink();
```
You get the following format (depends on Unity version):
```CSharp
#if UNITY_6000_0_OR_NEWER
string link = $"<color=#40a0ff><link=\"href='{relativePath}' line='{lineNum}'\">{relativePath}:{lineNum}</link></color>";
#else
string link = $"<a href=\"{relativePath}\" line=\"{lineNum}\">{relativePath}:{lineNum}</a>";
#endif
```
>[!NOTE] 
>Note: calling this function inside a Build will be practically identical to calling `ToAbsolutePath`. Hyperlinks will not be added since script files are absent in the Build. You will see correct file and line output, but with absolute paths and without clickability.
## Configuring ProjectSettings
You can configure aspects of the plugin in the menu `ProjectSettings > BurstTrace`.

⚠️ Do not change the file name or path! It must have the path `Resources/Burst Trace Config.asset`
### Disable 64-hash optimization
> The plugin replaces strings with hashes for optimization. By default, a 64-bit hash is used. The chance of collision is negligible but theoretically exists. If you encounter hash collisions (please create an Issue in the repository), enable this option. It will slightly reduce performance and increase memory usage, but completely eliminate the probability of collisions.

### Capture profiler
>Check this box to display the moment of log creation and recording in the Unity Profiler. It will also show inside Jobs. This way you can find out the exact cost of usage inside your Jobs. It is recommended to disable this in release, as writing to the Profiler can theoretically be more expensive than creating the log itself.

### Disable logs
>This will exclude log creation code from the Build but will not make the `TraceHandle` struct empty. It will still occupy 4 bytes, but its value will always be empty. This is done to avoid theoretical issues with serialization and memory structure between runs. The creation code itself will turn into empty methods, so calling it inside Burst will cost nothing, as Burst will simply strip it. Calling outside Burst is also extremely cheap, as it is just a call to an empty method.

