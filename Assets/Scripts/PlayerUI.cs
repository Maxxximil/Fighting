using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Вывод имени игрока в лобби
public class PlayerUI : MonoBehaviour
{
    public TMP_Text PlayerText;

    public void SetPlayer(string name)
    {
        
        PlayerText.text = name;
    }
}
