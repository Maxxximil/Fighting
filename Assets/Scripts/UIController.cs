using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Контролеер для пользовательского интерфейса
public class UIController : MonoBehaviour
{
    public static UIController Instance;

    public GameObject LoseScreen;
    public GameObject ControllButtons;

    //кнопка прыжка
   public void JumpButton()
    {
        Player.localPlayer.Jump();
    }

    //передвижение
    public void MoveButton(float move)
    {
        Player.localPlayer.Move(move);
    }
    //кнопка атаки
    public void AtackButton()
    {
        Player.localPlayer.Atack();
    }
}
