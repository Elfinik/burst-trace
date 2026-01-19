> 详细文档正在开发中

![Import Samples](Images/Import_Samples.png)

如果您想查看使用示例，可以导入 Samples 文件夹。

打开 Samples 场景。

如果您安装了 Entities 包，可以打开 `ECS/ECS Samples` 中的示例。

注意！如果您在 Unity 6+ 上运行示例，控制台可能会显示错误：
```
InvalidOperationException: You are trying to read Input using the UnityEngine.Input class, but you have switched active Input handling to Input System Package in Player Settings.
```
点击该错误，它会高亮显示场景中的 `EventSystem` 对象。点击检视面板中的 **Replace with InputSystemUIInputModule** 按钮。这将把旧的输入组件替换为与新 Input System 兼容的组件。
![Error Fix](Images/Unity_6_Input_Fix.png)

在 Unity 6 中，文本字段包含嵌入链接（富文本标签）。这是由于 Unity 6 中链接处理方式的更改。这不会以任何方式影响功能，为了兼容旧版本，演示场景中未对此进行更新。

`CustomThreads` 场景展示了从 `System.Threading.Tasks` 或任何其他多线程框架（非 Job System）工作的示例。"