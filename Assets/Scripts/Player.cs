using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Events;

//Скрипт игрока
public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(SyncHealth))][SerializeField] int _synchHealth; //Синхронизируемая переменная здоровья
    [SyncVar] [SerializeField] private float speed = 250;//синх переменная скорости
    [SyncVar] public string matchID;//синх айдишник матча
    [SyncVar(hook = "DisplayPlayerName")] public string PlayerDisplayName;//синх имя

    [SyncVar] public Match CurrentMatch;//синх текущий матч
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
        //получаем нетворкМатч и игровой интерфейс
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
            CmdSendName(MainMenu.Instanse.DisplayName);//отправляем имя
        }
    }

    public override void OnStartServer()//когда сервер запускается шифруем нетАйди и записываем в матчАйди
    {
        netIDGuid = netId.ToString().ToGuid();
        networkMatch.matchId = netIDGuid;
    }

    public override void OnStartClient()//когда клиент запускается
    {
        if (isLocalPlayer)//если локальный игрок
        {
            localPlayer = this;//Ставим локального игрока
        }
        else
        {
            PlayerLobbyUI = MainMenu.Instanse.SpawnPlayerUIPrefab(this);//Иначе создать имя игрока в лобби
        }
    }

    public override void OnStopClient()
    {
        ClientDisconnect();//дисконект клиента
    }

    public override void OnStopServer()
    {
        ServerDisconnect();//дисконект сервера
    }

    [Command]
    void CmdSendColor(int index)
    {
        RpcSendColor(index);//отправка клиентам цвет персонажа
    }

    [ClientRpc]
    void RpcSendColor(int index)
    {
        switch (index)//Изменить на выбранный цвет
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
            CmdSendColor(PlayerPrefs.GetInt("index"));//Если локальный игрок отправить команду на изменение цвета игрока
        }
    }


    [Command]
    public void CmdSendName(string name)
    {
        PlayerDisplayName = name;//записываем в переменную полученное имя
    }

    public void DisplayPlayerName(string name, string playerName)//Вывод имени игрока над персонажем
    {
        name = playerName;
        Debug.Log("Name: " + name + " : " + playerName);
        NameDisplayText.text = playerName;
    }

    public void HostGame(bool publicMatch)//захостить игру
    {
        string ID = MainMenu.GetRandomId();//Получаем рандомный айди для матча
        CmdHostGame(ID, publicMatch);//Отправляем команду на хост
    }

    [Command]
    public void CmdHostGame(string ID, bool publicMatch)
    {
        matchID = ID;
        if (MainMenu.Instanse.HostGame(ID, gameObject, publicMatch))//Если матч удачно создался
        {
            Debug.Log("Lobby create is successfull");
            networkMatch.matchId = ID.ToGuid();//Записываем в матчАйди сгенерированный айдишник
            TargetHostGame(true, ID);//Текущий игрок захостил игру
        }
        else//Если нет, то нет
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
        MainMenu.Instanse.HostSuccess(success, ID);//Актиация холста хоста
    }

    public void JoinGame(string inputID)
    {
        CmdJoinGame(inputID);//Комманда на присоединение игрока по айди
    }

    [Command]
    public void CmdJoinGame(string ID)
    {
        matchID = ID;
        if (MainMenu.Instanse.JoinGame(ID, gameObject))//Проверка на удачное подключение
        {
            Debug.Log("Join to lobby is successfull");
            networkMatch.matchId = ID.ToGuid();//Записываем в совй матчАйди айди матча хоста
            TargetJoinGame(true, ID);//Вызываем на клиенте присоединение
        }
        else//Не удачное подключение
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
        MainMenu.Instanse.JoinSuccess(success, ID);//Активация холста клиента
    }

    public void DisconnectGame()
    {
        CmdDisconnectGame();//Отправка команды на дисконект
    }

    [Command]
    public void CmdDisconnectGame()
    {
        ServerDisconnect();//Отключаем сервер
    }

    void ServerDisconnect()
    {
        MainMenu.Instanse.PlayerDisconnected(gameObject, matchID);//убираем всех игроков по матч айди
        RpcDisconnectGame();//Отправляем всем клиентам дисконект
        networkMatch.matchId = netIDGuid;
    }

    [ClientRpc]
    void RpcDisconnectGame()
    {
        ClientDisconnect();//Каждый клиент дисконнектится
    }

    void ClientDisconnect()
    {
        if(PlayerLobbyUI != null)
        {
            if (!isServer)
            {
                Destroy(PlayerLobbyUI);//Если не сервер уничтожить интерфейс игрока в лобби
            }
            else
            {
                PlayerLobbyUI.SetActive(false);//Иначе, деактивировать
            }
        }
    }

    public void SearchGame()//Поиск игры
    {
        CmdSearchGame();//Комманда поиска
    }

    [Command]
    void CmdSearchGame()
    {
        if(MainMenu.Instanse.SearchGame(gameObject,out matchID))//Если по айди есть матч
        {
            Debug.Log("Game is finding");//игра найдена
            networkMatch.matchId = matchID.ToGuid();//записываем матчАйди
            TargetSearchGame(true, matchID);//Передаем клиенту что матч найден с таким айди

            if(isServer&&PlayerLobbyUI != null)
            {
                PlayerLobbyUI.SetActive(true);
            }
        }
        else//Иначе ошибка
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
        MainMenu.Instanse.SearchGameSuccess(success, ID);//Останавливаем поиск и подключаемся
    }

    [Server]
    public void PlayerCountUpdated(int playerCount)
    {
        TargetPlayerCountUpdated(playerCount);//На сервере обновляем количество игроков
    }

    [TargetRpc]
    void TargetPlayerCountUpdated(int playerCount)//На клиенте проверям количество подкюченных игроков и если больше 1 активируем кнопку
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
        CmdBeginGame();//Команда начала игры
    }

    [Command]
    public void CmdBeginGame()
    {
        MainMenu.Instanse.BeginGame(matchID);//Запускаем игру
        Debug.Log("Game started");
    }

    public void StartGame()
    {
        TargetBeginGame();//Вызываем на клиенте  старт игры
    }

    [TargetRpc]
    void TargetBeginGame()
    {
        Debug.Log($"ID {matchID} | Start");

        Player[] players = FindObjectsOfType<Player>();//Получаем всех игроков и переносим их в донт дестрой он лоад
        for(int i = 0; i < players.Length; i++)
        {
            DontDestroyOnLoad(players[i]);
        }

        SendColor();//Отправляем цвет персонажа
        GameUI.GetComponent<Canvas>().enabled = true;//Активируем игровой холст
        MainMenu.Instanse.InGame = true;//Меняем состояние на "в игре"
        transform.localScale = new Vector3(2, 2, 2);
        SceneManager.LoadScene("Game", LoadSceneMode.Additive);//Загружаем сцену
        _facingRight = true;//Поворачиваемся направо
        _body.simulated = true;//Активируем ригидбади
        inGame = true;//Переъодим в состояние "в игре"
    }


    private void SyncHealth(int oldValue, int newValue)//синхронизация здоровья
    {
        Health = newValue;
    }

    [Command]
    public void CmdChangeHealth(int newValue)
    {
        ChangeHealthValue(newValue);//Изменить ХП
    }

    [Server]
    public void ChangeHealthValue(int newValue)//Изменить здоровье на новое значение
    {
        _synchHealth = newValue;
        Debug.Log("New HP: " + _synchHealth);

        if (_synchHealth <= 0)//Если ХП меньше или равно 0 то вызвать проигрыш у игрока
        {
            Debug.Log("0 hp");
            TargetLoseGame();

        }
    }

    [TargetRpc]
    void TargetLoseGame()
    {
        MainMenu.Instanse.LoseGame();//Активировать холст проигрыша
    }

    [Server]
    void CheckWin()//Проверяем на победу
    {
        Player[] players = FindObjectsOfType<Player>();//получаем всех игроков
        playersInLobby = players.Length;
        for (int i = 0; i < players.Length; i++)//У каждого игрока смотрим сколько ХП
        {
            if (players[i]._synchHealth == 0)//Если у кого-то 0
            {
                playersInLobby--;//Убираем игрока из списка
            }
            Debug.Log("Player " + i + " HP: " + players[i]._synchHealth);
        }
        Debug.Log("Players in Loby: " + playersInLobby);
        if (playersInLobby == 1)//Если остался один игрок
        {
            for (int i = 0; i < players.Length; i++)
            {

                if (players[i]._synchHealth != 0)//И его ХП не равно 0
                {
                    Debug.Log("Winner player number " + i);

                    //RpcWinGame(players[i].netId);
                    //MainMenu.Instanse.WinGame();
                    NetworkIdentity playerIdentity = players[i].GetComponent<NetworkIdentity>();//Получаем его айди
                    TargetWinGame(playerIdentity.connectionToClient);//и отправляем победу
                }
            }
        }
    }

    [Command]
    public void CmdCheckWin()
    {
        CheckWin();//Вызвать проверку победы
    }


    [TargetRpc]
    void TargetWinGame(NetworkConnection target)
    {
        MainMenu.Instanse.WinGame();//Вызвать холст победы для игрока по айди
    }

  

    [Command]
    public void CmdSpawnBullet(uint owner, Vector3 target)
    {
        RpcSpawnBullet(owner, target);//отправить клиентам запрос заспавнить фаербол
    }

    [ClientRpc]
    public void RpcSpawnBullet(uint owner, Vector3 target)
    {
        GameObject bulletGO = Instantiate(BulletPrefab, transform.position, Quaternion.identity);//Спав фаербола
        bulletGO.GetComponent<Fireball>().Init(owner, target);//Инициализация фаербола
    }


    



    void Update()
    {
        if (Health == 0)//если ХП равен 0 неактивен и прекращаем метод Update
        {
            gameObject.SetActive(false);
            return;
        }       

        if (isOwned)//Если есть права на объект
        {
            _anim = GetComponent<Animator>();//получаем аниматор
            Vector2 movement = new Vector2(deltaX, 0);
            transform.Translate(movement);//перемещаемся

            if (Mathf.Abs(deltaX) > 0)//Меняем аниматор
            {
                _anim.SetBool("SetSpeed", true);
            }
            else
            {
                _anim.SetBool("SetSpeed", false);
            }

            //Развороты персонажа
            if (!_facingRight && deltaX > 0)
            {
                Flip();
            }
            else if (_facingRight && deltaX < 0)
            {
                Flip();
            }
            //КД атаки
            if (_atackCD != 0)
            {
                _atackCD -= Time.deltaTime;
            }
            if(_atackCD < 0)
            {
                _atackCD = 0;
            }
        }


        if (Health >= 0)//Если ХП больше или равно 0
        {
            for (int i = 0; i < HealthGos.Length; i++)
            {
                HealthGos[i].SetActive(!(Health - 1 < i));//Девактивируем ХП бары
            }
        }      
    }

    private void Flip()//Разворот персонажа
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
    private void OnTriggerEnter2D(Collider2D collision)//Проигрыш при падении
    {
        if(isOwned && collision.CompareTag("Out"))
        {
            CmdChangeHealth(0);
            CmdCheckWin();
        }
    }

    public void Jump()//прыжок
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

    public void Move(float move)// передвижение
    {
        if (isOwned)
        {
            deltaX = move * speed * Time.deltaTime;        
        }
    }

    public void Atack()//Атака
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
