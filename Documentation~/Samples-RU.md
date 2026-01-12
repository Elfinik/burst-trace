> Подробная документация в разработке

![Import Samples](Images/Import_Samples.png)

Если вы хотите посмотреть примеры использования, вы можете импортировать папку с примерами. 

Откройте сцену Samples

Если у вас установлен пакет Entities, вы можете открыть пример в `ECS/ECS Samples`

Внимание! Если вы запускаете примеры на Unity 6+ вы увидите ошибку в консоли:
```
InvalidOperationException: You are trying to read Input using the UnityEngine.Input class, but you have switched active Input handling to Input System Package in Player Settings.
```
Нажмите на неё, она подсветит объект `EventSystem` на сцене. Нажмите кнопку **Replace with InputSystemUIInputModule** в инспекторе. Это заменит старый компонент ввода на совместимый с новой Input System.
![Error Fix](Images/Unity_6_Input_Fix.png)

В Unity 6 текст в текстовом поле будет иметь ссылки в своем теле. Это из-за изменения обработки ссылок в Unity 6. Это никак не влияет на функциональность, и просто не обновляется в демонстрационной сцене для совместимости со старыми версиями.

Сцена `CustomThreads` показывает пример работы из System.Threading.Tasks или любых других фреймворков многопоточности (не Job System)