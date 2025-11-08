using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GdkSample_GameSave
{
    public class OnScreenLog : MonoBehaviour
    {
        [SerializeField]
        private ScrollRect m_LogScrollRect;

        public GameObject m_LogUIPrefab;
        private string m_LogStringToDisplay;
        private int m_TotalLogPrinted;
        public List<GameObject> m_CreatedUILogGO = new List<GameObject>();

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            m_LogStringToDisplay = logString;
            string newString = "[" + type + "] : " + m_LogStringToDisplay;
            if (type == LogType.Exception)
            {
                newString = stackTrace;
            }

            PrintToUILog(newString);
        }

        public void PrintToUILog(string message)
        {
            var tempObject = Instantiate(m_LogUIPrefab);
            tempObject.SetActive(true);
            m_CreatedUILogGO.Add(tempObject);
            tempObject.GetComponent<Text>().text = System.DateTime.Now.ToString() + "\n " + " " + message;
            m_TotalLogPrinted++;
            tempObject.name = "Log" + m_TotalLogPrinted;
            tempObject.transform.SetParent(m_LogScrollRect.content.gameObject.transform, false);
            m_LogScrollRect.normalizedPosition = new Vector2(0, 0);
            m_LogScrollRect.verticalNormalizedPosition = 0;
        }
    }
}