using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Events;

//������ ������
public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(SyncHealth))][SerializeField] int _synchHealth; //���������������� ���������� ��������
    [SyncVar] [SerializeField] private float speed = 250;//���� ���������� ��������
    [SyncVar] public string matchID;//���� �������� �����
    [SyncVar(hook = "DisplayPlayerName")] public string PlayerDisplayName;//���� ���

    [SyncVar] public Match CurrentMatch;//���� ������� ����
    public GameObject PlayerLobbyUI;
    private Guid netIDGuid;

    public static Player localPlayer;
    public TMP_Text NameDisplayText;

    public SpriteRenderer CharacterColor;


    public GameObject BulletPrefab;

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
    public int playersInLobby = 0;
    private bool inGame = false;
    private void Awake()
    {
        //�������� ����������� � ������� ���������
        networkMatch = GetComponent<NetworkMatch>();
        GameUI = GameObject.FindGameObjectWithTag("GameUI");
    }
    void Start()
    {

        _body = this.GetComponent<Rigidbody2D>();
        
        _box = GetComponent<BoxCollider2D>();

        _synchHealth = Health;


        if (isLocalPlayer)
        {
            CmdSendName(MainMenu.Instanse.DisplayName);//���������� ���
        }
    }

    public override void OnStartServer()//����� ������ ����������� ������� ������� � ���������� � ��������
    {
        netIDGuid = netId.ToString().ToGuid();
        networkMatch.matchId = netIDGuid;
    }

    public override void OnStartClient()//����� ������ �����������
    {
        if (isLocalPlayer)//���� ��������� �����
        {
            localPlayer = this;//������ ���������� ������
        }
        else
        {
            PlayerLobbyUI = MainMenu.Instanse.SpawnPlayerUIPrefab(this);//����� ������� ��� ������ � �����
        }
    }

    public override void OnStopClient()
    {
        ClientDisconnect();//��������� �������
    }

    public override void OnStopServer()
    {
        ServerDisconnect();//��������� �������
    }

    [Command]
    void CmdSendColor(int index)
    {
        RpcSendColor(index);//�������� �������� ���� ���������
    }

    [ClientRpc]
    void RpcSendColor(int index)
    {
        switch (index)//�������� �� ��������� ����
        {
            case 0:
                CharacterColor.color = Color.white;
                break;
            case 1:
                CharacterColor.color = Color.green;
                break;
            case 2:
                CharacterColor.color = Color.red;
                break;
            case 3:
                CharacterColor.color = Color.gray;
                break;
        }
        
    }

    [Client]
    void SendColor()
    {
        if (isLocalPlayer)
        {
            CmdSendColor(PlayerPrefs.GetInt("index"));//���� ��������� ����� ��������� ������� �� ��������� ����� ������
        }
    }


    [Command]
    public void CmdSendName(string name)
    {
        PlayerDisplayName = name;//���������� � ���������� ���������� ���
    }

    public void DisplayPlayerName(string name, string playerName)//����� ����� ������ ��� ����������
    {
        name = playerName;
        Debug.Log("Name: " + name + " : " + playerName);
        NameDisplayText.text = playerName;
    }

    public void HostGame(bool publicMatch)//��������� ����
    {
        string ID = MainMenu.GetRandomId();//�������� ��������� ���� ��� �����
        CmdHostGame(ID, publicMatch);//���������� ������� �� ����
    }

    [Command]
    public void CmdHostGame(string ID, bool publicMatch)
    {
        matchID = ID;
        if (MainMenu.Instanse.HostGame(ID, gameObject, publicMatch))//���� ���� ������ ��������
        {
            Debug.Log("Lobby create is successfull");
            networkMatch.matchId = ID.ToGuid();//���������� � �������� ��������������� ��������
            TargetHostGame(true, ID);//������� ����� �������� ����
        }
        else//���� ���, �� ���
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
        MainMenu.Instanse.HostSuccess(success, ID);//�������� ������ �����
    }

    public void JoinGame(string inputID)
    {
        CmdJoinGame(inputID);//�������� �� ������������� ������ �� ����
    }

    [Command]
    public void CmdJoinGame(string ID)
    {
        matchID = ID;
        if (MainMenu.Instanse.JoinGame(ID, gameObject))//�������� �� ������� �����������
        {
            Debug.Log("Join to lobby is successfull");
            networkMatch.matchId = ID.ToGuid();//���������� � ���� �������� ���� ����� �����
            TargetJoinGame(true, ID);//�������� �� ������� �������������
        }
        else//�� ������� �����������
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
        MainMenu.Instanse.JoinSuccess(success, ID);//��������� ������ �������
    }

    public void DisconnectGame()
    {
        CmdDisconnectGame();//�������� ������� �� ���������
    }

    [Command]
    public void CmdDisconnectGame()
    {
        ServerDisconnect();//��������� ������
    }

    void ServerDisconnect()
    {
        MainMenu.Instanse.PlayerDisconnected(gameObject, matchID);//������� ���� ������� �� ���� ����
        RpcDisconnectGame();//���������� ���� �������� ���������
        networkMatch.matchId = netIDGuid;
    }

    [ClientRpc]
    void RpcDisconnectGame()
    {
        ClientDisconnect();//������ ������ ��������������
    }

    void ClientDisconnect()
    {
        if(PlayerLobbyUI != null)
        {
            if (!isServer)
            {
                Destroy(PlayerLobbyUI);//���� �� ������ ���������� ��������� ������ � �����
            }
            else
            {
                PlayerLobbyUI.SetActive(false);//�����, ��������������
            }
        }
    }

    public void SearchGame()//����� ����
    {
        CmdSearchGame();//�������� ������
    }

    [Command]
    void CmdSearchGame()
    {
        if(MainMenu.Instanse.SearchGame(gameObject,out matchID))//���� �� ���� ���� ����
        {
            Debug.Log("Game is finding");//���� �������
            networkMatch.matchId = matchID.ToGuid();//���������� ��������
            TargetSearchGame(true, matchID);//�������� ������� ��� ���� ������ � ����� ����

            if(isServer&&PlayerLobbyUI != null)
            {
                PlayerLobbyUI.SetActive(true);
            }
        }
        else//����� ������
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
        MainMenu.Instanse.SearchGameSuccess(success, ID);//������������� ����� � ������������
    }

    [Server]
    public void PlayerCountUpdated(int playerCount)
    {
        TargetPlayerCountUpdated(playerCount);//�� ������� ��������� ���������� �������
    }

    [TargetRpc]
    void TargetPlayerCountUpdated(int playerCount)//�� ������� �������� ���������� ����������� ������� � ���� ������ 1 ���������� ������
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
        CmdBeginGame();//������� ������ ����
    }

    [Command]
    public void CmdBeginGame()
    {
        MainMenu.Instanse.BeginGame(matchID);//��������� ����
        Debug.Log("Game started");
    }

    public void StartGame()
    {
        TargetBeginGame();//�������� �� �������  ����� ����
    }

    [TargetRpc]
    void TargetBeginGame()
    {
        Debug.Log($"ID {matchID} | Start");

        Player[] players = FindObjectsOfType<Player>();//�������� ���� ������� � ��������� �� � ���� ������� �� ����
        for(int i = 0; i < players.Length; i++)
        {
            DontDestroyOnLoad(players[i]);
        }

        SendColor();//���������� ���� ���������
        GameUI.GetComponent<Canvas>().enabled = true;//���������� ������� �����
        MainMenu.Instanse.InGame = true;//������ ��������� �� "� ����"
        transform.localScale = new Vector3(2, 2, 2);
        SceneManager.LoadScene("Game", LoadSceneMode.Additive);//��������� �����
        _facingRight = true;//�������������� �������
        _body.simulated = true;//���������� ���������
        inGame = true;//��������� � ��������� "� ����"
    }


    private void SyncHealth(int oldValue, int newValue)//������������� ��������
    {
        Health = newValue;
    }

    [Command]
    public void CmdChangeHealth(int newValue)
    {
        ChangeHealthValue(newValue);//�������� ��
    }

    [Server]
    public void ChangeHealthValue(int newValue)//�������� �������� �� ����� ��������
    {
        _synchHealth = newValue;
        Debug.Log("New HP: " + _synchHealth);

        if (_synchHealth <= 0)//���� �� ������ ��� ����� 0 �� ������� �������� � ������
        {
            Debug.Log("0 hp");
            TargetLoseGame();

        }
    }

    [TargetRpc]
    void TargetLoseGame()
    {
        MainMenu.Instanse.LoseGame();//������������ ����� ���������
    }

    [Server]
    void CheckWin()//��������� �� ������
    {
        Player[] players = FindObjectsOfType<Player>();//�������� ���� �������
        playersInLobby = players.Length;
        for (int i = 0; i < players.Length; i++)//� ������� ������ ������� ������� ��
        {
            if (players[i]._synchHealth == 0)//���� � ����-�� 0
            {
                playersInLobby--;//������� ������ �� ������
            }
            Debug.Log("Player " + i + " HP: " + players[i]._synchHealth);
        }
        Debug.Log("Players in Loby: " + playersInLobby);
        if (playersInLobby == 1)//���� ������� ���� �����
        {
            for (int i = 0; i < players.Length; i++)
            {

                if (players[i]._synchHealth != 0)//� ��� �� �� ����� 0
                {
                    Debug.Log("Winner player number " + i);

                    //RpcWinGame(players[i].netId);
                    //MainMenu.Instanse.WinGame();
                    NetworkIdentity playerIdentity = players[i].GetComponent<NetworkIdentity>();//�������� ��� ����
                    TargetWinGame(playerIdentity.connectionToClient);//� ���������� ������
                }
            }
        }
    }

    [Command]
    public void CmdCheckWin()
    {
        CheckWin();//������� �������� ������
    }


    [TargetRpc]
    void TargetWinGame(NetworkConnection target)
    {
        MainMenu.Instanse.WinGame();//������� ����� ������ ��� ������ �� ����
    }

  

    [Command]
    public void CmdSpawnBullet(uint owner, Vector3 target)
    {
        RpcSpawnBullet(owner, target);//��������� �������� ������ ���������� �������
    }

    [ClientRpc]
    public void RpcSpawnBullet(uint owner, Vector3 target)
    {
        GameObject bulletGO = Instantiate(BulletPrefab, transform.position, Quaternion.identity);//���� ��������
        bulletGO.GetComponent<Fireball>().Init(owner, target);//������������� ��������
    }


    



    void Update()
    {
        if (Health == 0)//���� �� ����� 0 ��������� � ���������� ����� Update
        {
            gameObject.SetActive(false);
            return;
        }       

        if (isOwned)//���� ���� ����� �� ������
        {
            _anim = GetComponent<Animator>();//�������� ��������
            Vector2 movement = new Vector2(deltaX, 0);
            transform.Translate(movement);//������������

            if (Mathf.Abs(deltaX) > 0)//������ ��������
            {
                _anim.SetBool("SetSpeed", true);
            }
            else
            {
                _anim.SetBool("SetSpeed", false);
            }

            //��������� ���������
            if (!_facingRight && deltaX > 0)
            {
                Flip();
            }
            else if (_facingRight && deltaX < 0)
            {
                Flip();
            }
            //�� �����
            if (_atackCD != 0)
            {
                _atackCD -= Time.deltaTime;
            }
            if(_atackCD < 0)
            {
                _atackCD = 0;
            }
        }


        if (Health >= 0)//���� �� ������ ��� ����� 0
        {
            for (int i = 0; i < HealthGos.Length; i++)
            {
                HealthGos[i].SetActive(!(Health - 1 < i));//������������� �� ����
            }
        }      
    }

    private void Flip()//�������� ���������
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
    private void OnTriggerEnter2D(Collider2D collision)//�������� ��� �������
    {
        if(isOwned && collision.CompareTag("Out"))
        {
            CmdChangeHealth(0);
            CmdCheckWin();
        }
    }

    public void Jump()//������
    {       
        if (isOwned)
        {
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

    public void Move(float move)// ������������
    {
        if (isOwned)
        {
            deltaX = move * speed * Time.deltaTime;        
        }
    }

    public void Atack()//�����
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
                pos.x += 7f;
            }
            else
            {
                pos.x -= 7f;
            }
            pos.z = 10f;
            _atackCD = 0.5f;
            //pos = Camera.main.ScreenToWorldPoint(pos);
            CmdSpawnBullet(netId, pos);
        }
    }
}
