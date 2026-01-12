> Detailed documentation in development

![Import Samples](Images/Import_Samples.png)

If you want to view usage examples, you can import the samples folder.

Open the Samples scene.

If you have the Entities package installed, you can open the sample in `ECS/ECS Samples`.

Attention! If you run the samples on Unity 6+, you will see an error in the Console:
```
InvalidOperationException: You are trying to read Input using the UnityEngine.Input class, but you have switched active Input handling to Input System Package in Player Settings.
```
Click on it; it will highlight the `EventSystem` object in the Scene. Click the **Replace with InputSystemUIInputModule** button in the Inspector. This will replace the old input component with one compatible with the new Input System.
![Error Fix](Images/Unity_6_Input_Fix.png)

In Unity 6, text fields contain embedded links (rich text tags). This is due to changes in link handling in Unity 6. This does not affect functionality in any way and is simply not updated in the demo scene for compatibility with older versions.

The `CustomThreads` scene shows an example of working from `System.Threading.Tasks` or any other multithreading frameworks (not Job System).