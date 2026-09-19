# Logging System

A lightweight, zero-allocation, category-based logging system for Unity inspired by Jason Storey's logger architecture. Features colored and gradient console channels, service-locator access, and one-symbol build stripping.

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
    [SerializeField] LoggerSO Logger; // Alternatively.

    void Awake()
    {
        Logger = LogService.GetLogger(LoggerType.Player);
    }

    void Start()
    {
        // Null-safe invocation: if Logger is commented out in Awake, nothing runs
        Logger?.Log("Player initialized");

        // Zero allocation with string interpolation (evaluated only if logs are enabled)
        Logger?.Log(() => $"Player health: {health}/{maxHealth}");
    }
}
```

### Static Classes
```csharp
static LoggerSO logger;
static LoggerSO Logger => logger ??= LogService.GetLogger(LoggerType.GameManager);
```

---

## Stripping Logs from Builds

1. Go to **Project Settings > Player > Other Settings > Scripting Define Symbols**.
2. Add **`DISABLELOGS`**.
3. All `LoggerSO.Log(...)` method bodies compile into empty no-ops in player builds.
4. Logs remain **fully enabled in the Unity Editor** regardless of build settings.
5. Standard `Debug.LogWarning` and `Debug.LogError` calls remain untouched so critical runtime issues are still reported.
