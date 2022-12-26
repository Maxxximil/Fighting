using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
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
}
