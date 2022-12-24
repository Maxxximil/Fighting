using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    #region
    private static UIManager _instance;

    public static UIManager Instance
    {
        get
        {
            return _instance;
        }
    }
    #endregion


    [SerializeField] private GameObject _spawnGroup;
    [SerializeField] private Text _playerName;


    private void Awake()
    {
        _instance = this;
    }
    public void SpawnGroupToogle()
    {
        _spawnGroup.SetActive(!_spawnGroup.activeSelf);
    }

    public void ChangePlayerText(string name)
    {
        _playerName.text = "Your Name is " + name;
    }


}
