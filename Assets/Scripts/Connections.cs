using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

//Скрипт для подключения к айпишникам через метод/кнопку
public class Connections : MonoBehaviour
{
    float searchInterval = 1.5f;
    float currentTime = 1.5f;
    public NetworkManager _networkManager;

    private void Start()
    {
        if (!Application.isBatchMode)
        {
            _networkManager.StartClient();
        }
        //StartCoroutine(Reconnect());
    }

    public void JoinClient()
    {
        //_networkManager.networkAddress = "localhost";
        _networkManager.StartClient();
    }

    private void FixedUpdate()
    {
        if (NetworkClient.active) return;
        if (currentTime > 0)
        {

            currentTime -= Time.deltaTime;
        }
        else
        {
            currentTime = searchInterval;
            _networkManager.StartClient();
        }
    }
}
