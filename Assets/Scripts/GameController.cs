using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameController : NetworkBehaviour
{
    public static GameController Instanse;
    public int PlayersInLobby = 0;
    
    public void CheckWin()
    {
        Player[] players = FindObjectsOfType<Player>();
        int playersInLobby = players.Length;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].Health == 0)
            {
                playersInLobby--;
            }
            Debug.Log("Player " + i + " HP: " + players[i].Health);
        }
        Debug.Log("Players in Loby: " + playersInLobby);
    }

    
}
