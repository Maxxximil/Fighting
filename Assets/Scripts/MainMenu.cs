using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System;
using System.Security.Cryptography;
using System.Text;

[System.Serializable]
public class Match : NetworkBehaviour
{
    public string ID;
    public readonly List<GameObject> players = new List<GameObject>();

    public Match(string ID, GameObject player)
    {
        this.ID = ID;
        players.Add(player);
    }
}



public class MainMenu : NetworkBehaviour
{
    public static MainMenu Instanse;
    public readonly SyncList<Match> matches = new SyncList<Match>();
    public readonly SyncList<string> matcheIDs = new SyncList<string>();
    public TMP_InputField JoinInput;
    public Button HostButtom;
    public Button JoinButtom;
    public Canvas LobbyCanvas;

    public Transform UILayerParent;
    public GameObject UIPlayerPrefab;
    public TMP_Text IDText;
    public Button BeginGameButton;
    public GameObject TurnManager;
    public bool InGame;

    private void Start()
    {
        Instanse = this;
    }

    private void Update()
    {
        if (!InGame)
        {
            Player[] players = FindObjectsOfType<Player>();
            for(int i = 0; i < players.Length; i++)
            {
                players[i].gameObject.transform.localScale = Vector3.zero;
            }
        }
    }

    public void Host()
    {
        JoinInput.interactable = false;
        HostButtom.interactable = false;
        JoinButtom.interactable = false;

        Player.localPlayer.HostGame();
    }

    public void HostSuccess(bool success, string matchID)
    {
        if (success)
        {
            LobbyCanvas.enabled = true;

            SpawnPlayerUIPrefab(Player.localPlayer);
            IDText.text = matchID;
            BeginGameButton.interactable = true;
        }
        else
        {
            JoinInput.interactable = true;
            HostButtom.interactable = true;
            JoinButtom.interactable = true;
        }
    }

    public void Join()
    {
        JoinInput.interactable = false;
        HostButtom.interactable = false;
        JoinButtom.interactable = false;

        Player.localPlayer.JoinGame(JoinInput.text.ToUpper());
    }

    public void JoinSuccess(bool success, string matchID)
    {
        if (success)
        {
            LobbyCanvas.enabled = true;

            SpawnPlayerUIPrefab(Player.localPlayer);
            IDText.text = matchID;
            BeginGameButton.interactable = false;
        }
        else
        {
            JoinInput.interactable = true;
            HostButtom.interactable = true;
            JoinButtom.interactable = true;
        }
    }

    public bool HostGame(string matchID, GameObject player)
    {
        if (!matcheIDs.Contains(matchID))
        {
            matcheIDs.Add(matchID);
            matches.Add(new Match(matchID, player));
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool JoinGame(string matchID, GameObject player)
    {
        if (matcheIDs.Contains(matchID))
        {
            for(int i = 0; i < matches.Count; i++)
            {
                if (matches[i].ID == matchID)
                {
                    matches[i].players.Add(player);
                    break;
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public static string GetRandomId()
    {
        string ID = string.Empty;
        for(int i = 0; i < 5; i++)
        {
            int rand = UnityEngine.Random.Range(0, 36);
            if (rand < 26)
            {
                ID += (char)(rand + 65);
            }
            else
            {
                ID += (rand - 26).ToString();
            }
        }
        return ID;
    } 

    public void SpawnPlayerUIPrefab(Player player)
    {
        GameObject newUIPlayer = Instantiate(UIPlayerPrefab, UILayerParent);
        newUIPlayer.GetComponent<PlayerUI>().SetPlayer(player);
    }

    public void StartGame()
    {
        Player.localPlayer.BeginGame();
    }

    public void BeginGame(string matchID)
    {
        GameObject newTurnManager = Instantiate(TurnManager);
        NetworkServer.Spawn(newTurnManager);
        newTurnManager.GetComponent<NetworkMatch>().matchId = matchID.ToGuid();
        TurnManager turnManager = newTurnManager.GetComponent<TurnManager>();

        for(int i = 0; i < matches.Count; i++)
        {
            if (matches[i].ID == matchID)
            {
                foreach(var player in matches[i].players)
                {
                    Player player1 = player.GetComponent<Player>();
                    turnManager.AddPlayer(player1);
                    player1.StartGame();
                }
                break;
            }
        }
    }
}

public static class MatchExtention
{
    public static Guid ToGuid(this string id)
    {
        MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
        byte[] inputBytes = Encoding.Default.GetBytes(id);
        byte[] hasBytes = provider.ComputeHash(inputBytes);

        return new Guid(hasBytes);
    }
}
