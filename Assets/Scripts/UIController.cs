using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    public GameObject LoseScreen;
    public GameObject ControllButtons;

   
    public void LoseScreenEnable()
    {
        ControllButtons.SetActive(false);
        LoseScreen.SetActive(true);
    }

   public void JumpButton()
    {
        Player.localPlayer.Jump();
    }

    public void LeftButton()
    {
        Player.localPlayer.Left();
    }

    public void RightButton()
    {
        Player.localPlayer.Right();
    }

    public void MoveButton(float move)
    {
        Player.localPlayer.Move(move);
    }

    public void AtackButton()
    {
        Player.localPlayer.Atack();
    }
}
