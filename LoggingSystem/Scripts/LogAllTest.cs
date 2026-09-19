using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Project.Logging
{
    /// <summary>
    /// Temporary test script to verify and showcase all logger channels.
    /// You can run this via:
    /// 1. Unity Editor Menu: Tools -> Logging -> Log All Loggers
    /// 2. Component Context Menu: Right-click component -> "Log All Loggers"
    /// 3. Entering Play Mode when attached to any GameObject in the scene.
    /// </summary>
    public class LogAllTest : MonoBehaviour
    {
        void Start()
        {
            LogAll();
        }

        [ContextMenu("Log All Loggers")]
#if UNITY_EDITOR
        [MenuItem("Tools/Logging/Log All Loggers")]
#endif
        public static void LogAll()
        {
            Debug.Log("<color=white>========== [LOGGING SYSTEM TEST START] ==========</color>");

            foreach (LoggerType type in Enum.GetValues(typeof(LoggerType)))
            {
                LoggerSO logger = LogService.GetLogger(type);
                if (logger != null)
                {
                    logger.Log($"Sample log output for channel [{type}]");
                }
                else
                {
                    Debug.LogWarning($"No LoggerSO found registered for LoggerType.{type}");
                }
            }

            Debug.Log("<color=white>========== [LOGGING SYSTEM TEST END] ==========</color>");
        }
    }
}
