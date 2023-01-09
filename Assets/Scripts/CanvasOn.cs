using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Активация канваса главного меню
public class CanvasOn : MonoBehaviour
{
    public GameObject Canvas;

    private void Start()
    {
        Canvas.SetActive(true);
    }
}
