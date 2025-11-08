using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GdkSample_Achievements
{
    public class Logger : MonoBehaviour
    {
        public Text textOut;

        private static int logEntryCounter = 0;
        private static int logQueueCapacity = 12;

        private static Queue<string> logQueue = new Queue<string>(logQueueCapacity);

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLogMessageReceived;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLogMessageReceived;
        }

        private void HandleLogMessageReceived(string logString, string stackTrace, LogType type)
        {
            string newString = "[" + type + "] : " + logString;
            if (type == LogType.Exception)
            {
                newString = stackTrace;
            }

            LogMessage(newString);
        }

        public static void LogMessage(string message)
        {
            logEntryCounter++;

            // Make space for the new entry if necessary
            if (logQueue.Count >= logQueueCapacity)
            {
                logQueue.Dequeue();
            }

            logQueue.Enqueue(logEntryCounter.ToString() + ": " + message);
        }

        private static string GetLogString()
        {
            StringBuilder stringBuilder = new StringBuilder(logQueue.Count);
            foreach (var line in logQueue)
            {
                stringBuilder.AppendLine(line);
            }

            return stringBuilder.ToString();
        }

        // Update is called once per frame
        private void Update()
        {
            textOut.text = GetLogString();
        }
    }
} // namespace GdkSample_Achievements