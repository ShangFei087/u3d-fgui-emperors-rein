using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

/// <summary>
/// 动态创建UIPanel
/// </summary>
public class Test : MonoBehaviour
{
    // Start is called before the first frame update


    // Update is called once per frame
    void Update()
    {
        
    }

    private void Start()
    {

        ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Console/FGUIs", (bundle) =>
        {
            UIPackage.AddPackage(bundle);


            FairyGUI.UIPanel panel = this.gameObject.AddComponent<FairyGUI.UIPanel>();// UnityEngine.UIPanel同名，因此要前缀 

            panel.packageName = "Console";

            panel.componentName = "PageConsoleMain";

            #region 非必要设置

            //设置 renderMode的方式
            panel.container.renderMode = RenderMode.ScreenSpaceOverlay;

            //设置 fairyBatching的方式
            panel.container.fairyBatching = true;

            //设置 sortingorder的方式
            panel.SetSortingOrder(1, true);

            //设置 hitTestMode的方式
            panel.SetHitTestMode(HitTestMode.Default);

            // 最后，创建出UI
            panel.CreateUI();
            #endregion //非必要设置 


        });
    }

    private void Start01()
    {

        #region 加载依赖的包


        UIPackage.AddPackage("UI/Console");//文件路径Assets\Resources\UI\Console_fui.bytes文件，FGUI编辑器 "
        //UIPackage.AddPackage("tavern/tavern");//文件路径
        
        #endregion //加载依赖的包

        FairyGUI.UIPanel panel = this.gameObject.AddComponent<FairyGUI.UIPanel>();// UnityEngine.UIPanel同名，因此要前缀 
            
        panel.packageName = "Console";

        panel.componentName = "PageConsoleMain"; 

        #region 非必要设置

        //设置 renderMode的方式
        panel.container.renderMode = RenderMode.ScreenSpaceOverlay; 
        
        //设置 fairyBatching的方式
        panel.container.fairyBatching = true; 
        
        //设置 sortingorder的方式
        panel.SetSortingOrder(1, true); 

        //设置 hitTestMode的方式
        panel.SetHitTestMode(HitTestMode.Default);

        // 最后，创建出UI
        panel.CreateUI(); 
        #endregion //非必要设置 
}
}
