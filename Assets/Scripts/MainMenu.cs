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
public class Match
{
    public string ID;
    public bool PublicMatch;
    public bool InMatch;
    public bool MatchFull;

    public List<GameObject> players = new List<GameObject>();

    public Match(string ID, GameObject player, bool publicMatch)
    {
        InMatch = false;
        MatchFull = false;
        this.ID = ID;
        PublicMatch = publicMatch;
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


    [Header("Lobby")]
    public GameObject ErrorPanel;
    public TMP_Text ErrorText;

    [Header("Character")]
    public Button ChooseButton;
    public Transform PreviewParent;
    public TMP_Text NameText;
    public List<Character> Characters;
    private int index;
    private List<GameObject> previewCharacters = new List<GameObject>();

    public GameObject Fireball;

    private void Start()
    {
        Instanse = this;

        _networkManager = FindObjectOfType<NetworkManager>();

        FirstTime = PlayerPrefs.GetInt("FirstTime", 1);


        if (PlayerPrefs.HasKey("index"))
        {
            index = PlayerPrefs.GetInt("index");
        }

        foreach(var character in Characters)
        {
            GameObject previewCharacter = Instantiate(character.PreviewObj, PreviewParent);
            previewCharacter.SetActive(false);
            previewCharacters.Add(previewCharacter);
        }

        //if (index == PlayerPrefs.GetInt("index"))
        //{
        //    ChooseButton.interactable = false;
        //    ChooseButton.GetComponentInChildren<TMP_Text>().text = "Chosen";
        //}
        //else
        //{
        //    ChooseButton.interactable = true;
        //    ChooseButton.GetComponentInChildren<TMP_Text>().text = "Choose";
        //}
        previewCharacters[index].SetActive(true);
        NameText.text = Characters[index].Name;

        if (FirstTime == 1)
        {
            JoinInput.interactable = false;
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].interactable = false;
            }
            ChangeNamePanel.SetActive(true);
            CloseButton.SetActive(false);
        }
        else
        {
            CloseButton.SetActive(true);
        }

        if (!PlayerPrefs.HasKey("Name"))
        {
            return;
        }

        string defaultName = PlayerPrefs.GetString("Name");
        NameInput.text = defaultName;
        DisplayName = defaultName;
        SetName(defaultName);
    }

    private void Update()
    {
        if (!InGame)
        {
            Player[] players = FindObjectsOfType<Player>();
            for(int i = 0; i < players.Length; i++)
            {
                players[i].gameObject.transform.localScale = Vector3.zero;
            }

            //if (FirstTime == 1)
            //{
            //    JoinInput.interactable = false;
            //    for (int i = 0; i < buttons.Length; i++)
            //    {
            //        buttons[i].interactable = false;
            //    }
            //    ChangeNamePanel.SetActive(true);
            //    CloseButton.SetActive(false);
            //}
            //else
            //{
            //    CloseButton.SetActive(true);
            //}
            //if (PlayerPrefs.HasKey("Name"))
            //{
            //    FirstTime = 0;
            //}
            
        }
    }

    public void SetName(string name)
    {
        if (name == DisplayName || !string.IsNullOrEmpty(name))
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
        JoinInput.interactable = false;
        for(int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }

        FirstTime = 0;

        ChangeNamePanel.SetActive(false);
        DisplayName = NameInput.text;
        PlayerPrefs.SetInt("FirstTime", FirstTime);
        PlayerPrefs.SetString("Name", DisplayName);
        Invoke(nameof(Disconect), 1f);
    }

    public void Choose()
    {
        PlayerPrefs.SetInt("index", index);
        JoinInput.interactable = false;
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }
        Invoke(nameof(Disconect), 1f);
    }

    public void ChangeIndex(bool previous)
    {
        previewCharacters[index].SetActive(false);

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

        if (index == PlayerPrefs.GetInt("index"))
        {
            ChooseButton.interactable = false;
            ChooseButton.GetComponentInChildren<TMP_Text>().text = "Chosen";
        }
        else
        {
            ChooseButton.interactable = true;
            ChooseButton.GetComponentInChildren<TMP_Text>().text = "Choose";
        }
        previewCharacters[index].SetActive(true);
        NameText.text = Characters[index].Name;
    }

    public void Disconect()
    {
        if (_networkManager.mode == NetworkManagerMode.Host)
        {
            _networkManager.StopHost();
        }
        else if(_networkManager.mode == NetworkManagerMode.ClientOnly)
        {
            _networkManager.StopClient();
        }
    }

    public void SetBeginButtonActive(bool active)
    {
        BeginGameButton.interactable = active;
    }

    public void Host(bool publicHost)
    {
        JoinInput.interactable = false;
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }

        Player.localPlayer.HostGame(publicHost);
    }

    public void HostSuccess(bool success, string matchID)
    {
        if (success)
        {
            LobbyCanvas.enabled = true;

            if(localPlayerLobbyUI != null)
            {
                Destroy(localPlayerLobbyUI);
            }

            localPlayerLobbyUI = SpawnPlayerUIPrefab(Player.localPlayer);
            IDText.text = matchID;
            BeginGameButton.interactable = true;
        }
        else
        {
            ErrorPanel.SetActive(true);
            ErrorText.text = "Create lobby is fault";

        }
    }

    public void Join()
    {
        JoinInput.interactable = false;
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }


        Player.localPlayer.JoinGame(JoinInput.text.ToUpper());
    }

    public void JoinSuccess(bool success, string matchID)
    {
        if (success)
        {
            LobbyCanvas.enabled = true;

            if (localPlayerLobbyUI != null)
            {
                Destroy(localPlayerLobbyUI);
            }

            localPlayerLobbyUI = SpawnPlayerUIPrefab(Player.localPlayer);
            IDText.text = matchID;
            BeginGameButton.interactable = false;
        }
        else
        {
            ErrorPanel.SetActive(true);
            ErrorText.text = "Can not found ID";

        }
    }

    public void Enable()
    {
        ErrorPanel.SetActive(false);
        for(int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = true;
        }
        JoinInput.interactable = true;
    }

    public void DisconnectGame()
    {
        if (localPlayerLobbyUI != null)
        {
            Destroy(localPlayerLobbyUI);
        }

        Player.localPlayer.DisconnectGame();
        LobbyCanvas.enabled = false;
        JoinInput.interactable = true;
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = true;
        }
    }

    public bool HostGame(string matchID, GameObject player, bool publicMatch)
    {
        if (!matcheIDs.Contains(matchID))
        {
            matcheIDs.Add(matchID);
            Match match = new Match(matchID, player, publicMatch);
            matches.Add(match);
            player.GetComponent<Player>().CurrentMatch = match;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool JoinGame(string matchID, GameObject player)
    {
        if (matcheIDs.Contains(matchID))
        {
            for(int i = 0; i < matches.Count; i++)
            {
                if (matches[i].ID == matchID)
                {
                    if (!matches[i].InMatch && !matches[i].MatchFull)
                    {
                        matches[i].players.Add(player);
                        player.GetComponent<Player>().CurrentMatch = matches[i];
                        matches[i].players[0].GetComponent<Player>().PlayerCountUpdated(matches[i].players.Count);
                        if (matches[i].players.Count == MaxPlayers)
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

    public bool SearchGame(GameObject player, out string ID)
    {
        ID = "";

        for (int i = 0; i < matches.Count; i++)
        {
            Debug.Log("Check ID " + matches[i].ID + " | in game " + matches[i].InMatch + " | full lobby " + matches[i].MatchFull + " | public lobby " + matches[i].PublicMatch);
            if (!matches[i].InMatch && !matches[i].MatchFull && matches[i].PublicMatch)
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

    public static string GetRandomId()
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

    public GameObject SpawnPlayerUIPrefab(Player player)
    {
        GameObject newUIPlayer = Instantiate(UIPlayerPrefab, UILayerParent);
        newUIPlayer.GetComponent<PlayerUI>().SetPlayer(player.PlayerDisplayName);

        return newUIPlayer;
    }

    public void StartGame()
    {
        Player.localPlayer.BeginGame();
    }

    public void SearchGame()
    {
        StartCoroutine(Searching());
    }

    public void CancelSearchGame()
    {
        JoinInput.interactable = true;
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = true;
        }

        _searching = false;
    }

    public void SearchGameSuccess(bool success, string ID)
    {
        if (success)
        {
            SearchCanvas.enabled = false;
            _searching = false;
            JoinSuccess(success, ID);
        }
    }

    public void BeginGame(string matchID)
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

    public void PlayerDisconnected(GameObject player, string ID)
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

    IEnumerator Searching()
    {
        JoinInput.interactable = false;
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }
        SearchCanvas.enabled = true;
        _searching = true;

        float searchInterval = 1;
        float currentTime = 1;

        while (_searching)
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

    public void SpawnFirebal(string matchID, Vector3 pos, uint owner, Vector3 target)
    {
        GameObject newFireBall = Instantiate(Fireball,pos,Quaternion.identity);
        NetworkServer.Spawn(newFireBall);
        newFireBall.GetComponent<NetworkMatch>().matchId = matchID.ToGuid();
        newFireBall.GetComponent<Bullet>().Init(owner, target);
    }
}

public static class MatchExtention
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
public class Character
{
    public string Name;
    public GameObject PreviewObj;
    public int ColorKode;
    public Sprite[] Sprites;
    
}
