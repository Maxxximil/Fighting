using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Connections : MonoBehaviour
{
    public NetworkManager _networkManager;

    private void Start()
    {
        if (!Application.isBatchMode)
        {
            _networkManager.StartClient();
        }
    }

    public void JoinClient()
    {
        _networkManager.networkAddress = "localhost";
        _networkManager.StartClient();
    }
}
