
> 実装例は `Samples > CustomThreadSample.cs` または `Samples > CustomThreads.unity` で確認できます。

**注意！この部分は、内部的な安全性チェックはありますが、まだユニットテストでカバーされていません。**

>[!WARNING] 
>注意！JobSystem と独自のスレッドシステム（BurstTrace のコンテキストにおいて）の並行使用は、インデックスの衝突により現在は許可されていません！ 次のバージョンで修正する予定ですが、この機能が重要な場合はリクエストを作成してください。優先的に追加します。

**警告:** この機能は実験的なものです。Unity Job System 向けではありません（Jobs には標準の `TraceHandle.Capture` を使用してください）。これはカスタムスレッドフレームワーク（例：System.Threading.Tasks）専用です。

`Elfinik.BurstTrace.Internal` 名前空間にある特別なクラス `BurstTraceCustomThreads` を使用してください。

通常の呼び出し：
```CSharp
using Elfinik.BurstTrace
...
traceHandle = TraceHandle.Capture();
```

これの代わりに、次のように呼び出す必要があります：

```CSharp
using Elfinik.BurstTrace.Internal
...
traceHandle = BurstTraceCustomThreads.Capture(threadIndex);
//or
traceHandle = BurstTraceCustomThreads.Capture(threadIndex, traceHandle);
```

主なタスクは `threadIndex` を渡すことです。これはスレッドのインデックスです。メインスレッドではゼロです。取得方法はフレームワークに依存します。複数のタスクを並列実行し、同じスレッドインデックスで関数を呼び出そうとすると、競合状態（Race Condition）が発生し、予期しない結果を招きます。

**重要:** 同時に実行される各スレッド/タスクに対して、競合状態 (Race Condition) を避けるために `threadIndex` の値は一意である必要があります。

最大スレッド数を取得するには、次を呼び出します：
```CSharp
BurstTraceCustomThreads.GetMaxThreadsCount
```

> この値は、ハードウェアスレッドの正確な数ではなく、システムが許容する並列インデックスの最大容量を反映しています。

`threadIndex` の最大値 = `BurstTraceCustomThreads.GetMaxThreadsCount - 1` です。また、これは `JobsUtility.ThreadIndexCount` と等しいです。通常、最大スレッド数は 20 なので、0 から 20 までのインデックスを指定できます。

もし 20 以上の並列タスクを実行する場合は、GitHub でリクエストを作成し、使用しているフレームワークと最大並列実行数を明記して開発者に連絡してください。

この関数には内部的な安全性チェックがあります。インデックスが 0 未満、または `BurstTraceCustomThreads.GetMaxThreadsCount` 以上の場合、例外が発生します。

**注意！この実装はすべてのフレームワークでテストされているわけではなく、Unity のセキュリティシステムと競合する可能性があります。エラーが発生した場合は、エラー内容、フレームワーク、および実装コードの例を記載して GitHub でリクエストを作成してください！**