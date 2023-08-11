using StreamerReborn;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTrans4Test : MonoBehaviour
{

    private Rect buttonRect1;
    private Rect buttonRect2;

    void Start()
    {
        // ����������ť��λ�úʹ�С
        buttonRect1 = new Rect(10, Screen.height - 60, 100, 50);
        buttonRect2 = new Rect(10, Screen.height - 120, 100, 50);
    }

    // Update is called once per frame
    void OnGUI()
    {
        // ������һ����ť
        if (GUI.Button(buttonRect1, "������Դ���"))
        {
            GameStatic.UIManager.HideAllByGroup(0);
            
            GameStatic.MyGameManager.GameWorld.EnterHall();
        }
    }
}
