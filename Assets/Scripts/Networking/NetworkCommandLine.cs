using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkCommandLine : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (Application.isEditor) return;

        var args = GetCommandLineArgs();

        if (args.TryGetValue("-mode", out string mode))
        {
            switch (mode)
            {
                case "server":
                    NetworkManager.Singleton.StartServer();
                    break;
                case "client":
                    NetworkManager.Singleton.StartClient();
                    break;
                case "host":
                    NetworkManager.Singleton.StartHost();
                    break;
            }
        }
    }

    Dictionary<string, string> GetCommandLineArgs()
    {
        Dictionary<string, string> argtDictionary = new();

        string[] args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i].ToLower();
            if (arg.StartsWith("-"))
            {
                string value = i < args.Length - 1 ? args[i + 1].ToLower() : null;
                value = (value?.StartsWith("-") ?? false) ? null : value;

                argtDictionary.Add(arg, value);
            }
        }

        return argtDictionary;
    }
}
