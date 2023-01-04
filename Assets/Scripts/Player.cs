using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(SyncHealth))][SerializeField] int _synchHealth;
    [SyncVar] [SerializeField] private float speed = 250;
    [SyncVar] public string matchID;
    [SyncVar(hook = "DisplayPlayerName")] public string PlayerDisplayName;

    [SyncVar] public Match CurrentMatch;
    public GameObject PlayerLobbyUI;
    private Guid netIDGuid;

    public static Player localPlayer;
    public TMP_Text NameDisplayText;




    public GameObject BulletPrefab;

    //public float speed = 250.0f;
    public float jumpForce = 12.0f;
    public int Health;
    public string Name;
    public GameObject[] HealthGos;
    public TMP_Text PlayerName;


    private GameObject GameUI;
    private NetworkMatch networkMatch;
    private Rigidbody2D _body;
    private Animator _anim;
    private BoxCollider2D _box;
    private bool _grounded = false;
    private bool _jump = false;
    private bool _facingRight = true;
    private bool _isMoved = false;
    private float deltaX = 0;
    private float _atackCD = 0;

    private void Awake()
    {
        networkMatch = GetComponent<NetworkMatch>();
        GameUI = GameObject.FindGameObjectWithTag("GameUI");
    }
    void Start()
    {
       

        _body = this.GetComponent<Rigidbody2D>();
        
        _box = GetComponent<BoxCollider2D>();


        if (isLocalPlayer)
        {
            CmdSendName(MainMenu.Instanse.DisplayName);
        }
    }

    public override void OnStartServer()
    {
        netIDGuid = netId.ToString().ToGuid();
        networkMatch.matchId = netIDGuid;
    }

    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            localPlayer = this;
        }
        else
        {
            PlayerLobbyUI = MainMenu.Instanse.SpawnPlayerUIPrefab(this);
        }
    }

    public override void OnStopClient()
    {
        ClientDisconnect();
    }

    public override void OnStopServer()
    {
        ServerDisconnect();
    }

    [Command]
    public void CmdSendName(string name)
    {
        PlayerDisplayName = name;
    }

    public void DisplayPlayerName(string name, string playerName)
    {
        name = playerName;
        Debug.Log("Name: " + name + " : " + playerName);
        NameDisplayText.text = playerName;
    }

    public void HostGame(bool publicMatch)
    {
        string ID = MainMenu.GetRandomId();
        CmdHostGame(ID, publicMatch);
    }

    [Command]
    public void CmdHostGame(string ID, bool publicMatch)
    {
        matchID = ID;
        if (MainMenu.Instanse.HostGame(ID, gameObject, publicMatch))
        {
            Debug.Log("Lobby create is successfull");
            networkMatch.matchId = ID.ToGuid();
            TargetHostGame(true, ID);
        }
        else
        {
            Debug.Log("Lobby create error");
            TargetHostGame(false, ID);
        }
    }

    [TargetRpc]
    void TargetHostGame(bool success, string ID)
    {
        matchID = ID;
        Debug.Log($"ID {matchID} == {ID}");
        MainMenu.Instanse.HostSuccess(success, ID);
    }

    public void JoinGame(string inputID)
    {
        CmdJoinGame(inputID);
    }

    [Command]
    public void CmdJoinGame(string ID)
    {
        matchID = ID;
        if (MainMenu.Instanse.JoinGame(ID, gameObject))
        {
            Debug.Log("Join to lobby is successfull");
            networkMatch.matchId = ID.ToGuid();
            TargetJoinGame(true, ID);
        }
        else
        {
            Debug.Log("Join to lobby error");
            TargetJoinGame(false, ID);
        }
    }

    [TargetRpc]
    void TargetJoinGame(bool success, string ID)
    {
        matchID = ID;
        Debug.Log($"ID {matchID} == {ID}");
        MainMenu.Instanse.JoinSuccess(success, ID);
    }

    public void DisconnectGame()
    {
        CmdDisconnectGame();
    }

    [Command]
    public void CmdDisconnectGame()
    {
        ServerDisconnect();
    }

    void ServerDisconnect()
    {
        MainMenu.Instanse.PlayerDisconnected(gameObject, matchID);
        RpcDisconnectGame();
        networkMatch.matchId = netIDGuid;
    }

    [ClientRpc]
    void RpcDisconnectGame()
    {
        ClientDisconnect();
    }

    void ClientDisconnect()
    {
        if(PlayerLobbyUI != null)
        {
            if (!isServer)
            {
                Destroy(PlayerLobbyUI);
            }
            else
            {
                PlayerLobbyUI.SetActive(false);
            }
        }
    }

    public void SearchGame()
    {
        CmdSearchGame();
    }

    [Command]
    void CmdSearchGame()
    {
        if(MainMenu.Instanse.SearchGame(gameObject,out matchID))
        {
            Debug.Log("Game is finding");
            networkMatch.matchId = matchID.ToGuid();
            TargetSearchGame(true, matchID);

            if(isServer&&PlayerLobbyUI != null)
            {
                PlayerLobbyUI.SetActive(true);
            }
        }
        else
        {
            Debug.Log("Game found not success");
            TargetSearchGame(false, matchID);


        }
    }

    [TargetRpc]
    void TargetSearchGame(bool success, string ID)
    {
        matchID = ID;
        Debug.Log("ID: " + matchID + "==" + ID + " | " + success);
        MainMenu.Instanse.SearchGameSuccess(success, ID);
    }

    [Server]
    public void PlayerCountUpdated(int playerCount)
    {
        TargetPlayerCountUpdated(playerCount);
    }

    [TargetRpc]
    void TargetPlayerCountUpdated(int playerCount)
    {
        if (playerCount > 1)
        {
            MainMenu.Instanse.SetBeginButtonActive(true);
        }
        else
        {
            MainMenu.Instanse.SetBeginButtonActive(false);
        }
    }

    public void BeginGame()
    {
        CmdBeginGame();
    }

    [Command]
    public void CmdBeginGame()
    {
        MainMenu.Instanse.BeginGame(matchID);
        Debug.Log("Game started");
    }

    public void StartGame()
    {
        TargetBeginGame();
    }

    [TargetRpc]
    void TargetBeginGame()
    {
        Debug.Log($"ID {matchID} | Start");

        Player[] players = FindObjectsOfType<Player>();
        for(int i = 0; i < players.Length; i++)
        {
            DontDestroyOnLoad(players[i]);
        }

        GameUI.GetComponent<Canvas>().enabled = true;
        MainMenu.Instanse.InGame = true;
        transform.localScale = new Vector3(2, 2, 2);
        SceneManager.LoadScene("Game", LoadSceneMode.Additive);
        _facingRight = true;
        _body.simulated = true;

    }


    private void SyncHealth(int oldValue, int newValue)
    {
        Health = newValue;
    }

    

    [Server]
    public void ChangeHealthValue(int newValue)
    {
        _synchHealth = newValue;

        if (_synchHealth <= 0)
        {
            UIController.Instance.LoseScreenEnable();
            NetworkServer.Destroy(gameObject);
        }
    }

    [Server]
    public void SpawnBullet(uint owner, Vector3 target)
    {
        Debug.Log("SpawnBullet");
        GameObject bulletGO = Instantiate(BulletPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(bulletGO);
        bulletGO.GetComponent<Bullet>().Init(owner, target);
    }

    //[Server]
    //public void OutOfMap()
    //{
    //    NetworkServer.Destroy(gameObject);
    //}


    //[Command]
    //public void CmdOutOfMap()
    //{
    //    OutOfMap();
    //}

    [Command]
    public void CmdSpawnBullet(uint owner, Vector3 target)
    {
        Debug.Log("CmdSpawnBullet");

        RpcSpawnBullet(owner, target);
        //MainMenu.Instanse.SpawnFirebal(matchID,transform.position,owner,target);

        //SpawnBullet(owner, target);
        //GameObject bulletGO = Instantiate(BulletPrefab, transform.position, Quaternion.identity);
        //NetworkServer.Spawn(bulletGO);
        //bulletGO.GetComponent<Bullet>().Init(owner, target);
    }

    [ClientRpc]
    public void RpcSpawnBullet(uint owner, Vector3 target)
    {
        GameObject bulletGO = Instantiate(BulletPrefab, transform.position, Quaternion.identity);
        //NetworkServer.Spawn(bulletGO);
        bulletGO.GetComponent<Fireball>().Init(owner, target);
    }


    [Command]
    public void CmdChangeHealth(int newValue)
    {
        ChangeHealthValue(newValue);
    }

    


    void Update()
    {

        if (isOwned)
        {
            
            _anim = GetComponent<Animator>();
            #region Movement
            //float deltaX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
            Vector2 movement = new Vector2(deltaX, 0);
            transform.Translate(movement);

            if (Mathf.Abs(deltaX) > 0)
            {
                _anim.SetBool("SetSpeed", true);
            }
            else
            {
                _anim.SetBool("SetSpeed", false);
            }

            Vector3 max = _box.bounds.max;
            Vector3 min = _box.bounds.min;
            Vector2 corner1 = new Vector2(max.x, min.y - .1f);
            Vector2 corner2 = new Vector2(min.x, min.y - .2f);
            Collider2D hit = Physics2D.OverlapArea(corner1, corner2);
            _grounded = true;
            if (hit == null)
            {
                _grounded = false;
            }
            _body.gravityScale = _grounded && deltaX == 0 ? 0 : 1;
            if (_grounded && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)))
            {
                _body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
            if (_grounded && _jump)
            {
                _body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                _jump = false;
            }

            #endregion

            if (!_facingRight && deltaX > 0)
            {
                Flip();
            }
            else if (_facingRight && deltaX < 0)
            {
                Flip();
            }

            if (_atackCD != 0)
            {
                Debug.Log("CD: " + _atackCD);
                _atackCD -= Time.deltaTime;
            }
            if(_atackCD < 0)
            {
                _atackCD = 0;
            }


            //if (Input.GetKeyDown(KeyCode.H))
            //{
            //    if (isServer)
            //    {
            //        ChangeHealthValue(Health - 1);
            //    }
            //    else
            //    {
            //        CmdChangeHealth(Health-1);
            //    }
            //}

            //if (Input.GetKeyDown(KeyCode.Mouse1))
            //{
            //    Debug.Log("Attack");
            //    _anim.SetTrigger("Attack");
            //    Vector3 pos = Input.mousePosition;
            //    Debug.Log("Attack pos " + pos);
            //    pos.z = 10f;
            //    pos = Camera.main.ScreenToWorldPoint(pos);
            //    Debug.Log("Attack pos after camera " + pos);

            //    //CmdSpawnBullet(netId, pos);

            //    //GameObject bulletGO = Instantiate(BulletPrefab, transform.position, Quaternion.identity);
            //    //NetworkServer.Spawn(bulletGO);
            //    //bulletGO.GetComponent<Bullet>().Init(netId, pos);
            //    //bulletGO.GetComponent<Bullet>().Init(netId, pos);



            //    if (isServer)
            //    {
            //        SpawnBullet(netId, pos);
            //    }
            //    else
            //    {
            //        CmdSpawnBullet(netId, pos);

            //    }
            //}


        }
        

        for (int i = 0; i < HealthGos.Length; i++)
        {
            HealthGos[i].SetActive(!(Health - 1 < i));
        }     
    }

    private void Flip()
    {
        if (hasAuthority)
        {
            _facingRight = !_facingRight;
            Vector3 Scale = transform.localScale;
            Scale.x *= -1;
            transform.localScale = Scale;
            if (MainMenu.Instanse.InGame)
            {
                Vector3 TextScale = NameDisplayText.transform.localScale;
                TextScale.x *= -1;
                NameDisplayText.transform.localScale = TextScale; 
            }
            
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        MainMenu.Instanse.Disconect();
        //if(isOwned && collision.CompareTag("Out"))
        //{
        //    if (isServer)
        //    {
        //        OutOfMap();
        //    }
        //    else
        //    {
        //        CmdOutOfMap();
        //    }
        //}
    }

    public void Jump()
    {
        //Debug.Log("Button");
        
        if (isOwned)
        {
            //Debug.Log("Jump");
            Vector3 max = _box.bounds.max;
            Vector3 min = _box.bounds.min;
            Vector2 corner1 = new Vector2(max.x, min.y - .1f);
            Vector2 corner2 = new Vector2(min.x, min.y - .2f);
            Collider2D hit = Physics2D.OverlapArea(corner1, corner2);
            _grounded = true;
            if (hit == null)
            {
                _grounded = false;
            }
            if (_grounded)
            {
                _body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }
    }

    public void Left()
    {
        float deltaX = -10 * speed * Time.deltaTime;
        Vector2 movement = new Vector2(deltaX, 0);
        transform.Translate(movement);
        _isMoved = true;
    }

    public void Right()
    {
        float deltaX = 10 * speed * Time.deltaTime;
        Vector2 movement = new Vector2(deltaX, 0);
        transform.Translate(movement);
        _isMoved = true;
    }

    public void Move(float move)
    {
        if (isOwned)
        {
            deltaX = move * speed * Time.deltaTime;        
        }
    }

    public void Atack()
    {
        if (isOwned && _atackCD == 0)
        {
            if (deltaX != 0)
            {
                _anim.SetTrigger("Attack");
            }
            Vector3 pos = /*Vector3.forward*/transform.position;
            if (_facingRight)
            {
                pos.x += 6f;
            }
            else
            {
                pos.x -= 6f;
            }
            pos.z = 10f;
            _atackCD = 0.5f;
            //pos = Camera.main.ScreenToWorldPoint(pos);
            CmdSpawnBullet(netId, pos);
        }
    }
}
