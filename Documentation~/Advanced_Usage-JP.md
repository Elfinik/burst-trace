# 高度な使用法 (Advanced Usage)
## スクリプトによるログ記録の一時的な無効化
ログ作成を一時的に無効にすることができます。これによりビルドからコードが削除されることはありませんが、ログ作成の実行が除外され、呼び出しによる負荷がほぼゼロになります。これを行うには、次を呼び出します：
```CSharp
BurstTrace.SetLogDisabled(true);
```
ログを無効にするには `true` を、有効にするには `false` を渡します。以下の関数を使用して、ログが有効かどうかを確認できます：
```CSharp
BurstTrace.IsLogEnabled();
```
ログが無効になっている場合、またはリリースビルドからプラグインのコードを除外した場合、関数は `false` を返します。ログが有効な場合は `true` を返します。
## リリースでの無効化
`ProjectSettings > BurstTrace > Disable logs` にチェックを入れることで、リリースビルドからコードを完全に除外できます。

これによりログ作成コードは除外されますが、`TraceHandle` 構造体は空にはなりません。依然として4バイトを占有しますが、値は常に空になります。これは、実行間のシリアル化やメモリ構造に関する理論上の問題を回避するためです。

作成コード自体は空のメソッドになるため、Burst 内部での呼び出しコストはゼロになります（Burst が単に削除するため）。Burst 外部での呼び出しも、単に空のメソッドを呼び出すだけなので非常に低コストです。
## 呼び出し元でのコード記述なしによる自動ログ作成
手動で `TraceHandle` を呼び出して渡す代わりに、呼び出し元の行のログを自動的に作成する関数を作りたい場合は、次のパターンを使用してください：
```CSharp
    public void WriteDamageEvent(DamageEvent damageEvent, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
    {
        this.damageSenderTraceHandle = BurstTraceAdvanced.RegisterLog(damageEvent.burstTrace, memberName, sourceFilePath, sourceLineNumber);
    }
```
この例では `DamageEvent` が関数に渡されていますが、何も渡さなくても構いません。ここで重要なのは次の3つの引数です：
```CSharp
[CallerMemberName] string memberName = "",
[CallerFilePath] string sourceFilePath = "",
[CallerLineNumber] int sourceLineNumber = 0
```
これらを関数に追加し、その後、カスタムログ登録呼び出しに渡します：
```CSharp
        TraceHandle customTraceHandle = BurstTraceAdvanced.RegisterLog(memberName, sourceFilePath, sourceLineNumber);
```
最初の引数として別の `TraceHandle` を渡してチェーンを作成することもできます。その後、通常通り関数を呼び出すだけです：
```CSharp
        WriteDamageEvent(damageEvent);
```
**関数呼び出し時に、これらの引数にデータを代入しないでください！ これらの引数に手動で値を渡さないでください。コンパイラが自動的に値を入力します。** 

このようにして、例えば、呼び出し時に追加のコードを記述することなく、呼び出された場所を記録する関数を作成できます。これを Burst 内部や並列 Job (Parallel Jobs) 内で使用することも可能です。
> 注意：この方法は標準の `TraceHandle.Capture()` よりもわずかに低速です。メソッド内部にログ呼び出しを隠蔽することが重要な場合にのみ使用してください。パフォーマンスの低下は **ごくわずか** です。
## さらなる分析のためのログ出力
完全なログ文字列を取得する必要がある場合は、次を呼び出します：
```CSharp
traceHandle.ToAbsolutePath();
```
これはログチェーンの行ごとの出力を返します。

次を呼び出すと：
```CSharp
traceHandle.ToProjectLink();
```
以下の形式が得られます（Unityのバージョンに依存）：
```CSharp
#if UNITY_6000_0_OR_NEWER
string link = $"<color=#40a0ff><link=\"href='{relativePath}' line='{lineNum}'\">{relativePath}:{lineNum}</link></color>";
#else
string link = $"<a href=\"{relativePath}\" line=\"{lineNum}\">{relativePath}:{lineNum}</a>";
#endif
```
>[!NOTE] 
>注意：ビルド内でこの関数を呼び出すと、`ToAbsolutePath` の呼び出しとほぼ同じ結果になります。ビルドにはスクリプトファイルが存在しないため、ハイパーリンクは追加されません。正しいファイルと行の出力が表示されますが、絶対パスとなり、クリック機能はありません。
## ProjectSettings の設定
メニューの `ProjectSettings > BurstTrace` でプラグインの各側面を設定できます。

⚠️ ファイル名やパスを変更しないでください！ パスは `Resources/Burst Trace Config.asset` である必要があります。

###  Memory optimization mode (~x3)
> このモードは、ストレージメモリの使用量を約3分の1に削減（約3倍の最適化）します。その代わり、初回のログ登録時およびログ読み取り時にわずかなオーバーヘッドが発生します。また、`Assets` フォルダ内のディレクトリ階層が深すぎるプロジェクトでは、ログの出力（文字列）が破損する可能性があります。**ファイルの相対パス（`Assets` フォルダ内）が124バイトを超える場合は、このオプションを有効にすることは推奨されません！**

### Disable 64-hash optimization
> プラグインは最適化のために文字列をハッシュに置き換えます。デフォルトでは64ビットハッシュが使用されます。衝突の確率は無視できるほど低いですが、理論上は存在します。もしハッシュ衝突に遭遇した場合（リポジトリで Issue を作成してください）、このオプションを有効にしてください。パフォーマンスがわずかに低下し、メモリ使用量が増加しますが、衝突の可能性を完全に排除します。

### Capture profiler
>ログの作成と記録の瞬間を Unity Profiler に表示するには、このチェックボックスをオンにします。Job 内部でも表示されます。これにより、Job 内部での使用コストを正確に把握できます。Profiler への書き込みはログ作成自体よりもコストがかかる可能性があるため、リリースでは無効にすることをお勧めします。

### Disable logs
>これにより、ビルドからログ作成コードが除外されますが、`TraceHandle` 構造体は空にはなりません。依然として4バイトを占有しますが、値は常に空になります。これは、実行間のシリアル化やメモリ構造に関する理論上の問題を回避するためです。作成コード自体は空のメソッドになるため、Burst 内部での呼び出しコストはゼロになります（Burst が単に削除するため）。Burst 外部での呼び出しも、単に空のメソッドを呼び出すだけなので非常に低コストです。" }

