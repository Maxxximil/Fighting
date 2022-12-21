using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class NetMan : NetworkManager
{
    #region
    private static NetMan _instance;

    public static NetMan Instance
    {
        get
        {
            return _instance;
        }
    }
    #endregion



    private bool _playerSpawned;
    private bool _playerConnected;
    private string _playerName;
    private Player _playerObj;


    public override void Awake()
    {
        _instance = this;
    }

    public void SetPlayer(Player pl1)
    {
        _playerObj = pl1;
        SetNamePlayer();
    }

    public void SetNamePlayer()
    {
        _playerObj.NewName();
    }

    public override void OnClientConnect()
    {
        //base.OnClientConnect();
        _playerConnected = true;
        UIManager.Instance.SpawnGroupToogle();
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<PosMessage>(OnCreateCharacter);
    }

    public void OnCreateCharacter(NetworkConnectionToClient conn, PosMessage message)
    {
        GameObject go = Instantiate(playerPrefab, message.vector2, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, go);
    }

    public override void Update()
    {
        base.Update();
        //if(Input.GetKeyDown(KeyCode.Mouse0)&&!_playerSpawned && _playerConnected)
        //{
        //    ActivatePlayerSpawn();
        //}
    }


    public void SpawnPlayer()
    {
        _playerName = PlayerManager.Instance.PlayerName;
        UIManager.Instance.ChangePlayerText(_playerName);
        //if (string.IsNullOrWhiteSpace(_playerName)) return;

        if (!clientLoadedScene)
        {
            // Ready/AddPlayer is usually triggered by a scene load completing.
            // if no scene was loaded, then Ready/AddPlayer it here instead.
            if (!NetworkClient.ready)
                NetworkClient.Ready();

            //if (autoCreatePlayer)
            //    NetworkClient.AddPlayer();
            NetworkClient.AddPlayer();

        }
        //if (!NetworkClient.ready && NetworkClient.isConnected)
        //{
        //    NetworkClient.Ready();
        //    if (NetworkClient.localPlayer == null)
        //    {
        //        NetworkClient.AddPlayer();
        //    }
        //}

        //if (NetworkClient.localPlayer == null)
        //{
        //    NetworkClient.AddPlayer();
        //}



        //if (autoCreatePlayer)
        //    NetworkClient.AddPlayer();
        //PosMessage pos = new PosMessage() { vector2 = new Vector2(0, 0) };

        //NetworkClient.Send(pos);
        _playerSpawned = true;
        UIManager.Instance.SpawnGroupToogle();
    }

}

public struct PosMessage : NetworkMessage
{
    public Vector2 vector2;
}
