using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerUI : MonoBehaviour
{
    public TMP_Text PlayerText;
    private Player _player;

    public void SetPlayer(Player player)
    {
        this._player = player;
        PlayerText.text = "Name";
    }
}
