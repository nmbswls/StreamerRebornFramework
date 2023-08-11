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
        // 设置两个按钮的位置和大小
        buttonRect1 = new Rect(10, Screen.height - 60, 100, 50);
        buttonRect2 = new Rect(10, Screen.height - 120, 100, 50);
    }

    // Update is called once per frame
    void OnGUI()
    {
        // 创建第一个按钮
        if (GUI.Button(buttonRect1, "进入测试大厅"))
        {
            GameStatic.UIManager.HideAllByGroup(0);
            
            GameStatic.MyGameManager.GameWorld.EnterHall();
        }
    }
}
