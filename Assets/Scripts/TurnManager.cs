using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TurnManager : MonoBehaviour
{
    private List<Player> _players = new List<Player>();

    public void AddPlayer(Player player)
    {
        _players.Add(player);
    }

}
