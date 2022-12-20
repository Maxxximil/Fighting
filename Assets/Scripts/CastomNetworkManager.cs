using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CastomNetworkManager : NetworkManager
{
    #region
    private static CastomNetworkManager _instance;

    public static CastomNetworkManager Instance
    {
        get
        {
            return _instance;
        }
    }
    #endregion

    private string _playerName;



    public override void Awake()
    {
        _instance = this;
    }

    public void SetPlayerName(string name)
    {
        _playerName = name;
    }

    public override void OnClientConnect()
    {
        //base.OnClientConnect();
        UIManager.Instance.SpawnGroupToogle();
    }

    public void SpawnPlayer()
    {

        if (string.IsNullOrWhiteSpace(_playerName)) return;
        if (!clientLoadedScene)
        {
            // Ready/AddPlayer is usually triggered by a scene load completing.
            // if no scene was loaded, then Ready/AddPlayer it here instead.
            if (!NetworkClient.ready)
                NetworkClient.Ready();

            if (autoCreatePlayer)
                NetworkClient.AddPlayer();
        }
    }
}
