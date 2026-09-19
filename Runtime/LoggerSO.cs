using System;
using UnityEngine;
using System.Text;

namespace Project.Logging
{
    [CreateAssetMenu(menuName = "Logging/Logger")]
    public class LoggerSO : ScriptableObject
    {
        [Header("Settings")]
        public LoggerType loggerType;
        [SerializeField] bool enableLogs = true;

        [Header("Prefix")]
        [Tooltip("When enabled, prefix is automatically set to the LoggerType enum name.")]
        [SerializeField] bool useEnumAsPrefix = false;
        [SerializeField] string prefix;
        [Tooltip("Separator symbol placed after the prefix (default is ' ')")]
        [SerializeField] string separator = " ";
        [SerializeField] bool boldPrefix = false;
        [SerializeField] bool italicPrefix = false;

        [Header("Coloring")]
        [Tooltip("Enable to use a multi-color gradient on the prefix instead of a solid color.")]
        [SerializeField] bool useGradient = false;
        [SerializeField] Color prefixColor = Color.white;
        [SerializeField] Gradient prefixGradient = new Gradient();

        string _hexColor;
        string _cachedPrefix;

        void OnValidate()
        {
            if (useEnumAsPrefix)
            {
                prefix = loggerType.ToString();
            }
            UpdateCachedPrefix();
        }

        void OnEnable()
        {
            UpdateCachedPrefix();
        }

        void UpdateCachedPrefix()
        {
            string effectivePrefix = useEnumAsPrefix ? loggerType.ToString() : prefix;
            if (string.IsNullOrEmpty(effectivePrefix))
            {
                effectivePrefix = loggerType.ToString();
            }

            string sep = separator ?? " ";
            if (!string.IsNullOrEmpty(sep) && !sep.EndsWith(" "))
            {
                sep += " ";
            }

            string coloredPrefix;
            if (useGradient && prefixGradient != null)
            {
                coloredPrefix = GenerateGradientPrefix(effectivePrefix, prefixGradient);
            }
            else
            {
                _hexColor = "#" + ColorUtility.ToHtmlStringRGBA(prefixColor);
                coloredPrefix = $"<color={_hexColor}>{effectivePrefix}</color>";
            }

            if (boldPrefix)
            {
                coloredPrefix = $"<b>{coloredPrefix}</b>";
            }
            if (italicPrefix)
            {
                coloredPrefix = $"<i>{coloredPrefix}</i>";
            }

            _cachedPrefix = $"{coloredPrefix}{sep}";
        }

        string GenerateGradientPrefix(string text, Gradient gradient)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (gradient == null) return text;

            var builder = new StringBuilder(text.Length * 26);
            for (int i = 0; i < text.Length; i++)
            {
                float t = text.Length > 1 ? (float)i / (text.Length - 1) : 0f;
                Color c = gradient.Evaluate(t);
                string hex = ColorUtility.ToHtmlStringRGBA(c);
                builder.Append("<color=#").Append(hex).Append('>').Append(text[i]).Append("</color>");
            }
            return builder.ToString();
        }

        private StringBuilder sb;

        public void Log(Action<StringBuilder> builder, UnityEngine.Object context = null)
        {
#if UNITY_EDITOR || !DISABLELOGS
            if (!enableLogs || builder == null) return;

            sb ??= new StringBuilder();
            sb.Clear();
            if (string.IsNullOrEmpty(_cachedPrefix)) UpdateCachedPrefix();

            sb.Append(_cachedPrefix);
            builder(sb);

            Debug.Log(sb.ToString(), context);
#endif
        }

        public void Log(Func<string> message, UnityEngine.Object context = null)
        {
#if UNITY_EDITOR || !DISABLELOGS
            if (!enableLogs || message == null) return;
            if (string.IsNullOrEmpty(_cachedPrefix)) UpdateCachedPrefix();

            string text = message() ?? "null";
            Debug.Log($"{_cachedPrefix}{text}", context);
#endif
        }

        public void Log(object message, UnityEngine.Object context = null)
        {
#if UNITY_EDITOR || !DISABLELOGS
            if (!enableLogs) return;
            if (string.IsNullOrEmpty(_cachedPrefix)) UpdateCachedPrefix();

            string text = message?.ToString() ?? "null";
            Debug.Log($"{_cachedPrefix}{text}", context);
#endif
        }
    }
}