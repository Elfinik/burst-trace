## 📚 API ドキュメント: BurstTrace

**Unity Burst Compiler** および標準の C# コード内での動作に最適化された、スタックトレース (Stack Trace) をキャプチャして処理するためのプラグインです。

## 📌 目次
  
- [BurstTrace](#class-bursttrace) — グローバル管理。
- [TraceHandle](#struct-tracehandle) — メインのログ構造体。
- [BurstTraceAdvanced](#class-bursttraceadvanced) — 手動登録とメモリ管理
- [BurstTraceCustomThreads](#class-bursttracecustomthreads) — System.Threading.Tasks からプラグインを使用する方法。

---

## <a id="class-bursttrace"></a>Class: BurstTrace

`public static class BurstTrace`

ログシステムを管理する機能を提供します。スタックキャプチャをグローバルに有効または無効にすることができます。

### メソッド (Methods)

#### SetLogDisabled

C#

```CSharp
public static void SetLogDisabled(bool disabled = false)
```

ログシステムを有効または無効にします。

- **パラメータ:**
	- `disabled` (`bool`): `true` の場合、ログ記録は無効になります。`false` (デフォルト) の場合、ログ記録は有効になります。

#### IsLogEnabled

C#

```CSharp
public static bool IsLogEnabled()
```

現在のログ記録ステータスを確認します。

- **戻り値:** ログ記録が有効な場合は `true`。手動で無効にされている場合、または現在のビルド (Build) でグローバルに無効にされている場合は `false` を返します。

---

## <a id="struct-tracehandle"></a>Struct: TraceHandle

`public struct TraceHandle`

特定のログレコードまたはログチェーンのハンドル (handle) を表します。この構造体は **スレッドセーフ** であり、Burst Jobs と Main Thread (メインスレッド) 間で受け渡し可能です。

>[!WARNING] 
>セッション間の永続的なシリアル化 (Persistent Cross-session serialization) は現在サポートされていません。

### キャプチャメソッド (Capture Methods)

#### Capture (Start New Chain)

C#

```CSharp
public static TraceHandle Capture(
    [CallerMemberName] string memberName = "",
    [CallerFilePath] string sourceFilePath = "",
    [CallerLineNumber] int sourceLineNumber = 0
)
```

現在のスタックフレームを新しいログチェーンの **開始 (start)** としてキャプチャします。

- **安全性:** Main Thread, Burst Jobs/Parallel Jobs (Unity Job System).
- **パラメータ:**
    - `memberName`, `sourceFilePath`, `sourceLineNumber`: コンパイラによって自動的に入力されます。**手動で値を渡さないでください。**
- **戻り値:** 現在のフレームを表す新しい `TraceHandle`。

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

現在のスタックフレームをキャプチャし、既存のログチェーンに **追加 (append)** します。

- **安全性:** Main Thread, Burst Jobs/Parallel Jobs (Unity Job System).
- **パラメータ:**
    - `prev`: 継続する以前のハンドル (`TraceHandle`)。
    - `memberName`, `sourceFilePath`, `sourceLineNumber`: コンパイラによって自動的に入力されます。
- **戻り値:** 更新されたチェーンを含む新しい `TraceHandle`。

### フォーマットメソッド (Formatting Methods)

#### ToProjectLink

C#

```CSharp
public string ToProjectLink()
```

ファイルへの **相対パス** (プロジェクトルートからの相対パス) を使用して、フォーマットされたログチェーン文字列を返します。

- ⚠️ **Main Thread のみ** (Burst 内部では無視されます)。

#### ToAbsolutePath

C#

```CSharp
public string ToAbsolutePath()
```

ファイルへの **絶対パス** を使用して、フォーマットされたログチェーン文字列を返します。

- ⚠️ **Main Thread のみ** (Burst 内部では無視されます)。

#### ToConsoleToken

C#

```CSharp
public FixedString128Bytes ToConsoleToken()
```

コンソール用のクリック可能なハイパーリンクを `FixedString128Bytes` 形式で生成します。

> [!TIP] 
> このメソッドは Burst-jobs 内での使用を想定して設計されています。スタック全体を含む重い文字列をフォーマットする代わりに、軽量なトークンを作成します。Unity コンソールでこのトークンをクリックすると、特別なコールバックがトリガーされ、完全な Stack Trace が展開・表示されます。

---

## <a id="class-bursttraceadvanced"></a>Class: BurstTraceAdvanced

> これは高度な使用法のためのクラスであり、別の名前空間に配置されています。

`namespace Elfinik.BurstTrace.Internal`

`public static class BurstTraceAdvanced`

拡張使用、手動ログ登録、およびメモリ監視のためのクラスです。

### 手動登録 (Manual Registration)

#### RegisterLog (Start New)

手動で渡された `FixedString` データを使用して、フレームを新しいチェーンの開始としてキャプチャします。

#### RegisterLog (Append)

手動で渡された `FixedString` データを使用して、フレームを既存のチェーンに追加します。

### メモリ管理 (Memory Management)

#### GetTotalAllocatedMemory

C#

```CSharp
[BurstDiscard]
public static int GetTotalAllocatedMemory()
```

ログシステムに割り当てられた合計メモリ量 (バイト単位) を返します。

- ⚠️ **Main Thread のみ** (Burst 内部では無視されます)。

#### GetUsedMemory

C#

```CSharp
[BurstDiscard]
public static int GetUsedMemory()
```

すでにログで埋まっているメモリ量 (バイト単位) を返します。

- ⚠️ **Main Thread のみ** (Burst 内部では無視されます)。

---

## <a id="class-bursttracecustomthreads"></a>Class: BurstTraceCustomThreads

> これは高度な使用法のためのクラスであり、別の名前空間に配置されています。

`namespace Elfinik.BurstTrace.Internal`

`public static class BurstTraceCustomThreads`

拡張使用のためのクラスであり、任意の* マルチスレッドフレームワークから BurstTrace を使用できるようにします。

\* 理論上、他のフレームワークでの使用に障害はありませんが、現在は System.Threading.Tasks のみがテストされており、それも部分的です。

#### GetMaxThreadsCount

C#

```CSharp
public static int GetMaxThreadsCount {get;}
```

可能な最大スレッドインデックス値を返します。

### キャプチャメソッド (Capture Methods)

#### Capture (Start New Chain)

C#

```CSharp
public static TraceHandle Capture(int threadIndex, 
    [CallerMemberName] string memberName = "",
    [CallerFilePath] string sourceFilePath = "",
    [CallerLineNumber] int sourceLineNumber = 0
)
```

現在のスタックフレームを新しいログチェーンの **開始 (start)** としてキャプチャします。

- **安全性:** Main Thread, 任意の C# Threads
- **パラメータ:**
    - `threadIndex`: 一意のワーカースレッドインデックス (Worker Thread Index)。
    - `memberName`, `sourceFilePath`, `sourceLineNumber`: コンパイラによって自動的に入力されます。**手動で値を渡さないでください。**
- **戻り値:** 現在のフレームを表す新しい `TraceHandle`。
- ⚠️ 利用可能な最大スレッド数 (`GetMaxThreadsCount`) を超える `threadIndex` を渡すと、例外がスローされます。
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

現在のスタックフレームをキャプチャし、既存のログチェーンに **追加 (append)** します。

- **安全性:** Main Thread, 任意の C# Threads
- **パラメータ:**
    - `threadIndex`: 一意のワーカースレッドインデックス (Worker Thread Index)。
    - `prev`: 継続する以前のハンドル (`TraceHandle`)。
    - `memberName`, `sourceFilePath`, `sourceLineNumber`: コンパイラによって自動的に入力されます。
- **戻り値:** 更新されたチェーンを含む新しい `TraceHandle`。
- ⚠️ 利用可能な最大スレッド数 (`GetMaxThreadsCount`) を超える `threadIndex` を渡すと、例外がスローされます。
---
