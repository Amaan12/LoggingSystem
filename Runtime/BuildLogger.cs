using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Project.Logging
{
    /// <summary>
    /// Monitors and displays debug logs in a Unity application.
    /// </summary>
    public class BuildLogger : MonoBehaviour
    {
        public enum BuildLoggerType
        {
            ClearPeriodically,
            ClearWhenMax,
        }

        /// <summary>
        /// The maximum number of messages to keep in the log queue.
        /// </summary>
        const uint QSize = 5;

        /// <summary>
        /// Queue to store recent log messages.
        /// </summary>
        readonly Queue<string> _logQueue = new Queue<string>();

        /// <summary>
        /// To enable larger fonts
        /// </summary>
        GUIStyle logStyle;

        [SerializeField] BuildLoggerType buildLoggerType;

        [Space]
        [SerializeField] bool testLogger;
        [SerializeField] bool clearOnEnable;
        [SerializeField] float secondsToClearAfter;

        readonly Queue<Coroutine> activeCoroutines = new Queue<Coroutine>();
        Coroutine testLoggingCoroutine;

        /// <summary>
        /// Initializes logging system.
        /// </summary>
        void Start()
        {
            // Create custom style
            logStyle = new GUIStyle();
            logStyle.fontSize = 20;           // increase font size
            logStyle.normal.textColor = Color.white;  // text color
            logStyle.wordWrap = true;         // wrap text if too long

            // Test logging system
            if (testLogger)
                TestLogging();
        }

        /// <summary>
        /// Registers the log handler when the script is enabled.
        /// </summary>
        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
            if (clearOnEnable && _logQueue.Count > 0)
            {
                while (activeCoroutines.Count > 0)
                {
                    Coroutine c = activeCoroutines.Dequeue();
                    if (c != null)
                        StopCoroutine(c);
                }
                if (testLoggingCoroutine != null)
                {
                    StopCoroutine(testLoggingCoroutine);
                    testLoggingCoroutine = null;
                }
                _logQueue.Clear();
            }
            Debug.Log("Logging system initialized.");
        }

        /// <summary>
        /// Unregisters the log handler when the script is disabled.
        /// </summary>
        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        /// <summary>
        /// Handles incoming log messages.
        /// </summary>
        /// <param name="logString">The log message.</param>
        /// <param name="stackTrace">The stack trace if available.</param>
        /// <param name="type">The type of log message (e.g., error, warning).</param>
        void HandleLog(string logString, string stackTrace, LogType type)
        {
            string typeColored = type switch
            {
                LogType.Warning => "<color=yellow>[" + type + "]</color>",
                LogType.Error => "<color=red>[" + type + "]</color>",
                LogType.Exception => "<color=red>[" + type + "]</color>",
                _ => "<color=white>[" + type + "]</color>"
            };

            string formattedLog = typeColored + " : " + logString;
            // string formattedLog = "[" + type + "] : " + logString;

            _logQueue.Enqueue(formattedLog);
            // Clear after some time
            if (buildLoggerType == BuildLoggerType.ClearPeriodically)
                activeCoroutines.Enqueue(StartCoroutine(DequeLog()));

            if (type == LogType.Exception)
            {
                _logQueue.Enqueue(stackTrace);
                // Clear after some time
                if (buildLoggerType == BuildLoggerType.ClearPeriodically)
                    activeCoroutines.Enqueue(StartCoroutine(DequeLog()));
            }

            // Keep log queue size within limit
            if (buildLoggerType == BuildLoggerType.ClearWhenMax)
            {
                while (_logQueue.Count > QSize)
                    _logQueue.Dequeue();
            }
        }

        /// <summary>
        /// Renders the debug log messages on the screen.
        /// </summary>
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 450, 400, 450, Screen.height));
            // GUILayout.Label("\n" + string.Join("\n", _logQueue.ToArray()));
            GUILayout.Label("\n" + string.Join("\n", _logQueue.ToArray()), logStyle);

            GUILayout.EndArea();
        }

        /// <summary>
        /// Tests the logging system.
        /// </summary>
        void TestLogging()
        {
            testLoggingCoroutine = StartCoroutine(LogWithDelay());
        }

        IEnumerator LogWithDelay()
        {
            Debug.Log("This is a test log message.");
            yield return new WaitForSeconds(0.5f);

            Debug.LogWarning("This is a test warning message.");
            yield return new WaitForSeconds(0.5f);

            Debug.LogError("This is a test error message.");
            yield return new WaitForSeconds(0.5f);

            Debug.LogException(new Exception("This is a test exception message."));
            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < 10; i++)
            {
                Debug.Log("This is log message #" + i);
                yield return new WaitForSeconds(0.1f);
            }

            Debug.Log("This is the final test log message.");
            yield return new WaitForSeconds(0.5f);

            if (buildLoggerType == BuildLoggerType.ClearWhenMax)
                _logQueue.Clear();
        }

        IEnumerator DequeLog()
        {
            yield return new WaitForSeconds(secondsToClearAfter);
            if (_logQueue.Count > 0)
                _logQueue.Dequeue();
            if (activeCoroutines.Count > 0)
                activeCoroutines.Dequeue();
        }
    }
}