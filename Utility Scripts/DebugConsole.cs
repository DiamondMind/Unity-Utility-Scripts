using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DiamondMind.Prototypes.Tools
{
    public class DebugConsole : MonoBehaviour
    {
        public int maxQueue = 15;
        public Text logText; 
        private Queue<string> logQueue = new Queue<string>();

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLogg(string logString, string stackTrace, LogType type)
        {
            logQueue.Enqueue(logString);
            if (logQueue.Count > 28) logQueue.Dequeue(); // Limit logs on screen
            logText.text = string.Join("\n", logQueue.ToArray());
        }

        private void HandleLog(string logString, string stackTrace, LogType logType)
        {
            string timeStamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string logTypeLabel = $"[{logType}]";

            // Colour coding for different log types
            string coloredLogType = logTypeLabel;
            switch (logType)
            {
                case LogType.Warning:
                    coloredLogType = $"<color=yellow>{logTypeLabel}</color>";
                    break;
                case LogType.Error:
                case LogType.Exception:
                    coloredLogType = $"<color=red>{logTypeLabel}</color>";
                    break;
                case LogType.Log:
                    coloredLogType = $"<color=white>{logTypeLabel}</color>";
                    break;
            }

            // Format the full log message
            string formattedLog = $"{timeStamp} {coloredLogType} {logString}";

            // Append part of the stack trace for errors/exceptions
            if (logType == LogType.Error || logType == LogType.Exception)
            {
                string shortStack = stackTrace.Split('\n')[0];
                formattedLog += $"\n<color=grey>{shortStack}</color>";
            }

            // Enqueue the formatted log
            logQueue.Enqueue(formattedLog);

            // Keep the log queue within limit
            if (logQueue.Count > maxQueue)
                logQueue.Dequeue();

            // Display the logs
            logText.text = string.Join("\n", logQueue.ToArray());
        }
    }

}
