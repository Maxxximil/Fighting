using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//���������� ��� ����������������� ����������
public class UIController : MonoBehaviour
{
    public static UIController Instance;

    public GameObject LoseScreen;
    public GameObject ControllButtons;

    //������ ������
   public void JumpButton()
    {
        Player.localPlayer.Jump();
    }

    //������������
    public void MoveButton(float move)
    {
        Player.localPlayer.Move(move);
    }
    //������ �����
    public void AtackButton()
    {
        Player.localPlayer.Atack();
    }
}
