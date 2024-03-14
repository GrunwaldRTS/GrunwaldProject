using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagement : MonoBehaviour
{
    [SerializeField] uint _tickRate = 50;
    [SerializeField] float _reconcolidationTreshold = 5f;
    [SerializeField] float _idleReconcolidationTreshold = 0.5f;
    [SerializeField] float _reconcolidationCoolDown = 1f;
    [SerializeField] float _extrapolationDelay = 100f;

    private void Start()
    {
        Time.fixedDeltaTime = 1f / _tickRate;
    }
    private void FixedUpdate()
    {
        CurrentTick++;
    }
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
    public int TickRate { get => (int)_tickRate; }
    public float ReconcilidationTreshold { get => _reconcolidationTreshold; }
    public float IdleReconcolidationTreshold { get => _idleReconcolidationTreshold; }
    public float ReconcolidationCoolDown { get => _reconcolidationCoolDown; }
    public float ExtrapolationDelay { get => _extrapolationDelay; }
    public int CurrentTick { get; private set; }

    public InputState GetInputState(Vector3 newPosition, Quaternion finalRotation)
    {
        return new(CurrentTick, DateTime.Now, newPosition,  finalRotation);
    }
}
