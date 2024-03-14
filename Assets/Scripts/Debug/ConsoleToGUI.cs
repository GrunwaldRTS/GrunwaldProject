using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleToGUI : MonoBehaviour
{
    string myLog;
    List<string> lines = new();
    private string output;
    private string stack;
    [SerializeField] int maxLines = 10;

    void OnEnable()
    {
        Application.logMessageReceived += Log;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }

    public void Log(string logString, string stackTrace, LogType type)
    {
        output = logString;
        stack = stackTrace;
        DateTime now = DateTime.Now;
        string hours = now.Hour < 10 ? $"0{now.Hour}" : now.Hour.ToString();
        string minutes = now.Minute < 10 ? $"0{now.Minute}" : now.Minute.ToString();
        string seconds = now.Second < 10 ? $"0{now.Second}" : now.Second.ToString();
        lines.Add($"[{hours}:{minutes}:{seconds}] {output}");
        if (lines.Count > maxLines)
        {
            lines.RemoveAt(0);
        }

        myLog = "";

        foreach (string line in lines)
        {
            myLog += $"{line}\n";
        }
    }
    void OnGUI()
    {
        //if (!Application.isEditor) //Do not display in editor ( or you can use the UNITY_EDITOR macro to also disable the rest)
        {
            float width = Screen.width - Screen.width * 0.75f;
            float height = Screen.height - Screen.height * 0.5f;
            myLog = GUI.TextArea(new Rect(10, Screen.height - height - 10, width, height), myLog);
        }
    }
    //#endif
}
