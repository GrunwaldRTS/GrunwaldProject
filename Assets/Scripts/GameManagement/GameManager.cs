using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.MessageBox;

public class GameManager : NetworkBehaviour
{
    [SerializeField]
    NetworkVariable<PlayerData> _playerData = new(new PlayerData(100, 100, 100));
    public PlayerData PlayerData { 
        get =>_playerData.Value; 
        private set
        {
            if (IsServer)
            {
                _playerData.Value = value;
            }
        }
    }
    private void Awake()
    {

    }
}
