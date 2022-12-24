using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;
public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(SyncHealth))][SerializeField] int _synchHealth;
    [SyncVar(hook = nameof(SyncName))] [SerializeField] string _synchName;
    [SyncVar]
    [SerializeField]
    private float speed = 250;
    [SyncVar] public string matchID;


    public static Player localPlayer;



    public GameObject BulletPrefab;

    //public float speed = 250.0f;
    public float jumpForce = 12.0f;
    public int Health;
    public string Name;
    public GameObject[] HealthGos;
    public TMP_Text PlayerName;


    private NetworkMatch networkMatch;
    private Rigidbody2D _body;
    private Animator _anim;
    private BoxCollider2D _box;
    private bool _grounded = false;
    private bool _jump = false;
    private bool _facingRight = true;
    void Start()
    {
       

        _body = this.GetComponent<Rigidbody2D>();
        
        _box = GetComponent<BoxCollider2D>();

        networkMatch = GetComponent<NetworkMatch>();

        if (isLocalPlayer)
        {
            localPlayer = this;
        }
        else
        {
            MainMenu.Instanse.SpawnPlayerUIPrefab(this);
        }
    }


    public void HostGame()
    {
        string ID = MainMenu.GetRandomId();
        CmdHostGame(ID);
    }

    [Command]
    public void CmdHostGame(string ID)
    {
        matchID = ID;
        if (MainMenu.Instanse.HostGame(ID, gameObject))
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
        DontDestroyOnLoad(gameObject);
        MainMenu.Instanse.InGame = true;
        transform.localScale = new Vector3(2, 2, 2);
        SceneManager.LoadScene("Game", LoadSceneMode.Additive);
        _facingRight = true;
        _body.simulated = true;

    }


    public void NewName()
    {
        if (isOwned)
        {
            if (isServer)
            {
                Debug.Log("IsServerNewName");
                SetPlayerName(PlayerManager.Instance.PlayerName);
            }
            else
            {
                Debug.Log("ElseNewName");
                CmdSetPlayerName(PlayerManager.Instance.PlayerName);
            }

            //CmdSetPlayerName(PlayerManager.Instance.PlayerName);
        }

    }



    private void SyncName(string oldValue, string newValue)
    {
        Name = newValue;
    }

    private void SyncHealth(int oldValue, int newValue)
    {
        Health = newValue;
    }

    private void ShowNamePlayer()
    {
        PlayerName.text = _synchName;
    }

    [Server]
    public void ChangeHealthValue(int newValue)
    {
        _synchHealth = newValue;

        if (_synchHealth <= 0)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    [Server]
    public void SpawnBullet(uint owner, Vector3 target)
    {
        GameObject bulletGO = Instantiate(BulletPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(bulletGO);
        bulletGO.GetComponent<Bullet>().Init(owner, target);
    }

    [Server]
    public void OutOfMap()
    {
        NetworkServer.Destroy(gameObject);
    }


    [Server]
    public void SetPlayerName(string newName)
    {
        Debug.Log("SetPlayerName " + newName);
        _synchName = newName;
        ShowNamePlayer();
    }

    [Command]
    public void CmdOutOfMap()
    {
        OutOfMap();
    }

    [Command]
    public void CmdSpawnBullet(uint owner, Vector3 target)
    {
        SpawnBullet(owner, target);
    }

    [Command]
    public void CmdChangeHealth(int newValue)
    {
        ChangeHealthValue(newValue);
    }

    [Command]
    public void CmdSetPlayerName(string newName)
    {
        SetPlayerName(newName);
        RpcSetPlayerName(newName);
        Debug.Log("CmdSetPlayerName " + newName);
        //_synchName = newName;
        //PlayerName.text = _synchName;
    }

    [ClientRpc]
    public void RpcSetPlayerName(string newName)
    {
        Debug.Log("RpcSetPlayerName " + newName);
        _synchName = newName;
        ShowNamePlayer();
    }


    void Update()
    {
        if (PlayerName.text != _synchName) ShowNamePlayer();

        if (isOwned)
        {
            _anim = GetComponent<Animator>();
            #region Movement
            float deltaX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
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

            //if (deltaX != 0)
            //{
            //    transform.localScale = new Vector3(Mathf.Sign(deltaX) * 2, 2, 1);
            //}

            #endregion

            if (!_facingRight && deltaX > 0)
            {
                Flip();
            }
            else if (_facingRight && deltaX < 0)
            {
                Flip();
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                if (isServer)
                {
                    ChangeHealthValue(Health - 1);
                }
                else
                {
                    CmdChangeHealth(Health-1);
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                Vector3 pos = Input.mousePosition;
                pos.z = 10f;
                pos = Camera.main.ScreenToWorldPoint(pos);

                if (isServer)
                    SpawnBullet(netId, pos);
                else
                    CmdSpawnBullet(netId, pos);
            }

            
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
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(isOwned && collision.CompareTag("Out"))
        {
            if (isServer)
            {
                OutOfMap();
            }
            else
            {
                CmdOutOfMap();
            }
        }
    }

    //public void Jump()
    //{
    //    Debug.Log("Button");
    //    //_jump = true;
    //    if (/*_grounded && */isOwned)
    //    {
    //        Debug.Log("Jump");
    //        _body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    //    }
    //}
}
