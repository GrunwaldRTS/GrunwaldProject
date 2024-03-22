using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkManagement : MonoBehaviour
{
    [SerializeField] float _reconcolidationTreshold = 5f;
    [SerializeField] float _idleReconcolidationTreshold = 0.5f;
    [SerializeField] float _reconcolidationCoolDown = 1f;
    [SerializeField] int _interpolationDelay = 100;

    static NetworkManagement _instance;
    public static NetworkManagement Instance { 
        get 
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("NetworkManager").GetComponent<NetworkManagement>();
            }

            return _instance;
        }
    }
    public float ReconcilidationTreshold { get => _reconcolidationTreshold; }
    public float IdleReconcolidationTreshold { get => _idleReconcolidationTreshold; }
    public float ReconcolidationCoolDown { get => _reconcolidationCoolDown; }
    public int InterpolationDelay { get => _interpolationDelay; }

    public InputState GetInputState(Vector3 newPosition, Quaternion finalRotation)
    {
        return new(NetworkManager.Singleton.LocalTime.Tick, NetworkManager.Singleton.LocalTime.Time, newPosition,  finalRotation);
    }
}
