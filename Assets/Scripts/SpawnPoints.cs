using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Точки спавна
public class SpawnPoints : MonoBehaviour
{
    private void Awake()
    {
        SpawnPoints[] spawnPoints = FindObjectsOfType<SpawnPoints>();

        if (spawnPoints.Length > 1)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
}
