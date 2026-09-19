using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Logging
{
    public enum LoggerType
    {
        Menu,
        Save,
        Audio,
        GameManager,
        Scene,
        Time,
        Pool,
        Settings,
        Input,
        Interactable,
        Tutorial,
        Player,
        AI,
        Combat,
        Physics,
        NavMesh,
        Inventory,
        Quest,
        Economy,
        Progression,
        Achievement,
        UI,
        Dialogue,
        Camera,
        Animation,
        VFX,
        Localization,
        Network,
        Analytics
    }

    public static class LogService
    {
        [Header("Logger Registry")]
        static Dictionary<LoggerType, LoggerSO> _loggers;
        const string loggerRegistryPath = "Logger/LoggerRegistrySO";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            // Debug.Log("LogService.Reset()");
            _loggers = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            // Debug.Log("LogService.Init()");
            if (_loggers != null)
            {
                return;
            }

            LoggerRegistrySO registry = Resources.Load<LoggerRegistrySO>(loggerRegistryPath);
            if (registry == null)
            {
                Debug.LogWarning("LoggerRegistry not found in Resources.");
                return;
            }

            _loggers = new Dictionary<LoggerType, LoggerSO>();

            foreach (LoggerSO logger in registry.Loggers)
            {
                if (logger == null)
                {
                    continue;
                }

                if (_loggers.ContainsKey(logger.loggerType))
                {
                    Debug.LogWarning($"Duplicate LoggerType found: {logger.loggerType}", logger);
                    continue;
                }

                _loggers.Add(logger.loggerType, logger);
            }
        }

        public static LoggerSO GetLogger(LoggerType type)
        {
            if (_loggers == null)
            {
                Init();
            }

            if (_loggers != null && _loggers.TryGetValue(type, out LoggerSO logger))
                return logger;

            Debug.LogWarning($"No Logger found for type: {type}");
            return null;
        }
    }
}