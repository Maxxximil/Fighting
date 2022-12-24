using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    #region
    private static InputManager _instance;

    public static InputManager Instance
    {
        get
        {
            return _instance;
        }
    }
    #endregion

    private Vector2 movement = new Vector2();
    [SerializeField] private Player _playerObj;
    [SerializeField] private InputField playerName;
    private void Awake()
    {
        _instance = this;
    }

    private void Update()
    {
        if (_playerObj != null)
        {
            MoveInput();
        }
    }

    private void MoveInput()
    {
        movement.x = Input.GetAxis("Horizontal");
        movement.y = Input.GetAxis("Vertical");

       
           // _playerObj.CmdMovePlayer(movement);

        
    }

    public void SetPlayer(Player pl)
    {
        _playerObj = pl;
    }

    public void SpawnPlayer()
    {
        CastomNetworkManager.Instance.SpawnPlayer();
    }

    public void SendName()
    {
        CastomNetworkManager.Instance.SetPlayerName(playerName.text); 
    }
}
