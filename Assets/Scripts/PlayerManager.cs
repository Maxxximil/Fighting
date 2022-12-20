using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private InputField _name;

    #region
    private static PlayerManager _instance;

    public static PlayerManager Instance
    {
        get
        {
            return _instance;
        }
    }
    #endregion


    private string _playerName;

    public string PlayerName { get; private set; }

    private void Awake()
    {
        _instance = this;
        PlayerName = "";
    }

    public void SetPlayerName()
    {
        _playerName = _name.text;
        PlayerName = _playerName;
    }

    
   
    
}
