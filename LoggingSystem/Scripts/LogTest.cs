using UnityEngine;

namespace Project.Logging
{
    /// <summary>
    /// Just cache the Logger in Awake(), it exists because LogService initializes the loggers before Awake()
    /// For a non-monobehavior classes, cache it in the constructor, or cache it as a static field with lazy initialization if there's a static function in that class
    /// For static classes, static field with lazy initialization. Example: 
    /// static LoggerSO logger;
    /// static LoggerSO Logger => logger ??= LogService.GetLogger(LoggerType.LogA);
    /// </summary>
    public class LogTest : MonoBehaviour
    {
        LoggerSO Logger;

        void Awake()
        {
            // Logger = LogService.GetLogger(LoggerType.Menu);
            Logger?.Log("LogTest.Awake()");
        }

        void Start()
        {
            Logger?.Log("LogTest.Start()");
        }
    }
}