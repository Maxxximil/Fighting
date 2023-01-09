using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System;
using System.Security.Cryptography;
using System.Text;

[System.Serializable]
public class Match//класс матча в котор хранятся
{
    public string ID;//айди
    public bool PublicMatch;//публичный матч или нет
    public bool InMatch;//В игре или нет
    public bool MatchFull;//Полный ли матч
    public int PlayersInMatch;//Сколько игроков в мачте

    public List<GameObject> players = new List<GameObject>();//количество игроков

    public Match(string ID, GameObject player, bool publicMatch)//конструктор матча
    {
        InMatch = false;
        MatchFull = false;
        this.ID = ID;
        PublicMatch = publicMatch;
        PlayersInMatch = 1;
        players.Add(player);
    }

    public Match()
    {

    }
}



public class MainMenu : NetworkBehaviour
{
    public static MainMenu Instanse;
    public readonly SyncList<Match> matches = new SyncList<Match>();
    public readonly SyncList<string> matcheIDs = new SyncList<string>();
    public int MaxPlayers;
    
    private NetworkManager _networkManager;

    [Header("MainMenu")]
    public TMP_InputField JoinInput;
    public Button[] buttons;
    public Canvas LobbyCanvas;
    public Canvas SearchCanvas;
    private bool _searching;

    [Header("Name")]
    public GameObject ChangeNamePanel;
    public GameObject CloseButton;
    public Button SetNameButton;
    public TMP_InputField NameInput;
    public int FirstTime = 1;
    [SyncVar] public string DisplayName;



    [Header("Lobby")]
    public Transform UILayerParent;
    public GameObject UIPlayerPrefab;
    public TMP_Text IDText;
    public Button BeginGameButton;
    public GameObject localPlayerLobbyUI;
    public bool InGame;


    [Header("Error")]
    public GameObject ErrorPanel;
    public TMP_Text ErrorText;

    [Header("Character")]
    public Button ChooseButton;
    public Transform PreviewParent;
    public TMP_Text NameText;
    public List<Character> Characters;
    public TMP_Text CoinsText;
    public int ChangeNameCost;
    public int Coins;
    private int index;
    private List<GameObject> previewCharacters = new List<GameObject>();

    [Header("Game")]
    public GameObject LoseScreen;
    public GameObject WinScreen;



    private void Start()
    {
        Instanse = this;

        _networkManager = FindObjectOfType<NetworkManager>();

        FirstTime = PlayerPrefs.GetInt("FirstTime", 1);//Заходили ли мы в игру до этого
        Coins = PlayerPrefs.GetInt("Coins", Coins);//Получаем количество монет


        if (PlayerPrefs.HasKey("index"))//Если есть индекс цвета, получаем его
        {
            index = PlayerPrefs.GetInt("index");
        }

        foreach(var character in Characters)//Создаем превью скинов
        {
            GameObject previewCharacter = Instantiate(character.PreviewObj, PreviewParent);
            previewCharacter.SetActive(false);
            previewCharacters.Add(previewCharacter);
        }

        if (index == PlayerPrefs.GetInt("index"))//Если индекс скина совпадает с цветом игрока, то деактивируем кнопку
        {
            ChooseButton.GetComponent<Image>().color = Color.white;
            ChooseButton.interactable = false;
            ChooseButton.GetComponentInChildren<TMP_Text>().text = "Chosen";
        }
        else//иначе выводим стоимость и возможность купить
        {
            if (Characters[index].purchased == 0)
            {
                ChooseButton.interactable = true;
                ChooseButton.GetComponentInChildren<TMP_Text>().text = Characters[index].Cost + "C";

                if (Coins >= Characters[index].Cost)
                {
                    ChooseButton.GetComponent<Image>().color = Color.green;
                }
                else
                {
                    ChooseButton.GetComponent<Image>().color = Color.red;
                }
            }
            else
            {
                ChooseButton.GetComponent<Image>().color = Color.white;
                ChooseButton.interactable = true;
                ChooseButton.GetComponentInChildren<TMP_Text>().text = "Choose";
            }

        }
        previewCharacters[index].SetActive(true);//активируем текущий скин
        NameText.text = Characters[index].Name;
        Characters[0].purchased = 1;//Обчыный скин уже куплен
        

        for(int i = 0; i < Characters.Count; i++)//проверяем куплены ли скины
        {
            if (PlayerPrefs.HasKey("purchased" + i))
            {
                Characters[i].purchased = PlayerPrefs.GetInt("purchased" + i);
            }
        }


        if (!PlayerPrefs.HasKey("Name"))//Если у нас нет имени выходим
        {
            return;
        }

        string defaultName = PlayerPrefs.GetString("Name");//Получаем имя, выбираем его и выводим
        NameInput.text = defaultName;
        DisplayName = defaultName;
        SetName(defaultName);
    }

    private void Update()
    {
        if (!InGame)//Если не в игре
        {
            Player[] players = FindObjectsOfType<Player>();
            for(int i = 0; i < players.Length; i++)//Всех персонажей уменьшаем
            {
                players[i].gameObject.transform.localScale = Vector3.zero;
            }

            if (FirstTime == 1)//Если зашли в первыц раз выбираем имя
            {
                JoinInput.interactable = false;
                for (int i = 0; i < buttons.Length; i++)
                {
                    buttons[i].interactable = false;
                }
                ChangeNamePanel.SetActive(true);
                CloseButton.SetActive(false);
            }
            else//Иначе назначаем цену за изменение имени
            {
                SetNameButton.GetComponentInChildren<TMP_Text>().text = ChangeNameCost + "C";
                CloseButton.SetActive(true);

                if (Coins >= ChangeNameCost)
                {
                    SetNameButton.GetComponent<Image>().color = Color.green;
                }
                else
                {
                    SetNameButton.GetComponent<Image>().color = Color.red;
                }
            }
            CoinsText.text = Coins + "C";

            PlayerPrefs.SetInt("Coins", Coins);//Выводим количество монет
        }
    }

    public void SetName(string name)
    {
        //При наборе проверям не совпадает ли имяс предыдщум и не пустой ли инпт филд
        if (name == DisplayName || string.IsNullOrEmpty(name))
        {
            SetNameButton.interactable = false;
        }
        else
        {
            SetNameButton.interactable = true;
        }
    }

    public void SaveName()
    {
        //Сохраняем имя
        if (FirstTime == 0)//если не в первый раз
        {
            if (Coins >= ChangeNameCost)//Провереям хватает ли денег
            {
                Coins -= ChangeNameCost;//вычитаем
                JoinInput.interactable = false;
                for (int i = 0; i < buttons.Length; i++)
                {
                    buttons[i].interactable = false;
                }

                FirstTime = 0;//Ставим что уже не в первый раз
                //заносим данный в префы и отключаем холст выбора персонажа
                ChangeNamePanel.SetActive(false);
                DisplayName = NameInput.text;
                PlayerPrefs.SetInt("FirstTime", FirstTime);
                PlayerPrefs.SetString("Name", DisplayName);
                Invoke(nameof(Disconect), 1f);//Перезаходим
            }
            else//Не хватает денег
            {
                JoinInput.interactable = false;
                for (int i = 0; i < buttons.Length; i++)
                {
                    buttons[i].interactable = false;
                }
                ErrorPanel.SetActive(true);
                ErrorText.text = "Not enough money";
            }
        }
        else//Если в первый раз
        {
            ChangeNamePanel.SetActive(false);
            JoinInput.interactable = false;
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].interactable = false;
            }

            FirstTime = 0;

            DisplayName = NameInput.text;
            PlayerPrefs.SetInt("FirstTime", FirstTime);
            PlayerPrefs.SetString("Name", DisplayName);
            Invoke(nameof(Disconect), 1f);
        }    
    }

    public void Choose()//Выбор цвета
    {
        if (Characters[index].purchased == 0)//Если не куплен
        {
            if (Coins >= Characters[index].Cost)//и денег юольше чем стоит
            {
                //Вычитаем деньги, сохраняем префы
                Coins -= Characters[index].Cost;
                Characters[index].purchased = 1;//выставляем что скин куплен
                PlayerPrefs.SetInt("purchased" + index, Characters[index].purchased);
                PlayerPrefs.SetInt("index", index);
                JoinInput.interactable = false;
                for (int i = 0; i < buttons.Length; i++)
                {
                    buttons[i].interactable = false;
                }
                Invoke(nameof(Disconect), 1f);//перезаходим
            }
            else//ошибка не хватает денег
            {
                JoinInput.interactable = false;
                for (int i = 0; i < buttons.Length; i++)
                {
                    buttons[i].interactable = false;
                }
                ErrorPanel.SetActive(true);
                ErrorText.text = "Not enough money";
            }
        }
        else// если куплен выбираем цвет и перезаходим
        {
            PlayerPrefs.SetInt("index", index);
            JoinInput.interactable = false;
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].interactable = false;
            }
            Invoke(nameof(Disconect), 1f);
        }
        
    }

    public void ChangeIndex(bool previous)//выбор скина
    {
        previewCharacters[index].SetActive(false);//Деактивируем выбранный скин
        //перемещение по скинам
        if (!previous)
        {
            index = (index + 1) % previewCharacters.Count;
        }
        else
        {
            index--;
            if (index < 0)
            {
                index += previewCharacters.Count;
            }
        }
        //если выбран скин, деактивируем кнопку и меням текст на выбран
        if (index == PlayerPrefs.GetInt("index"))
        {
            ChooseButton.GetComponent<Image>().color = Color.white;
            ChooseButton.interactable = false;
            ChooseButton.GetComponentInChildren<TMP_Text>().text = "Chosen";
        }
        else
        {
            if (Characters[index].purchased == 0)//если скин не куплен
            {
                ChooseButton.interactable = true;//интерактивность кнопки
                ChooseButton.GetComponentInChildren<TMP_Text>().text = Characters[index].Cost + "C";//стоимость

                if(Coins >= Characters[index].Cost)//если можно купить, цвет кнопки зеленный
                {
                    ChooseButton.GetComponent<Image>().color = Color.green;
                }
                else//если нельзя красный
                {
                    ChooseButton.GetComponent<Image>().color = Color.red;
                }
            }
            else//если куплен то можно выбрать
            {
                ChooseButton.GetComponent<Image>().color = Color.white;
                ChooseButton.interactable = true;
                ChooseButton.GetComponentInChildren<TMP_Text>().text = "Choose";
            }
            
        }
        previewCharacters[index].SetActive(true);
        NameText.text = Characters[index].Name;
    }

    public void Disconect()//Отключение
    {
        if (_networkManager.mode == NetworkManagerMode.Host)//Если хост останавливаем хост
        {
            _networkManager.StopHost();
        }
        else if(_networkManager.mode == NetworkManagerMode.ClientOnly)//если клиент останавливаем клиент
        {
            _networkManager.StopClient();
        }
    }

    public void SetBeginButtonActive(bool active)//Активация кнопки начала
    {
        BeginGameButton.interactable = active;
    }

    public void Host(bool publicHost)//кнопка хоста игры
    {
        //отключаем все кнопки
        JoinInput.interactable = false;
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }

        Player.localPlayer.HostGame(publicHost);//Отправляем запрос на хост
    }

    public void HostSuccess(bool success, string matchID)//при удачном хостинге
    {
        if (success)
        {
            LobbyCanvas.enabled = true;//Включаем холст лобби и создаем интерфес лобби с именами

            if(localPlayerLobbyUI != null)
            {
                Destroy(localPlayerLobbyUI);
            }

            localPlayerLobbyUI = SpawnPlayerUIPrefab(Player.localPlayer);
            IDText.text = matchID;//выводим айди матча
            BeginGameButton.interactable = true;//Активируем кнопку старта
        }
        else//если неудачно выводим ошибку
        {
            ErrorPanel.SetActive(true);
            ErrorText.text = "Create lobby is fault";

        }
    }

    public void Join()//Кнопка подключения
    {
        //деактивируем все кнопки
        JoinInput.interactable = false;
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }


        Player.localPlayer.JoinGame(JoinInput.text.ToUpper());//Отправляем айди матча с инпутфилда
    }

    public void JoinSuccess(bool success, string matchID)//При удачном подключении
    {
        if (success)
        {
            LobbyCanvas.enabled = true;//активация холста лобби

            if (localPlayerLobbyUI != null)
            {
                Destroy(localPlayerLobbyUI);//если окно лобби уже есть, удалить
            }

            localPlayerLobbyUI = SpawnPlayerUIPrefab(Player.localPlayer);//и создать новое с присоединившимся игроком
            IDText.text = matchID;//Выводим матч айди
            BeginGameButton.interactable = false;//Деактивируем кнопку начала
        }
        else
        {
            ErrorPanel.SetActive(true);
            ErrorText.text = "Can not found ID";

        }
    }

    public void Enable()//Включение всех кнопок
    {
        ErrorPanel.SetActive(false);//И выключение окна ошибки
        for(int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = true;
        }
        JoinInput.interactable = true;
    }

    public void DisconnectGame()//дисконект от игры
    {
        if (localPlayerLobbyUI != null)
        {
            Destroy(localPlayerLobbyUI);//Удалаяем UI, ели он есть
        }

        Player.localPlayer.DisconnectGame();//Отключаемся
        LobbyCanvas.enabled = false;//Отключаем холст лобби и активируем кнопки
        JoinInput.interactable = true;
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = true;
        }
    }

    public bool HostGame(string matchID, GameObject player, bool publicMatch)//Хост игры
    {
        if (!matcheIDs.Contains(matchID))//проверяем чтобы не был занят айди
        {
            matcheIDs.Add(matchID);//добавляем в общую базу айдишников
            Match match = new Match(matchID, player, publicMatch);//Создаем новый матч
            matches.Add(match);//добавляем в общий пул матчей
            player.GetComponent<Player>().CurrentMatch = match;//Для хоста назначаем текущий матч
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool JoinGame(string matchID, GameObject player)//Присоеденение к игре
    {
        if (matcheIDs.Contains(matchID))//если в пуле айдишников есть айди по которому присоединяюсь
        {
            for(int i = 0; i < matches.Count; i++)
            {
                if (matches[i].ID == matchID)//Ищем по айди матчей нужный айди
                {
                    if (!matches[i].InMatch && !matches[i].MatchFull)//И если не в матче и лобби не заполнено
                    {
                        //Присоединяем игроков
                        matches[i].PlayersInMatch++;
                        matches[i].players.Add(player);
                        player.GetComponent<Player>().CurrentMatch = matches[i];
                        matches[i].players[0].GetComponent<Player>().PlayerCountUpdated(matches[i].players.Count);
                        if (matches[i].players.Count == MaxPlayers)//Если максимум игроков достигнут, лобби заполнено
                        {
                            matches[i].MatchFull = true;
                        }
                        break;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool SearchGame(GameObject player, out string ID)//поиск игры
    {
        ID = "";

        for (int i = 0; i < matches.Count; i++)
        {
            Debug.Log("Check ID " + matches[i].ID + " | in game " + matches[i].InMatch + " | full lobby " + matches[i].MatchFull + " | public lobby " + matches[i].PublicMatch);
            if (!matches[i].InMatch && !matches[i].MatchFull && matches[i].PublicMatch)//Проверяем матчи, если есть доступный присоединяемся
            {
                if (JoinGame(matches[i].ID, player))
                {
                    ID = matches[i].ID;
                    return true;
                }
            }
        }

        return false;
    }

    public static string GetRandomId()//Генерация рандомного айди
    {
        string ID = string.Empty;
        for(int i = 0; i < 5; i++)
        {
            int rand = UnityEngine.Random.Range(0, 36);
            if (rand < 26)
            {
                ID += (char)(rand + 65);
            }
            else
            {
                ID += (rand - 26).ToString();
            }
        }
        return ID;
    } 

    public GameObject SpawnPlayerUIPrefab(Player player)//Создание имени игрока в лобби
    {
        GameObject newUIPlayer = Instantiate(UIPlayerPrefab, UILayerParent);
        newUIPlayer.GetComponent<PlayerUI>().SetPlayer(player.PlayerDisplayName);

        return newUIPlayer;
    }

    public void StartGame()//Кнопка старта игры
    {
        foreach(var match in matches)
        {
            Debug.Log("Players in Match: " + match.PlayersInMatch);
        }
        Player.localPlayer.BeginGame();
    }

    public void SearchGame()//Кнопка поиска игры
    {
        StartCoroutine(Searching());
    }

    public void CancelSearchGame()//Отмена поиска игры
    {
        JoinInput.interactable = true;
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = true;
        }

        _searching = false;
    }

    public void SearchGameSuccess(bool success, string ID)//Игра найдена
    {
        if (success)
        {
            SearchCanvas.enabled = false;//Отключить полотно поиска
            _searching = false;//остановить поиск
            JoinSuccess(success, ID);//Подключиться к игре
        }
    }

    public void BeginGame(string matchID)//Для всех игроков в одном матча вызываем старт игры
    {
        for(int i = 0; i < matches.Count; i++)
        {
            if (matches[i].ID == matchID)
            {
                matches[i].InMatch = true;
                foreach(var player in matches[i].players)
                {
                    player.GetComponent<Player>().StartGame();
                }
                break;
            }
        }
    }

    public void PlayerDisconnected(GameObject player, string ID)//убираем игроков по матч айди
    {
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].ID == ID)
            {
                int playerIndex = matches[i].players.IndexOf(player);
                if (matches[i].players.Count > playerIndex)
                {
                    matches[i].players.RemoveAt(playerIndex);
                }

                if (matches[i].players.Count == 0)
                {
                    matches.RemoveAt(i);
                    matcheIDs.Remove(ID);
                }
                else
                {
                    matches[i].players[0].GetComponent<Player>().PlayerCountUpdated(matches[i].players.Count);
                }
                break;
            }
        }
    }

    IEnumerator Searching()//Поиск лобби
    {
        //отключаем кнопки
        JoinInput.interactable = false;
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }
        SearchCanvas.enabled = true;//холст поиска
        _searching = true;//в поиске

        float searchInterval = 1;
        float currentTime = 1;

        while (_searching)//раз в секунду проводим поиск, пока не найдем или не отменим
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
            }
            else
            {
                currentTime = searchInterval;
                Player.localPlayer.SearchGame();
            }
            yield return null;
        }
        SearchCanvas.enabled = false;
    }


    public void LoseGame()//Вызов холста проигрыша
    {
        LoseScreen.SetActive(true);
    }

    public void WinGame()//вызов холта выирыша
    {
        WinScreen.SetActive(true);

    }
}

public static class MatchExtention//Guid для MatchID
{
    public static Guid ToGuid(this string id)
    {
        MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
        byte[] inputBytes = Encoding.Default.GetBytes(id);
        byte[] hasBytes = provider.ComputeHash(inputBytes);

        return new Guid(hasBytes);
    }
}

[System.Serializable]
public class Character//Имя скина, цвет, цена
{
    public string Name;
    public GameObject PreviewObj;
    public int ColorKode;

    [Space(5)]
    public int Cost;

    [HideInInspector] public int purchased;
    
}
