## 📚 API Documentation: BurstTrace

Плагин для захвата и обработки стека вызовов (Stack Trace), оптимизированный для работы внутри **Unity Burst Compiler** и обычного C# кода.

## 📌 Оглавление

- [BurstTrace](#class-bursttrace) — Глобальное управление.
- [TraceHandle](#struct-tracehandle) — Основная структура лога.
- [BurstTraceAdvanced](#class-bursttraceadvanced) — Ручная регистрация и память.
- [BurstTraceCustomThreads](#class-bursttracecustomthreads) — Работа с плагином из System.Threading.Tasks.

---

## <a id="class-bursttrace"></a>Class: BurstTrace

`public static class BurstTrace`

Предоставляет функциональность для управления системой логирования. Позволяет глобально включать или отключать захват стека.

### Методы

#### SetLogDisabled

C#

```CSharp
public static void SetLogDisabled(bool disabled = false)
```

Включает или отключает систему логирования.

- **Параметры:**
	- `disabled` (`bool`): Если `true`, логирование отключается. Если `false` (по умолчанию), логирование включается.

#### IsLogEnabled

C#

```CSharp
public static bool IsLogEnabled()
```

Проверяет текущий статус логирования.

- **Возвращает:**`true`, если логирование включено. Возвращает `false`, если оно отключено вручную или глобально для текущей сборки (Build).

---

## <a id="struct-tracehandle"></a>Struct: TraceHandle

`public struct TraceHandle`

Представляет дескриптор (handle) конкретной записи лога или цепочки логов. Эта структура **потокобезопасна** и может передаваться между Burst Jobs и Main Thread.

>[!WARNING] 
>Постоянная сериализация между сессиями (Cross-session serialization) в данный момент не поддерживается.

### Методы захвата (Capture)

#### Capture (Start New Chain)

C#

```CSharp
public static TraceHandle Capture(
    [CallerMemberName] string memberName = "",
    [CallerFilePath] string sourceFilePath = "",
    [CallerLineNumber] int sourceLineNumber = 0
)
```

Захватывает текущий кадр стека как **начало** новой цепочки логов.

- **Безопасность:** Main Thread, Burst Jobs/Parallel Jobs (Unity Job System).
- **Параметры:**
	- `memberName`, `sourceFilePath`, `sourceLineNumber`: Заполняются компилятором автоматически. **Не передавайте значения вручную.**
- **Возвращает:** Новый `TraceHandle`, представляющий текущий кадр.

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

Захватывает текущий кадр стека и **добавляет** его к существующей цепочке логов.

- **Безопасность:** Main Thread, Burst Jobs/Parallel Jobs (Unity Job System).
- **Параметры:**
	- `prev`: Предыдущий дескриптор (`TraceHandle`), который нужно продолжить.
	- `memberName`, `sourceFilePath`, `sourceLineNumber`: Заполняются компилятором автоматически.
- **Возвращает:** Новый `TraceHandle`, содержащий обновленную цепочку.

### Методы форматирования

#### ToProjectLink

C#

```CSharp
public string ToProjectLink()
```

Возвращает отформатированную строку цепочки логов, используя **относительные пути** к файлам (относительно корня проекта).

- ⚠️ **Только Main Thread** (игнорируется внутри Burst).

#### ToAbsolutePath

C#

```CSharp
public string ToAbsolutePath()
```

Возвращает отформатированную строку цепочки логов, используя **абсолютные пути** к файлам.

- ⚠️ **Только Main Thread** (игнорируется внутри Burst).

#### ToConsoleToken

C#

```CSharp
public FixedString128Bytes ToConsoleToken()
```

Генерирует кликабельную гиперссылку для консоли в формате `FixedString128Bytes`.

> [!TIP] 
> Этот метод создан для использования внутри Burst-jobs. Вместо того чтобы форматировать тяжелую строку со всем стеком, он создает легковесный токен. При клике на этот токен в консоли Unity срабатывает специальный callback, который разворачивает и отображает полный Stack Trace.

---

## <a id="class-bursttraceadvanced"></a>Class: BurstTraceAdvanced

> Это класс для продвинутого использования и он расположен в отдельном пространстве имен

`namespace Elfinik.BurstTrace.Internal`

`public static class BurstTraceAdvanced`

Класс для расширенного использования, ручной регистрации логов и мониторинга памяти.

### Ручная регистрация

#### RegisterLog (Start New)

Захватывает кадр как начало новой цепочки, используя переданные вручную данные `FixedString`.

#### RegisterLog (Append)

Добавляет кадр к существующей цепочке, используя переданные вручную данные `FixedString`.

### Управление памятью

#### GetTotalAllocatedMemory

C#

```CSharp
[BurstDiscard]
public static int GetTotalAllocatedMemory()
```

Возвращает общий объем памяти (в байтах), выделенный для системы логирования.

- ⚠️ **Только Main Thread** (игнорируется внутри Burst).

#### GetUsedMemory

C#

```CSharp
[BurstDiscard]
public static int GetUsedMemory()
```

Возвращает объем памяти (в байтах), который уже заполнен логами.

- ⚠️ **Только Main Thread** (игнорируется внутри Burst).

---

## <a id="class-bursttracecustomthreads"></a>Class: BurstTraceCustomThreads

> Это класс для продвинутого использования и он расположен в отдельном пространстве имен

`namespace Elfinik.BurstTrace.Internal`

`public static class BurstTraceCustomThreads`

Класс для расширенного использования, позволяет использовать BurstTrace из любого* многопоточного фреймворка.

\* Теоретически преград для использования в других фреймворках нет, но в данный момент тестировалось только System.Threading.Tasks, и то частично.

#### GetMaxThreadsCount

C#

```CSharp
public static int GetMaxThreadsCount {get;}
```

Возвращает максимально возможное значение индекса потока.

### Методы захвата (Capture)

#### Capture (Start New Chain)

C#

```CSharp
public static TraceHandle Capture(int threadIndex, 
    [CallerMemberName] string memberName = "",
    [CallerFilePath] string sourceFilePath = "",
    [CallerLineNumber] int sourceLineNumber = 0
)
```

Захватывает текущий кадр стека как **начало** новой цепочки логов.

- **Безопасность:** Main Thread, Any C# Threads
- **Параметры:**
	- `threadIndex`: Уникальный индекс рабочего потока (Worker Thread Index).
	- `memberName`, `sourceFilePath`, `sourceLineNumber`: Заполняются компилятором автоматически. **Не передавайте значения вручную.**
- **Возвращает:** Новый `TraceHandle`, представляющий текущий кадр.

- ⚠️ Передача `threadIndex`, превышающего максимальное число доступных потоков (`GetMaxThreadsCount`), вызовет исключение.
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

Захватывает текущий кадр стека и **добавляет** его к существующей цепочке логов.

- **Безопасность:** Main Thread, Any C# Threads
- **Параметры:**
	- `threadIndex`: Уникальный индекс рабочего потока (Worker Thread Index).
	- `prev`: Предыдущий дескриптор (`TraceHandle`), который нужно продолжить.
	- `memberName`, `sourceFilePath`, `sourceLineNumber`: Заполняются компилятором автоматически.
- **Возвращает:** Новый `TraceHandle`, содержащий обновленную цепочку.

- ⚠️ Передача `threadIndex`, превышающего максимальное число доступных потоков (`GetMaxThreadsCount`), вызовет исключение.
---
