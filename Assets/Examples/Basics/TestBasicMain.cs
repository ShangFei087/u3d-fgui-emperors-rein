using FairyGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class TestBasicMain : MonoBehaviour
{
    void Awake()
    {
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
        //Use the font names directly
        UIConfig.defaultFont = "Microsoft YaHei";
#else
        //Need to put a ttf file into Resources folder. Here is the file name of the ttf file.
        UIConfig.defaultFont = "afont";
#endif
        UIPackage.AddPackage("UI/Basics");

        UIConfig.verticalScrollBar = "ui://Basics/ScrollBar_VT";
        UIConfig.horizontalScrollBar = "ui://Basics/ScrollBar_HZ";
        UIConfig.popupMenu = "ui://Basics/PopupMenu";
        UIConfig.buttonSound = (NAudioClip)UIPackage.GetItemAsset("Basics", "click");
    }
    void Start()
    {
        Application.targetFrameRate = 60;

    }

    // Update is called once per frame
    void Update()
    {
        
    }




    private Window _winA;
    private Window _winB;

    [Button]
    private void PlayWindow1()
    {

            if (_winB == null)
                _winB = new Window2();
            _winB.Show();
       
    }

    [Button]
    private void PlayWindow2()
    {
            if (_winA == null)
                _winA = new Window1();
            _winA.Show();

    }
}
