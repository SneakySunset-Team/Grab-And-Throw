using System.IO;
using UnityEngine;

public class GTLogToFile : MonoBehaviour
{
    private string logFilePath;

    void Start()
    {
        logFilePath = Path.Combine(Application.persistentDataPath, "logs.txt");
        Application.logMessageReceived += HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        string logMessage = $"{System.DateTime.Now}: {logString}\n";
        File.AppendAllText(logFilePath, logMessage);
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }
}