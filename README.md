# Logging System

A lightweight, zero-allocation, category-based logging system for Unity inspired by Jason Storey's logger architecture. Features colored and gradient console channels, service-locator access, and one-symbol build stripping.

---

## Installation

### Method 1: Unity Package Manager via Git URL (Recommended)

1. In the Unity Editor, open **Window > Package Manager**.
2. Click the **`+`** button in the top-left toolbar.
3. Select **Add package from git URL...**.
4. Enter the repository URL:
   ```text
   https://github.com/Amaan12/LoggingSystem.git
   ```
5. Click **Add**. Unity will automatically fetch and install the package.

### Method 2: Via `Packages/manifest.json`

Add the package entry directly to your project's `Packages/manifest.json` under `dependencies`:
```json
{
  "dependencies": {
    "com.amaan.loggingsystem": "https://github.com/Amaan12/LoggingSystem.git",
    ...
  }
}
```

---

## Setup & Sample Configuration

`LogService` loads channels from `Resources/Logger/LoggerRegistrySO`. To get started with the pre-configured channels:

1. In the **Package Manager** window, select **LoggingSystem** under **Packages**.
2. Switch to the **Samples** tab in the details panel on the right.
3. Click **Import** next to **Resources**.
   * Unity imports the sample into your project directly as:
     ```text
     Assets/Samples/LoggingSystem/1.0.0/Resources/Logger/
     ```
   * Because Unity automatically indexes any folder named `Resources` anywhere in the project, `LogService` finds `Resources/Logger/LoggerRegistrySO` out-of-the-box!
4. *(Optional)* If you prefer keeping resources under the root `Assets/Resources` folder, you can also move or copy the `Logger` folder to `Assets/Resources/Logger`.
5. Customize, toggle, or add channels in `Logger/Loggers/` to fit your project.

---

## Why This System? (Design Rationale)

* **Cross-Cutting Service Locator:** Logging is a cross-cutting concern. While `[SerializeField]` dependency injection works, `LogService` provides instant global access without wiring dependencies across every prefab.
* **Granular Control:**
  * **Globally:** Enable/disable any channel asset via its `enableLogs` toggle.
  * **Per-Script:** Comment out `LogService.GetLogger(...)` in `Awake()`. By using `Logger?.Log()`, logs in that script are muted instantly without commenting out individual lines.
* **Build Stripping:** Strip all logger calls from release builds at compile-time with the `DISABLELOGS` symbol, while keeping warnings, errors, and Editor logs active.
* **Beautiful Console Output:** Text-colored prefixes and per-character gradients for instant visual filtering, avoiding cluttered asset-store highlights (e.g. EditorConsolePro).
* **Zero-Allocation Friendly:** Supports `Func<string>` lambdas and `Action<StringBuilder>` overloads to eliminate string allocations when logging is disabled.
* **Naming Convention:** Name the local field `LoggerSO Logger;` so calls read cleanly as `Logger?.Log(...)`.

---

## Core Components

### 1. `LoggerSO` (ScriptableObject)
The core asset representing an individual logging channel (`Create > Logging > Logger`).
* **Settings:** `loggerType` enum identifier, `enableLogs` master toggle.
* **Prefix Options:**
  * `useEnumAsPrefix`: Automatically syncs the prefix text with the `LoggerType` enum.
  * `prefix`: Custom prefix text.
  * `separator`: Trailing separator symbol (defaults to `' '`).
  * `boldPrefix` / `italicPrefix`: Rich text formatting toggles.
* **Coloring Options:**
  * Solid Color: Configurable `prefixColor`.
  * Gradient: Toggle `useGradient` and assign a `prefixGradient` to generate smooth per-character gradient text.
* **Overloads:**
  * `Log(object, context)` — Standard object/string logging.
  * `Log(Func<string>, context)` — Lazy evaluation; skips string interpolation when disabled.
  * `Log(Action<StringBuilder>, context)` — Reuses a cached buffer for zero heap allocation.

### 2. `LogService` (Static Service Locator)
* Automatically initializes before scene load (`[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`) with lazy-initialization fallback for Edit Mode.
* Loads `Resources/Logger/LoggerRegistrySO` and indexes channels into a dictionary.
* Access any logger anywhere via `LogService.GetLogger(LoggerType)`.

### 3. `LogRegistrySO` (ScriptableObject)
* Central registry holding references to all active `LoggerSO` channel assets (`Create > Logging > Logger Registry`).
* Located at `Assets/.../Resources/Logger/LoggerRegistrySO.asset`.

### 4. `BuildLogger` (MonoBehaviour)
* On-screen runtime GUI console monitor for development builds.
* Captures Unity's `Application.logMessageReceived` and displays on-screen messages with customizable clear policies (`ClearPeriodically` or `ClearWhenMax`).
* It is better to use Quantum Console instead, since this GUI is using old UI system of Unity which is unoptimized and will cause FPS drops.

### 5. `LogAllTest` & `LogTest` (Testing Utilities)
* **Menu Command:** Run **`Tools > Logging > Log All Loggers`** to output a sample log from every registered channel into the console without entering Play Mode.
* **Play Mode:** Attach `LogAllTest` to any GameObject to test channels on `Start()`.

---

## Usage Guide

### Basic Component Logging
```csharp
using UnityEngine;
using Project.Logging;

public class PlayerController : MonoBehaviour
{
    LoggerSO Logger;
    // Alternatively: [SerializeField] LoggerSO Logger;

    void Awake()
    {
        Logger = LogService.GetLogger(LoggerType.Player);
    }

    void Start()
    {
        // 1. Static text (no variables)
        Logger?.Log("Player initialized");

        // 2. String interpolation (dynamic variables)
        Logger?.Log(() => $"Player health: {health}/{maxHealth}");

        // 3. Multi-line or loops
        Logger?.Log(sb => {
            sb.AppendLine("Equipped items:");
            foreach (var item in equippedItems)
                sb.AppendLine($"- {item.name}");
        });
    }
}
```

### Static Classes
```csharp
static LoggerSO logger;
static LoggerSO Logger => logger ??= LogService.GetLogger(LoggerType.GameManager);
```

---

### When to Use Which Overload

* **Direct String (`Logger?.Log("Text")`):**
  * **Use for:** Static text with no variables.
  * **Why:** String literals are interned at compile time. When disabled, this has zero heap allocations and avoids any delegate overhead.
* **Lambda (`Logger?.Log(() => $"Value: {val}")`):**
  * **Use for:** Any message with string interpolation (`$"..."`) or variable formatting.
  * **Why:** Defers string construction. When the channel is disabled, string formatting and string allocation are completely skipped.
* **StringBuilder (`Logger?.Log(sb => ...)`):**
  * **Use for:** Large dumps, multi-line logs, or assembling text across loops.
  * **Why:** Reuses an internal buffer, avoiding repeated intermediate string allocations during loop iterations.

---

### Optimization Comparison

| Log Method | When Enabled | When Disabled |
| :--- | :--- | :--- |
| **`Debug.Log($"...")`** *(Unity default)* | Full cost: formats string, crosses native C++ boundary, and captures stack trace. | Formats string and allocates heap memory before checking log level; crosses native boundary. |
| **`Logger?.Log("...")`** *(Direct String)* | Same as `Debug.Log` (appends prefix and logs). | **Zero heap allocation.** Immediate exit; completely skips native engine and stack trace. |
| **`Logger?.Log(() => $"...")`** *(Lambda)* | Small delegate overhead, then logs normally (difference vs `Debug.Log` is negligible). | **Highly optimized.** Interpolation never executes, saving string allocation, native calls, and stack traces. |
| **`Logger?.Log(sb => ...)`** *(StringBuilder)* | **Most memory-efficient for long logs.** Reuses buffer; avoids intermediate strings. | Block never executes; no string construction or native calls. |

> [!TIP]
> **Null-Conditional Muting:** If `Logger` is not initialized or set to `null` (e.g., commented out in `Awake`), C#'s `?.` operator skips argument evaluation and closures entirely, resulting in zero overhead.

---

## Stripping Logs from Builds

1. Go to **Project Settings > Player > Other Settings > Scripting Define Symbols**.
2. Add **`DISABLELOGS`**.
3. All `LoggerSO.Log(...)` method bodies compile into empty no-ops in player builds.
4. Logs remain **fully enabled in the Unity Editor** regardless of build settings.
5. Standard `Debug.LogWarning` and `Debug.LogError` calls remain untouched so critical runtime issues are still reported.

---

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.
