using System.Collections.Generic;
using UnityEngine;

namespace Project.Logging
{
    [CreateAssetMenu(menuName = "Logging/Logger Registry")]
    public class LoggerRegistrySO : ScriptableObject
    {
        public List<LoggerSO> Loggers = new();
    }
}